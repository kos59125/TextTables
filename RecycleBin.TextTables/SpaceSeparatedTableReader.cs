using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RecycleBin.TextTables
{
   /// <summary>
   /// Represents a reader for space-separated table.
   /// </summary>
   [Serializable]
   public class SpaceSeparatedTableReader : TextTableReader
   {
      private const int EndOfStream = -1;
      private const int WhiteSpace = ' ';

      private readonly TextReader reader;
      private readonly string newline;
      private readonly int newlineFirst;
      private readonly char quotation;

      /// <summary>
      /// Initializes a new <see cref="CsvReader"/> with the specified path to reading file.
      /// </summary>
      /// <param name="reader">The reader.</param>
      /// <param name="settings">The settings.</param>
      public SpaceSeparatedTableReader(TextReader reader, SpaceSeparatedTableReaderSettings settings = null)
      {
         if (reader == null)
         {
            throw new ArgumentNullException("reader");
         }
         settings = settings ?? new SpaceSeparatedTableReaderSettings();
         this.reader = reader;
         this.newline = settings.RecordDelimiter.AsNewline();
         this.newlineFirst = (int)this.newline[0];
         this.quotation = settings.QuotationCharacter;
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

         int nextCharacter = this.reader.Read();
         if (nextCharacter == EndOfStream)
         {
            return ReadState.EndOfStream;
         }

         switch (currentState)
         {
            case ReadState.Default:
               while (nextCharacter != WhiteSpace && nextCharacter != this.newlineFirst)
               {
                  if (nextCharacter == EndOfStream)
                  {
                     return ReadState.EndOfStream;
                  }
                  field.Append((char)nextCharacter);
                  nextCharacter = this.reader.Read();
               }
               if (CheckNewline(nextCharacter))
               {
                  return ReadState.EndOfRecord;
               }
               if (nextCharacter == WhiteSpace)
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
               while (nextCharacter == WhiteSpace)
               {
                  nextCharacter = this.reader.Read();
               }
               if (nextCharacter == this.quotation)
               {
                  return ReadState.Quoted;
               }
               if (CheckNewline(nextCharacter))
               {
                  return ReadState.EndOfRecord;
               }
               field.Append((char)nextCharacter);
               return ReadState.Default;
            case ReadState.Escaped:
               if (nextCharacter == this.quotation)
               {
                  field.Append(this.quotation);
                  return ReadState.Quoted;
               }
               if (CheckNewline(nextCharacter))
               {
                  return ReadState.EndOfRecord;
               }
               if (nextCharacter == WhiteSpace)
               {
                  return ReadState.EndOfField;
               }
               throw new FormatException();
         }
         throw new InvalidOperationException();
      }

      private bool CheckNewline(int firstCharacter)
      {
         // CR or LF
         if (firstCharacter == this.newlineFirst && this.newline.Length == 1)
         {
            return true;
         }
         // CRLF
         if (this.reader.Peek() == '\n')
         {
            this.reader.Read();
            return true;
         }
         return false;
      }
   }
}
