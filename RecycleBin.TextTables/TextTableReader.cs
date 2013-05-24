using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RecycleBin.TextTables
{
   /// <summary>
   /// Represents a reader that parses tabular data in plain-text form.
   /// </summary>
   [Serializable]
   public abstract class TextTableReader : IEnumerator<Record>, IDisposable
   {
      private readonly Record currentRecord;
      private readonly StringBuilder fieldBuffer;
      private ReadState state;

      /// <summary>
      /// Gets the current record.
      /// </summary>
      public Record Current
      {
         get { return this.currentRecord; }
      }

      object IEnumerator.Current
      {
         get { return this.currentRecord; }
      }

      /// <summary>
      /// Initializes a new instance.
      /// </summary>
      protected TextTableReader()
      {
         this.currentRecord = new Record();
         this.fieldBuffer = new StringBuilder();
         this.state = ReadState.EndOfRecord;
      }

      /// <summary>
      /// Sets the specified array as a header.
      /// </summary>
      /// <returns>The header.</returns>
      public void SetHeader(string[] header)
      {
         this.currentRecord.SetHeader(header);
      }

      /// <summary>
      /// Handles the next row as a header and advances the current position.
      /// </summary>
      /// <returns>The header.</returns>
      public void HandleHeaderRow()
      {
         if (!MoveNext())
         {
            throw new InvalidOperationException("There is no row to read.");
         }
         var header = this.currentRecord.ToArray();
         this.currentRecord.SetHeader(header);
      }

      private string ReadField()
      {
         if (this.state == ReadState.Closed)
         {
            throw new ObjectDisposedException("Stream has been closed.");
         }
         if (this.state == ReadState.EndOfStream)
         {
            return null;
         }
         this.fieldBuffer.Length = 0;
         do
         {
            this.state = Succeed(this.state, this.fieldBuffer);
         } while (state <= ReadState.Escaped);
         return this.fieldBuffer.ToString();
      }

      /// <summary>
      /// Succeeds the state of the reader.
      /// </summary>
      /// <returns><c>True</c> if more records are available; otherwise, <c>False</c>.</returns>
      public bool MoveNext()
      {
         if (this.state == ReadState.Closed)
         {
            throw new ObjectDisposedException("Stream has been closed.");
         }
         if (this.state == ReadState.EndOfStream)
         {
            return false;
         }
         this.currentRecord.Clear();
         do
         {
            var field = ReadField();
            if (field == null)
            {
               break;
            }
            this.currentRecord.Add(field);
         } while (state <= ReadState.EndOfField);
         return this.state != ReadState.EndOfStream || this.currentRecord.FieldCount != 1 || !string.IsNullOrEmpty(this.currentRecord[0]);
      }

      /// <summary>
      /// Advances a state of the reader.
      /// </summary>
      /// <param name="state">A current state.</param>
      /// <param name="buffer">A buffer.</param>
      /// <returns>A new state.</returns>
      protected abstract ReadState Succeed(ReadState state, StringBuilder buffer);


      /// <summary>
      /// Reads records to the end of the underlying stream.
      /// </summary>
      /// <returns>The records.</returns>
      public IEnumerable<string[]> ReadToEndRaw()
      {
         while (MoveNext())
         {
            yield return this.currentRecord.ToArray();
         }
      }

      /// <summary>
      /// Reads records to the end of the underlying stream.
      /// </summary>
      /// <typeparam name="TRecord">The type of records.</typeparam>
      /// <returns>The records.</returns>
      public IEnumerable<TRecord> ReadToEnd<TRecord>()
      {
         while (MoveNext())
         {
            yield return this.currentRecord.Convert<TRecord>();
         }
      }

      /// <summary>
      /// Unused.
      /// </summary>
      public virtual void Reset()
      {
      }

      /// <summary>
      /// Disposes the reader.
      /// </summary>
      public void Close()
      {
         Dispose(true);
      }

      /// <summary>
      /// Disposes the reader.
      /// </summary>
      public void Dispose()
      {
         Dispose(true);
      }

      /// <summary>
      /// Disposes the reader.
      /// </summary>
      /// <param name="disposing">Determines disposing unmanaged resources.</param>
      protected virtual void Dispose(bool disposing)
      {
         this.state = ReadState.Closed;
      }

      /// <summary>
      /// Represents a position of a reader.
      /// </summary>
      protected enum ReadState
      {
         /// <summary>
         /// Unquoted field.
         /// </summary>
         Default,

         /// <summary>
         /// Quoted field.
         /// </summary>
         Quoted,

         /// <summary>
         /// Escaped character.
         /// </summary>
         Escaped,

         /// <summary>
         /// End of a field.
         /// </summary>
         EndOfField,

         /// <summary>
         /// End of a record.
         /// </summary>
         EndOfRecord,

         /// <summary>
         /// End of a stream.
         /// </summary>
         EndOfStream,

         /// <summary>
         /// Stream has been closed.
         /// </summary>
         Closed,
      }
   }
}
