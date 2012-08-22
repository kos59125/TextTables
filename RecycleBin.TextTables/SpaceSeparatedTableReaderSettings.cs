using System;

namespace RecycleBin.TextTables
{
   /// <summary>
   /// Specifies specification of <see cref="CsvReader"/>.
   /// </summary>
   [Serializable]
   public class SpaceSeparatedTableReaderSettings
   {
      /// <summary>
      /// Initializes a new <see cref="CsvReaderSettings"/>.
      /// </summary>
      public SpaceSeparatedTableReaderSettings()
      {
         RecordDelimiter = EndOfLine.CRLF;
         QuotationCharacter = '"';
      }

      /// <summary>
      /// Gets or sets the delimiter of records.
      /// </summary>
      public EndOfLine RecordDelimiter { get; set; }

      /// <summary>
      /// Gets or sets the character of quotation.
      /// </summary>
      public char QuotationCharacter { get; set; }
   }
}
