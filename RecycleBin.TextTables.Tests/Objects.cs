using System;
using System.Collections.Generic;

namespace RecycleBin.TextTables
{
   public class PartialRecord
   {
      public object IgnoredField;

      [Column(1)]
      public int IntField;

      [Column("double1", NullString = "NA")]
      public double? NullableDoubleField;

      [Column("parsed1", NullString = "NA", ParserType = typeof(Parser))]
      public int? ParsedField;

      [Column(0)]
      public string StringField;

      [Column("string2")]
      public string StringProperty { get; set; }

      [Column("int2")]
      public int IntProperty { get; set; }

      [Column(7, NullString = "NA")]
      public double? NullableDoubleProperty { get; set; }

      [Column(8, NullString = "NA", ParserType = typeof(Parser))]
      public int? ParsedProperty { get; set; }

      public int IgnoredProperty { get; set; }
   }

   public class FullRecord
   {
      [Column(1)]
      public int IntField;

      [Column("double1", NullString = "NA")]
      public double? NullableDoubleField;

      [Column("parsed1", NullString = "Undefined", ParserType = typeof(Parser), FormatterType = typeof(Formatter))]
      public int? ParsedField;

      [Column(0)]
      public string StringField;

      [Column("string2")]
      public string StringProperty { get; set; }

      [Column("int2")]
      public int IntProperty { get; set; }

      [Column(6, NullString = "NA(Double)")]
      public double? NullableDoubleProperty { get; set; }

      [Column(7, NullString = "NA(Int32)", ParserType = typeof(Parser), FormatterType = typeof(Formatter))]
      public int? ParsedProperty { get; set; }
   }

   public class MultipleColumnRecord
   {
      [Column(0)]
      [Column(1, ParserType = typeof(Parser), FormatterType = typeof(Formatter))]
      public int Value { get; set; }
   }

   public class ArrayRecord
   {
      private int[,] property = new int[3, 2];
      [Column(0, ArrayIndex = new[] { 0, 0 })]
      [Column(1, ArrayIndex = new[] { 0, 1 })]
      [Column(2, ArrayIndex = new[] { 1, 0 })]
      [Column(3, ArrayIndex = new[] { 1, 1 })]
      [Column(4, ArrayIndex = new[] { 2, 0 })]
      [Column(5, ArrayIndex = new[] { 2, 1 })]
      public int[,] Property { get { return property; } internal set { this.property = value; } }

      [Column(6, ArrayIndex = new[] { 0 }, FormatterType = typeof(Formatter), ParserType = typeof(Parser))]
      [Column(7, ArrayIndex = new[] { 1 }, FormatterType = typeof(Formatter), ParserType = typeof(Parser))]
      public int[] Field = new int[2];
   }

   public class IndexerRecord
   {
      private readonly Dictionary<string, int> dictionary = new Dictionary<string, int>();

      [Column(0, PropertyIndex = new object[] { "A" })]
      [Column(1, PropertyIndex = new object[] { "B" }, FormatterType = typeof(Formatter), ParserType = typeof(Parser))]
      [Column(2, PropertyIndex = new object[] { "C" })]
      public int this[string key]
      {
         get
         {
            int value;
            this.dictionary.TryGetValue(key, out value);
            return value;
         }
         set
         {
            if (this.dictionary.ContainsKey(key))
            {
               this.dictionary[key] = value;
            }
            else
            {
               this.dictionary.Add(key, value);
            }
         }
      }
   }

   public class Parser : IParser
   {
      public object Parse(string value, IFormatProvider provider)
      {
         switch (value)
         {
            case "一":
               return 1;
            case "二":
               return 2;
            default:
               throw new ArgumentException("value");
         }
      }
   }

   public class Formatter : IFormatter
   {
      public string Format(object value, IFormatProvider provider)
      {
         switch (Convert.ToInt32(value))
         {
            case 1:
               return "一";
            case 2:
               return "二";
            default:
               throw new ArgumentException("value");
         }
      }
   }
}
