using System;
using System.IO;
using System.Text;

namespace RecycleBin.TextTables
{
   /// <summary>
   /// Represents a reader for tabular data separated by some special string, comma in paticular.
   /// </summary>
   /// <remarks>
   /// This reader class handles all newlines as '\n'.
   /// Consider to use <see cref="CsvReader"/> instead if your CSV fields may contain newlines.
   /// </remarks>
   [Serializable]
   public class FastCsvReader : TextTableReader
   {
      private const int EndOfStream = -1;
      private const int EndOfLine = '\n';

      private readonly TextReader reader;
      private readonly char quotation;
      private readonly char separator;
      /// <summary>
      /// The buffer to hold line.
      /// </summary>
      private string buffer;
      private int bufferIndex = 0;
      private bool disposed;

      /// <summary>
      /// Initializes a new <see cref="FastCsvReader"/> with the specified path to reading file.
      /// </summary>
      /// <param name="path">The path to file to read.</param>
      /// <param name="settings">The settings.</param>
      public FastCsvReader(string path, FastCsvReaderSettings settings = null)
         : this(new StreamReader(path), settings, false)
      {
      }

      /// <summary>
      /// Initializes a new <see cref="FastCsvReader"/> with the specified path to reading file.
      /// </summary>
      /// <param name="path">The path to file to read.</param>
      /// <param name="encoding">The encoding.</param>
      /// <param name="settings">The settings.</param>
      public FastCsvReader(string path, Encoding encoding, FastCsvReaderSettings settings = null)
         : this(new StreamReader(path, encoding), settings, false)
      {
      }

      /// <summary>
      /// Initializes a new <see cref="FastCsvReader"/> with the specified file stream.
      /// </summary>
      /// <param name="stream">The file stream.</param>
      /// <param name="settings">The settings.</param>
      public FastCsvReader(Stream stream, FastCsvReaderSettings settings = null)
         : this(new StreamReader(stream), settings, false)
      {
      }

      /// <summary>
      /// Initializes a new <see cref="FastCsvReader"/> with the specified file stream.
      /// </summary>
      /// <param name="stream">The file stream.</param>
      /// <param name="encoding">The encoding.</param>
      /// <param name="settings">The settings.</param>
      public FastCsvReader(Stream stream, Encoding encoding, FastCsvReaderSettings settings = null)
         : this(new StreamReader(stream, encoding), settings, false)
      {
      }

      /// <summary>
      /// Initializes a new <see cref="FastCsvReader"/> with the specified path to reading file.
      /// </summary>
      /// <param name="reader">The reader.</param>
      /// <param name="settings">The settings.</param>
      public FastCsvReader(TextReader reader, FastCsvReaderSettings settings = null)
         : this(reader, settings, true)
      {
      }

      private FastCsvReader(TextReader reader, FastCsvReaderSettings settings, bool disposed)
      {
         if (reader == null)
         {
            throw new ArgumentNullException("reader");
         }
         settings = settings ?? new FastCsvReaderSettings();
         this.reader = reader;
         this.separator = settings.FieldDelimiter;
         this.quotation = settings.QuotationCharacter;
         this.disposed = disposed;
         FillBuffer();
      }

      private void FillBuffer()
      {
         this.buffer = this.reader.ReadLine();
         this.bufferIndex = 0;
      }

      private int Read()
      {
         if (buffer == null)
         {
            return EndOfStream;
         }
         if (bufferIndex >= buffer.Length)
         {
            FillBuffer();
            return buffer == null ? EndOfStream : EndOfLine;
         }
         else
         {
            return buffer[bufferIndex++];
         }
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

         int nextCharacter = Read();
         if (nextCharacter == EndOfStream)
         {
            return ReadState.EndOfStream;
         }

         switch (currentState)
         {
            case ReadState.Default:
               while (nextCharacter != this.separator && nextCharacter != EndOfLine)
               {
                  if (nextCharacter == EndOfStream)
                  {
                     return ReadState.EndOfStream;
                  }
                  field.Append((char)nextCharacter);
                  nextCharacter = Read();
               }
               if (nextCharacter == EndOfLine)
               {
                  return ReadState.EndOfRecord;
               }
               if (nextCharacter == this.separator)
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
                  nextCharacter = Read();
               }
               return ReadState.Escaped;
            case ReadState.EndOfRecord:
            case ReadState.EndOfField:
               if (nextCharacter == this.quotation)
               {
                  return ReadState.Quoted;
               }
               if (nextCharacter == EndOfLine)
               {
                  return ReadState.EndOfRecord;
               }
               if (nextCharacter == this.separator)
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
               if (nextCharacter == EndOfLine)
               {
                  return ReadState.EndOfRecord;
               }
               if (nextCharacter == this.separator)
               {
                  return ReadState.EndOfField;
               }
               throw new FormatException();
         }
         throw new InvalidOperationException();
      }

      /// <inheritDoc />
      protected override void Dispose(bool disposing)
      {
         if (!this.disposed)
         {
            this.reader.Dispose();
            this.disposed = true;
         }
         base.Dispose(disposing);
      }
   }
}

