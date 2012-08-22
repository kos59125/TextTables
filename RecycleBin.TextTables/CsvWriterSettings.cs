using System;

namespace RecycleBin.TextTables
{
   /// <summary>
   /// Specifies specification of <see cref="CsvWriter"/>.
   /// </summary>
   [Serializable]
   public class CsvWriterSettings
   {
      private string fieldDelimiter;

      /// <summary>
      /// Initializes a new <see cref="CsvWriterSettings"/>.
      /// </summary>
      public CsvWriterSettings()
      {
         FieldDelimiter = ",";
         RecordDelimiter = EndOfLine.CRLF;
         QuotationCharacter = '"';
         QuoteField = true;
      }

      /// <summary>
      /// Gets or sets the delimiter of fields.
      /// </summary>
      public string FieldDelimiter
      {
         get { return this.fieldDelimiter; }
         set
         {
            if (value == null)
            {
               throw new ArgumentNullException("value");
            }
            if (value.Length == 0)
            {
               throw new ArgumentException("No character is contained in the specified delimiter.", "value");
            }
            this.fieldDelimiter = value;
         }
      }

      /// <summary>
      /// Gets or sets the delimiter of records.
      /// </summary>
      public EndOfLine RecordDelimiter { get; set; }

      /// <summary>
      /// Gets or sets the character of quotation.
      /// </summary>
      public char QuotationCharacter { get; set; }

      /// <summary>
      /// Gets or sets the value indicating whether all fields should be quoted.
      /// </summary>
      public bool QuoteField { get; set; }
   }
}
