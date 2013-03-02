using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RecycleBin.TextTables
{
   /// <summary>
   /// Represents a writer of tabular data in plain-text form.
   /// </summary>
   [Serializable]
   public abstract class TextTableWriter : IDisposable
   {
      private readonly Dictionary<Type, List<Tuple<GetValue, ColumnAttribute>>> typeCache;
      private string[] header;

      /// <summary>
      /// Initializes a new instance.
      /// </summary>
      protected TextTableWriter()
      {
         this.typeCache = new Dictionary<Type, List<Tuple<GetValue, ColumnAttribute>>>();
      }

      /// <summary>
      /// Gets or sets a value indicating whether the writer will flush its buffer to the underlying stream after every call to write a record.
      /// </summary>
      public bool AutoFlush { get; set; }

      /// <summary>
      /// Clears all buffers for the current writer and causes any buffered data to be written to the underlying stream.
      /// </summary>
      public abstract void Flush();

      /// <summary>
      /// Sets the header of the table.
      /// </summary>
      /// <param name="header">The header.</param>
      /// <seealso cref="WriteHeader"/>
      public void SetHeader(string[] header)
      {
         this.header = header;
      }

      /// <summary>
      /// Sets the header of the table.
      /// </summary>
      /// <param name="recordType">The type of records.</param>
      /// <seealso cref="WriteHeader"/>
      public void SetHeader(Type recordType)
      {
         if (recordType == null)
         {
            throw new ArgumentNullException("recordType");
         }
         this.header = GetMembers(recordType).Select(tuple => tuple.Item2.Name ?? "").ToArray();
      }

      /// <summary>
      /// Writes the header of the table.
      /// </summary>
      /// <exception cref="InvalidOperationException">No header is specified.</exception>
      /// <seealso cref="SetHeader(string[])"/>
      /// <seealso cref="SetHeader(Type)"/>
      public void WriteHeader()
      {
         if (this.header == null)
         {
            throw new InvalidOperationException("Header is undefined.");
         }
         WriteRecordRaw(this.header);
      }

      /// <summary>
      /// Writes the specified record to the current stream.
      /// </summary>
      /// <param name="record">The record to write.</param>
      public void WriteRecordRaw(string[] record)
      {
         if (record == null)
         {
            throw new ArgumentNullException("record");
         }
         OnStartRecord();
         for (var index = 0; index < record.Length; index++)
         {
            var field = record[index];
            OnStartField(field, index);
            WriteField(field, index);
            OnEndField(field, index);
         }
         OnEndRecord();

         if (AutoFlush)
         {
            Flush();
         }
      }

      /// <summary>
      /// Writes the specified record to the current stream.
      /// </summary>
      /// <typeparam name="TRecord">The type of the record.</typeparam>
      /// <param name="record">The record.</param>
      public void WriteRecord<TRecord>(TRecord record)
      {
         Type recordType = typeof(TRecord);
         var values = from tuple in GetMembers(recordType)
                      let value = tuple.Item1(record)
                      select tuple.Item2.Format(value);
         WriteRecordRaw(values.ToArray());
      }

      /// <summary>
      /// Specifies the action before writing a record.
      /// </summary>
      protected virtual void OnStartRecord() { }

      /// <summary>
      /// Writes the field.
      /// </summary>
      /// <param name="field">The field.</param>
      /// <param name="index">The field index.</param>
      protected abstract void WriteField(string field, int index);

      /// <summary>
      /// Specifies the action after writing a record.
      /// </summary>
      protected virtual void OnEndRecord() {}

      /// <summary>
      /// Specifies the action before writing a field.
      /// </summary>
      /// <param name="field">The field to write.</param>
      /// <param name="index">The index of the field.</param>
      protected virtual void OnStartField(string field, int index) {}

      /// <summary>
      /// Specifies the action after writing a field.
      /// </summary>
      /// <param name="field">The field to write.</param>
      /// <param name="index">The index of the field.</param>
      protected virtual void OnEndField(string field, int index) {}

      private IEnumerable<Tuple<GetValue, ColumnAttribute>> GetMembers(Type recordType)
      {
         List<Tuple<GetValue, ColumnAttribute>> members;
         if (!this.typeCache.TryGetValue(recordType, out members))
         {
            var properties = from property in recordType.GetProperties()
                             let attributes = property.GetCustomAttributes(typeof(ColumnAttribute), true)
                             from attribute in attributes.Cast<ColumnAttribute>()
                             let getProperty = attribute.GenerateGetValue(property)
                             select Tuple.Create(getProperty, attribute);
            var fields = from field in recordType.GetFields()
                         let attributes = field.GetCustomAttributes(typeof(ColumnAttribute), true)
                         from attribute in attributes.Cast<ColumnAttribute>()
                         let getField = attribute.GenerateGetValue(field)
                         select Tuple.Create(getField, attribute);
            members = properties.Concat(fields).ToList();
            ArrangeMembersByIndex(members);
            this.typeCache.Add(recordType, members);
         }
         return members;
      }

      private void ArrangeMembersByIndex(IList<Tuple<GetValue, ColumnAttribute>> members)
      {
         var count = members.Count;
         var indexed = (from tuple in members
                        group tuple by tuple.Item2.GetIndex(this.header)
                        into indexedTuple
                        where 0 <= indexedTuple.Key && indexedTuple.Key < count
                        where indexedTuple.Count() == 1
                        orderby indexedTuple.Key
                        select indexedTuple).ToDictionary(indexedTuple => indexedTuple.Key, indexedTuple => indexedTuple.Single());
         var nonIndexed = new Queue<Tuple<GetValue, ColumnAttribute>>(from tuple in members
                                                                      where tuple.Item2.GetIndex(this.header) < 0
                                                                      orderby tuple.Item2.Name
                                                                      select tuple);
         if (indexed.Count + nonIndexed.Count != count)
         {
            throw new InvalidOperationException("Invalid index.");
         }
         for (var index = 0; index < count; index++)
         {
            if (indexed.ContainsKey(index))
            {
               members[index] = indexed[index];
            }
            else
            {
               members[index] = nonIndexed.Dequeue();
            }
         }
      }

      /// <summary>
      /// Closes the writer.
      /// </summary>
      public void Close()
      {
         Dispose(true);
      }

      /// <summary>
      /// Disposes the writer.
      /// </summary>
      public void Dispose()
      {
         Dispose(true);
      }

      /// <summary>
      /// Disposes the writer.
      /// </summary>
      /// <param name="disposing">Determines disposing unmanaged resources.</param>
      protected virtual void Dispose(bool disposing) {}
   }
}
