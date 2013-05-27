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
      private readonly Dictionary<Type, Parse> parserCache;
      private readonly Dictionary<Type, Format> formatterCache;

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
      /// Gets or sets a value which indicates the format of numbers.
      /// </summary>
      /// <remarks>
      /// This value is ignored when <see cref="ParserType"/> is not <c>null</c>.
      /// </remarks>
      public NumberStyles NumberStyle { get; set; }

      /// <summary>
      /// Gets or sets a value which indicates the format of <see cref="System.DateTime"/>.
      /// </summary>
      /// <remarks>
      /// This value is ignored when <see cref="ParserType"/> is not <c>null</c>.
      /// </remarks>
      public DateTimeStyles DateTimeStyle { get; set; }

      /// <summary>
      /// Gets or sets a parser of the column.
      /// </summary>
      /// <returns>The parser.</returns>
      public Type ParserType { get; set; }

      /// <summary>
      /// Gets or sets a string which specifies output format.
      /// </summary>
      /// <remarks>
      /// This value is ignored when <see cref="FormatterType"/> is not <c>null</c>.
      /// </remarks>
      public string FormatString { get; set; }

      /// <summary>
      /// Gets or sets a formatter of the column.
      /// </summary>
      /// <returns>The formatter.</returns>
      public Type FormatterType { get; set; }

      /// <summary>
      /// Initializes a new instance.
      /// </summary>
      public ValueAttribute()
      {
         this.parserCache = new Dictionary<Type, Parse>();
         this.formatterCache = new Dictionary<Type, Format>();
         NumberStyle = NumberStyles.Any;
         DateTimeStyle = DateTimeStyles.None;
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
         var parserType = ParserType ?? CreateParserType(memberType);
         var parse = CreateOrGetParse(parserType);
         return parse(value, culture);
      }

      private Type CreateParserType(Type type)
      {
         if (type.IsArray && ArrayIndex != null)
         {
            type = type.GetElementType();
         }
         if (type.IsGenericTypeOf(typeof(Nullable<>)))
         {
            type = type.GetGenericArguments()[0];
         }
         if (type.IsGenericTypeOf(typeof(Lazy<>)))
         {
            var innerType = type.GetGenericArguments()[0];
            type = typeof(LazyParser<>).MakeGenericType(innerType);
         }
         return type;
      }

      private Parse CreateOrGetParse(Type parserType)
      {
         Parse parse;
         if (!this.parserCache.TryGetValue(parserType, out parse))
         {
            if (parserType.IsGenericTypeOf(typeof(LazyParser<>)))
            {
               parse = GenerateParseLazy(parserType, this);
            }
            else
            {
               parse = GenerateParse(parserType, FormatString, NumberStyle, DateTimeStyle);
               if (parse == null)
               {
                  var message = string.Format("Cannot find any way to convert field to member type {0}.", parserType.FullName);
                  throw new NotSupportedException(message);
               }
            }
            this.parserCache.Add(parserType, parse);
         }
         return parse;
      }

      private static Parse GenerateParseLazy(Type lazyParserType, ValueAttribute attribute)
      {
         var parse = typeof(IParser).GetMethod("Parse", new[] { typeof(string), typeof(IFormatProvider) });
         var instance = Activator.CreateInstance(lazyParserType, attribute);
         return (value, provider) => parse.Invoke(instance, new object[] { value, provider });
      }

      private static Parse GenerateParse(Type type, string format, NumberStyles numberStyle, DateTimeStyles datetimeStyle)
      {
         if (type.IsEnum)
         {
            return (value, _) => Enum.Parse(type, value, true);
         }
         var typeCode = Type.GetTypeCode(type);
         switch (typeCode)
         {
            case TypeCode.Boolean:
               return (value, _) => Boolean.Parse(value);
            case TypeCode.Byte:
               return (value, provider) => Byte.Parse(value, numberStyle, provider);
            case TypeCode.Char:
               return (value, _) => Char.Parse(value);
            case TypeCode.DateTime:
               if (format != null)
               {
                  return (value, provider) => DateTime.ParseExact(value, format, provider, datetimeStyle);
               }
               return (value, provider) => DateTime.Parse(value, provider, datetimeStyle);
            case TypeCode.Decimal:
               return (value, provider) => Decimal.Parse(value, numberStyle, provider);
            case TypeCode.Double:
               return (value, provider) => Double.Parse(value, numberStyle, provider);
            case TypeCode.Empty:
               return null;
            case TypeCode.Int16:
               return (value, provider) => Int16.Parse(value, numberStyle, provider);
            case TypeCode.Int32:
               return (value, provider) => Int32.Parse(value, numberStyle, provider);
            case TypeCode.Int64:
               return (value, provider) => Int64.Parse(value, numberStyle, provider);
            case TypeCode.SByte:
               return (value, provider) => SByte.Parse(value, numberStyle, provider);
            case TypeCode.Single:
               return (value, provider) => Single.Parse(value, numberStyle, provider);
            case TypeCode.String:
               return (value, _) => value;
            case TypeCode.UInt16:
               return (value, provider) => UInt16.Parse(value, numberStyle, provider);
            case TypeCode.UInt32:
               return (value, provider) => UInt32.Parse(value, numberStyle, provider);
            case TypeCode.UInt64:
               return (value, provider) => UInt64.Parse(value, numberStyle, provider);
            default:
               return GenerateParseObject(type, format);
         }
      }

      private static Parse GenerateParseObject(Type parserType, string format)
      {
         Parse parse = null;
         if (format != null)
         {
            // * TimeSpan.ParseExact(string, string, IFormatProvider, TimeSpanStyles)
            // * DateTimeOffset.ParseExact(string, string, IFormatProvider, DateTimeStyles)
            // are not supported for the following reasons:
            // 1) ParseExact(string, string, IFormatProvider) is caught by FindParseExact in the next line.
            // 2) ParserType is availabe for such a complex parsing task.
            parse = FindParseExact(parserType, format);
            if (parse != null)
            {
               return parse;
            }
         }
         parse = FindParse(parserType);
         if (parse != null)
         {
            return parse;
         }
         throw new ArgumentException(string.Format("ParserType '{0}' does not have Parse(String [, IFormatProvider]) method or ParseExact(String, String [, IFormatProvider]).", parserType.FullName));
      }

      private static Parse FindParseExact(Type parserType, string format)
      {
         var parse = parserType.GetMethod("ParseExact", new[] { typeof(string), typeof(string), typeof(IFormatProvider) });
         if (parse != null)
         {
            var parser = parse.IsStatic ? null : Activator.CreateInstance(parserType);
            return (value, provider) => parse.Invoke(parser, new object[] { value, format, provider });
         }
         parse = parserType.GetMethod("ParseExact", new[] { typeof(string), typeof(string) });
         if (parse != null)
         {
            var parser = parse.IsStatic ? null : Activator.CreateInstance(parserType);
            return (value, _) => parse.Invoke(parser, new object[] { value, format });
         }
         return null;
      }

      private static Parse FindParse(Type parserType)
      {
         var parse = parserType.GetMethod("Parse", new[] { typeof(string), typeof(IFormatProvider) });
         if (parse != null)
         {
            var parser = parse.IsStatic ? null : Activator.CreateInstance(parserType);
            return (value, provider) => parse.Invoke(parser, new object[] { value, provider });
         }
         parse = parserType.GetMethod("Parse", new[] { typeof(string) });
         if (parse != null)
         {
            var parser = parse.IsStatic ? null : Activator.CreateInstance(parserType);
            return (value, _) => parse.Invoke(parser, new object[] { value });
         }
         return null;
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
            var format = FormatString == null ? "{0}" : ("{0:" + FormatString + "}");
            return string.Format(culture, format, value);
         }
         else
         {
            var valueType = value.GetType();
            Format format;
            if (!this.formatterCache.TryGetValue(FormatterType, out format))
            {
               format = GenerateFormat(FormatterType);
               this.formatterCache.Add(FormatterType, format);
            }
            return format(value, culture);
         }
      }

      private static Format GenerateFormat(Type formatterType)
      {
         var format = formatterType.GetMethod("Format", new[] { typeof(object), typeof(IFormatProvider) });
         if (format != null && format.ReturnType == typeof(string))
         {
            var formatter = format.IsStatic ? null : Activator.CreateInstance(formatterType);
            return (value, provider) => (string)format.Invoke(formatter, new[] { value, provider });
         }
         format = formatterType.GetMethod("Format", new[] { typeof(object) });
         if (format != null && format.ReturnType == typeof(string))
         {
            var formatter = format.IsStatic ? null : Activator.CreateInstance(formatterType);
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
