using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RecycleBin.TextTables
{
   /// <summary>
   /// Represents a record.
   /// </summary>
   public class Record : IEnumerable<string>
   {
      private static readonly string[] Empty = new string[0];

      private readonly List<string> record;
      private readonly Dictionary<Type, List<Tuple<SetValue, ColumnAttribute, Type>>> typeCache;
      private string[] header;

      /// <summary>
      /// Gets the field specified by the given index.
      /// </summary>
      /// <param name="index">The field index.</param>
      /// <returns>The field.</returns>
      public string this[int index]
      {
         get { return this.record[index]; }
      }

      /// <summary>
      /// Gets the field specified by the given name.
      /// </summary>
      /// <param name="name">The field name.</param>
      /// <returns>The field.</returns>
      public string this[string name]
      {
         get
         {
            if (name == null)
            {
               throw new ArgumentNullException("name");
            }
            if (this.header == null)
            {
               throw new InvalidOperationException("Header is not defined.");
            }
            int index = Array.IndexOf(this.header ?? Empty, name);
            if (index < 0)
            {
               throw new KeyNotFoundException();
            }
            return this.record[index];
         }
      }

      /// <summary>
      /// Gets the number of fields in the current record.
      /// </summary>
      public int FieldCount
      {
         get { return this.record.Count; }
      }

      internal Record()
      {
         this.record = new List<string>();
         this.header = null;
         this.typeCache = new Dictionary<Type, List<Tuple<SetValue, ColumnAttribute, Type>>>();
      }

      /// <summary>
      /// Reads a record in the specified type.
      /// </summary>
      /// <typeparam name="TRecord">The type of record.</typeparam>
      /// <returns>The record.</returns>
      /// <seealso cref="ColumnAttribute"/>
      public TRecord Convert<TRecord>()
      {
         var recordType = typeof(TRecord);
         if (recordType.IsNullable())
         {
            recordType = recordType.GetGenericArguments()[0];
         }
         var prototype = Activator.CreateInstance(recordType);
         SetValues(recordType, ref prototype);
         return (TRecord)prototype;
      }

      /// <summary>
      /// Reads a record in the specified type.
      /// </summary>
      /// <param name="recordType">The record type.</param>
      /// <param name="prototype">The record to store values.</param>
      /// <seealso cref="ColumnAttribute"/>
      public void Convert(Type recordType, ref object prototype)
      {
         SetValues(recordType, ref prototype);
      }

      private void SetValues(Type recordType, ref object prototype)
      {
         List<Tuple<SetValue, ColumnAttribute, Type>> members;
         if (!this.typeCache.TryGetValue(recordType, out members))
         {
            var properties = from property in recordType.GetProperties()
                             let attributes = property.GetCustomAttributes(typeof(ColumnAttribute), true)
                             from attribute in attributes.Cast<ColumnAttribute>()
                             let setProperty = attribute.GenerateSetValue(property)
                             select Tuple.Create(setProperty, attribute, property.PropertyType);
            var fields = from field in recordType.GetFields()
                         let attributes = field.GetCustomAttributes(typeof(ColumnAttribute), true)
                         from attribute in attributes.Cast<ColumnAttribute>()
                         let setField = attribute.GenerateSetValue(field)
                         select Tuple.Create(setField, attribute, field.FieldType);
            members = properties.Concat(fields).ToList();
            this.typeCache.Add(recordType, members);
         }

         foreach (var tuple in members)
         {
            var setValue = tuple.Item1;
            var attribute = tuple.Item2;
            var type = tuple.Item3;
            var index = attribute.GetIndex(this.header);
            var value = attribute.Parse(this[index], type);
            setValue(prototype, value);
         }
      }

      internal void SetHeader(string[] header)
      {
         this.header = header;
      }

      internal void Add(string field)
      {
         this.record.Add(field);
      }

      internal void Clear()
      {
         this.record.Clear();
      }

      /// <inheritDoc />
      public IEnumerator<string> GetEnumerator()
      {
         return this.record.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return this.record.GetEnumerator();
      }
   }
}
