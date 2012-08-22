using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace RecycleBin.TextTables
{
   /// <summary>
   /// Provides metainformation about relationship between
   /// </summary>
   [Serializable]
   [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
   public class ValueAttribute : Attribute
   {
      private readonly Dictionary<Type, Func<string, IFormatProvider, object>> parserCache;
      private readonly Dictionary<Type, Func<object, IFormatProvider, string>> formatterCache;

      /// <summary>
      /// Gets or sets the zero-based index in an array.
      /// </summary>
      /// <returns>The zero-based index.</returns>
      public int[] ArrayIndex { get; set; }

      /// <summary>
      /// Gets or sets the property index.
      /// </summary>
      /// <returns>The property index.</returns>
      public object[] PropertyIndex { get; set; }

      /// <summary>
      /// Gets or sets the value to regard as <c>Nothing</c>.
      /// </summary>
      /// <returns>The string representation of null value.</returns>
      public string NullString { get; set; }

      /// <summary>
      /// Gets or sets a language culture name.
      /// </summary>
      /// <returns>The language culture name.</returns>
      public string CultureName { get; set; }

      /// <summary>
      /// Gets or sets a parser of the column.
      /// </summary>
      /// <returns>The parser.</returns>
      public Type ParserType { get; set; }

      /// <summary>
      /// Gets or sets a formatter of the column.
      /// </summary>
      /// <returns>The formatter.</returns>
      public Type FormatterType { get; set; }

      public ValueAttribute()
      {
         this.parserCache = new Dictionary<Type, Func<string, IFormatProvider, object>>();
         this.formatterCache = new Dictionary<Type, Func<object, IFormatProvider, string>>();
      }

      /// <summary>
      /// Parses a string and converts it into an object.
      /// </summary>
      /// <param name="value">The string value.</param>
      /// <param name="memberType">The type to convert.</param>
      /// <returns>The object value.</returns>
      public object Parse(string value, Type memberType)
      {
         if (value == NullString)
         {
            return null;
         }
         var culture = CultureName == null ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(CultureName);
         if (ParserType == null)
         {
            var type = memberType;
            if (type.IsArray && ArrayIndex != null)
            {
               type = type.GetElementType();
            }
            if (type.IsNullable() && ParserType == null)
            {
               type = type.GetGenericArguments()[0];
            }
            return ParsePrimitive(type, value, culture);
         }
         else
         {
            Func<string, IFormatProvider, object> parse;
            if (!this.parserCache.TryGetValue(ParserType, out parse))
            {
               parse = GenerateParse(ParserType);
               this.parserCache.Add(ParserType, parse);
            }
            return parse(value, culture);
         }
      }

      private static object ParsePrimitive(Type type, string value, IFormatProvider provider)
      {
         var typeCode = Type.GetTypeCode(type);
         switch (typeCode)
         {
            case TypeCode.Boolean:
               return Boolean.Parse(value);
            case TypeCode.Byte:
               return Byte.Parse(value, provider);
            case TypeCode.Char:
               return Char.Parse(value);
            case TypeCode.DateTime:
               return DateTime.Parse(value, provider);
            case TypeCode.Decimal:
               return Decimal.Parse(value, provider);
            case TypeCode.Double:
               return Double.Parse(value, provider);
            case TypeCode.Empty:
               return null;
            case TypeCode.Int16:
               return Int16.Parse(value, provider);
            case TypeCode.Int32:
               return Int32.Parse(value, provider);
            case TypeCode.Int64:
               return Int64.Parse(value, provider);
            case TypeCode.SByte:
               return SByte.Parse(value, provider);
            case TypeCode.Single:
               return Single.Parse(value, provider);
            case TypeCode.String:
               return value;
            case TypeCode.UInt16:
               return UInt16.Parse(value, provider);
            case TypeCode.UInt32:
               return UInt32.Parse(value, provider);
            case TypeCode.UInt64:
               return UInt64.Parse(value, provider);
            default:
               throw new NotSupportedException(string.Format("Type code {0} is not supported.", typeCode));
         }
      }

      private static Func<string, IFormatProvider, object> GenerateParse(Type parserType)
      {
         var parser = Activator.CreateInstance(parserType);
         var parse = parserType.GetMethod("Parse", new[] { typeof(string), typeof(IFormatProvider) });
         if (parse != null)
         {
            return (value, provider) => parse.Invoke(parser, new object[] { value, provider });
         }
         parse = parserType.GetMethod("Parse", new[] { typeof(string) });
         if (parse != null)
         {
            return (value, _) => parse.Invoke(parser, new object[] { value });
         }
         throw new ArgumentException(string.Format("ParserType '{0}' does not have Parse(String [, IFormatProvider]) method.", parserType.FullName));
      }

      /// <summary>
      /// Parses an object and converts it into an string.
      /// </summary>
      /// <param name="value">The object value.</param>
      /// <returns>The string representation.</returns>
      public string Format(object value)
      {
         if (value == null)
         {
            return NullString ?? "";
         }
         var culture = CultureName == null ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(CultureName);
         if (FormatterType == null)
         {
            return string.Format(culture, "{0}", value);
         }
         else
         {
            var valueType = value.GetType();
            Func<object, IFormatProvider, string> format;
            if (!this.formatterCache.TryGetValue(FormatterType, out format))
            {
               format = GenerateFormat(FormatterType);
               this.formatterCache.Add(FormatterType, format);
            }
            return format(value, culture);
         }
      }

      private static Func<object, IFormatProvider, string> GenerateFormat(Type formatterType)
      {
         var formatter = Activator.CreateInstance(formatterType);
         var format = formatterType.GetMethod("Format", new[] { typeof(object), typeof(IFormatProvider) });
         if (format != null && format.ReturnType == typeof(string))
         {
            return (value, provider) => (string)format.Invoke(formatter, new[] { value, provider });
         }
         format = formatterType.GetMethod("Format", new[] { typeof(object) });
         if (format != null && format.ReturnType == typeof(string))
         {
            return (value, _) => (string)format.Invoke(formatter, new[] { value });
         }
         throw new ArgumentException(string.Format("FormatterType '{0}' does not have Format(Object [, IFormatProvider]) method.", formatterType.FullName));
      }

      internal GetValue GenerateGetValue(PropertyInfo property)
      {
         GetValue getValue = instance => property.GetValue(instance, PropertyIndex);
         var propertyType = property.PropertyType;
         if (propertyType.IsArray && ArrayIndex != null)
         {
            return instance =>
            {
               var array = getValue(instance) as Array;
               return array == null ? Activator.CreateInstance(propertyType.GetElementType()) : array.GetValue(ArrayIndex);
            };
         }
         return getValue;
      }

      internal GetValue GenerateGetValue(FieldInfo field)
      {
         GetValue getValue = field.GetValue;
         var fieldType = field.FieldType;
         if (fieldType.IsArray && ArrayIndex != null)
         {
            return instance =>
            {
               var array = getValue(instance) as Array;
               return array == null ? Activator.CreateInstance(fieldType.GetElementType()) : array.GetValue(ArrayIndex);
            };
         }
         return getValue;
      }

      internal SetValue GenerateSetValue(PropertyInfo property)
      {
         SetValue setValue = (instance, value) => property.SetValue(instance, value, PropertyIndex);
         var propertyType = property.PropertyType;
         if (propertyType.IsArray && ArrayIndex != null)
         {
            GetValue getValue = instance => property.GetValue(instance, null);
            return (instance, value) => ((Array)getValue(instance)).SetValue(value, ArrayIndex);
         }
         return setValue;
      }

      internal SetValue GenerateSetValue(FieldInfo field)
      {
         SetValue setValue = field.SetValue;
         var fieldType = field.FieldType;
         if (fieldType.IsArray && ArrayIndex != null)
         {
            GetValue getValue = field.GetValue;
            return (instance, value) => ((Array)getValue(instance)).SetValue(value, ArrayIndex);
         }
         return setValue;
      }
   }
}
