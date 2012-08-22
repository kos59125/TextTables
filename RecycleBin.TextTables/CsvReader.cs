using System;
using System.IO;
using System.Text;

namespace RecycleBin.TextTables
{
   /// <summary>
   /// Represents a reader for tabular data separated by some special string, comma in paticular.
   /// </summary>
   [Serializable]
   public class CsvReader : TextTableReader
   {
      private const int EndOfStream = -1;

      private readonly StackBufferedTextReader reader;
      private readonly string delimiter;
      private readonly char delimiterFirst;
      private readonly char quotation;
      private readonly string separator;
      private readonly char separatorFirst;
      /// <summary>
      /// The buffer to check special tokens.
      /// </summary>
      /// <seealso cref="CheckSpecialToken"/>
      private readonly char[] buffer;

      /// <summary>
      /// Initializes a new <see cref="CsvReader"/> with the specified path to reading file.
      /// </summary>
      /// <param name="reader">The reader.</param>
      /// <param name="settings">The settings.</param>
      public CsvReader(TextReader reader, CsvReaderSettings settings = null)
      {
         if (reader == null)
         {
            throw new ArgumentNullException("reader");
         }
         settings = settings ?? new CsvReaderSettings();
         this.reader = reader is StackBufferedTextReader ? (StackBufferedTextReader)reader : new StackBufferedTextReader(reader);
         this.delimiter = settings.RecordDelimiter.AsNewline();
         this.delimiterFirst = this.delimiter[0];
         this.separator = settings.FieldDelimiter;
         this.separatorFirst = this.separator[0];
         this.quotation = settings.QuotationCharacter;
         this.buffer = new char[Math.Max(this.delimiter.Length, this.separator.Length)];
      }

      /// <summary>
      /// Generates a next state from the current state.
      /// </summary>
      /// <param name="currentState">The current state.</param>
      /// <param name="field">The buffer containing a field value.</param>
      /// <returns>The next state.</returns>
      protected override ReadState Succeed(ReadState currentState, StringBuilder field)
      {
         if (currentState == ReadState.Closed)
         {
            throw new ObjectDisposedException("Stream has been closed.");
         }

         var nextCharacter = this.reader.Read();
         if (nextCharacter == EndOfStream)
         {
            return ReadState.EndOfStream;
         }

         switch (currentState)
         {
            case ReadState.Default:
               while (nextCharacter != this.delimiterFirst && nextCharacter != this.separatorFirst)
               {
                  if (nextCharacter == EndOfStream)
                  {
                     return ReadState.EndOfStream;
                  }
                  field.Append((char)nextCharacter);
                  nextCharacter = this.reader.Read();
               }
               if (CheckSpecialToken(nextCharacter, this.delimiter))
               {
                  return ReadState.EndOfRecord;
               }
               if (CheckSpecialToken(nextCharacter, this.separator))
               {
                  return ReadState.EndOfField;
               }
               field.Append((char)nextCharacter);
               return ReadState.Default;
            case ReadState.Quoted:
               while (nextCharacter != this.quotation)
               {
                  if (nextCharacter == EndOfStream)
                  {
                     throw new FormatException();
                  }
                  field.Append((char)nextCharacter);
                  nextCharacter = this.reader.Read();
               }
               return ReadState.Escaped;
            case ReadState.EndOfRecord:
            case ReadState.EndOfField:
               if (nextCharacter == this.quotation)
               {
                  return ReadState.Quoted;
               }
               if (CheckSpecialToken(nextCharacter, this.delimiter))
               {
                  return ReadState.EndOfRecord;
               }
               if (CheckSpecialToken(nextCharacter, this.separator))
               {
                  return ReadState.EndOfField;
               }
               field.Append((char)nextCharacter);
               return ReadState.Default;
            case ReadState.Escaped:
               if (nextCharacter == this.quotation)
               {
                  field.Append(this.quotation);
                  return ReadState.Quoted;
               }
               if (CheckSpecialToken(nextCharacter, this.delimiter))
               {
                  return ReadState.EndOfRecord;
               }
               if (CheckSpecialToken(nextCharacter, this.separator))
               {
                  return ReadState.EndOfField;
               }
               throw new FormatException();
         }
         throw new InvalidOperationException();
      }

      private bool CheckSpecialToken(int firstCharacter, string token)
      {
         if (firstCharacter != token[0])
         {
            return false;
         }
         for (var index = 1; index < token.Length; index++)
         {
            char c = (char)this.reader.Read();
            this.buffer[index] = c;
            if (c == token[index])
            {
               continue;
            }
            while (index >= 1)
            {
               this.reader.Push(this.buffer[index--]);
            }
            return false;
         }
         return true;
      }
   }
}
