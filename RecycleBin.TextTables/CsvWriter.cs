using System;
using System.IO;
using System.Linq;

namespace RecycleBin.TextTables
{
   /// <summary>
   /// Provides the <see cref="TextTableWriter"/> implementation for CSV.
   /// </summary>
   [Serializable]
   public class CsvWriter : TextTableWriter
   {
      private readonly string delimiter;
      private readonly char quotation;
      private readonly string separator;
      private readonly bool quoteField;
      private readonly string quotationString;
      private readonly string escapedQuotation;

      /// <summary>
      /// Initializes a new <see cref="CsvWriter"/> with the specified path to output.
      /// </summary>
      /// <param name="writer">The writer.</param>
      /// <param name="settings">The settings.</param>
      public CsvWriter(TextWriter writer, CsvWriterSettings settings = null)
         : base(writer)
      {
         settings = settings ?? new CsvWriterSettings();
         this.delimiter = settings.RecordDelimiter.AsNewline();
         this.separator = settings.FieldDelimiter;
         this.quotation = settings.QuotationCharacter;
         this.quotationString = settings.QuotationCharacter.ToString();
         this.quoteField = settings.QuoteField;
         this.escapedQuotation = this.quotationString + this.quotationString;
      }

      /// <summary>
      /// Outputs a newline.
      /// </summary>
      protected override void OnEndRecord()
      {
         Write(this.delimiter);
      }

      /// <summary>
      /// Outputs a separator (if needed) and a quotation.
      /// </summary>
      /// <param name="field">The field.</param>
      /// <param name="index">The index.</param>
      protected override void OnStartField(string field, int index)
      {
         if (index != 0)
         {
            Write(this.separator);
         }
      }

      /// <summary>
      /// Writes the field.
      /// </summary>
      /// <param name="field">The field.</param>
      /// <param name="index">The field index.</param>
      protected override void WriteField(string field, int index)
      {
         Write(Enquote(field));
      }

      private string Enquote(string field)
      {
         if (quoteField || field.Contains(this.delimiter) || field.Contains(this.separator) || field.Any(c => c == this.quotation))
         {
            return this.quotation + field.Replace(this.quotationString, this.escapedQuotation) + this.quotation;
         }
         else
         {
            return field;
         }
      }
   }
}
