using System;

namespace RecycleBin.TextTables
{
   /// <summary>
   /// Specifies specification of <see cref="CsvReader"/>.
   /// </summary>
   [Serializable]
   public class FastCsvReaderSettings
   {
      /// <summary>
      /// Initializes a new <see cref="CsvReaderSettings"/>.
      /// </summary>
      public FastCsvReaderSettings()
      {
         FieldDelimiter = ',';
         QuotationCharacter = '"';
      }

      /// <summary>
      /// Gets or sets the delimiter of fields.
      /// </summary>
      public char FieldDelimiter { get; set; }

      /// <summary>
      /// Gets or sets the character of quotation.
      /// </summary>
      public char QuotationCharacter { get; set; }
   }
}
