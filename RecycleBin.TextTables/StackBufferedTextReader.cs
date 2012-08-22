using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RecycleBin.TextTables
{
   /// <summary>
   /// Represents a <see cref="TextReader"/> that is able to push any characters to head position.
   /// </summary>
   public class StackBufferedTextReader : TextReader
   {
      private readonly Stack<char> charBuffer;
      private readonly TextReader internalReader;

      /// <summary>
      /// Initializes a new instance.
      /// </summary>
      /// <param name="reader">The underlying reader.</param>
      /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <c>Nothing</c>.</exception>
      public StackBufferedTextReader(TextReader reader)
      {
         if (reader == null)
         {
            throw new ArgumentNullException("reader");
         }
         this.internalReader = reader;
         this.charBuffer = new Stack<char>();
      }

      /// <summary>
      /// Removes all reverted characters.
      /// </summary>
      /// <remarks>
      /// Usually called when the underlying reader is changed outside of the current reader.
      /// </remarks>
      public void ClearBuffer()
      {
         this.charBuffer.Clear();
      }

      /// <summary>
      /// Reads the next character from the input stream and advances the character position by one character.
      /// </summary>
      /// <returns>The next character from the input stream, or -1 if no more characters are available.</returns>
      public override int Read()
      {
         return this.charBuffer.Count > 0 ? this.charBuffer.Pop() : this.internalReader.Read();
      }

      /// <summary>
      /// Reads a maximum of count characters from the current stream and writes the data to buffer, beginning at the specified index.
      /// </summary>
      /// <param name="buffer">The container to store the read data.</param>
      /// <param name="index">The position in <paramref name="buffer"/> at which to begin writing.</param>
      /// <param name="count">The maximum number of characters to read.</param>
      /// <returns>The number of characters that have been read, or 0 if at the end of the stream and no data was read.</returns>
      public override int Read(char[] buffer, int index, int count)
      {
         if (buffer == null)
         {
            throw new ArgumentNullException("buffer");
         }
         if (index < 0 || buffer.Length <= index)
         {
            throw new ArgumentOutOfRangeException("index");
         }
         if (index + count < buffer.Length)
         {
            throw new ArgumentException("The number of characters to read is greater than the available space from index to the end of the destination array.", "count");
         }
         var bufferedCount = this.charBuffer.Count;
         if (bufferedCount >= count)
         {
            var readCount = count;
            while (--count >= 0)
            {
               buffer[index++] = this.charBuffer.Pop();
            }
            return readCount;
         }
         if (bufferedCount > 0)
         {
            this.charBuffer.CopyTo(buffer, index);
            ClearBuffer();
         }
         return bufferedCount + this.internalReader.Read(buffer, index + bufferedCount, count - bufferedCount);
      }

      /// <summary>
      /// Reads a line of characters from the current stream and returns the data as a string.
      /// </summary>
      /// <returns>The next line from the input stream, or <c>Nothing</c> if the end of the input stream is reached.</returns>
      public override string ReadLine()
      {
         if (this.charBuffer.Count == 0)
         {
            return this.internalReader.ReadLine();
         }
         var builder = new StringBuilder();
         while (this.charBuffer.Count > 0)
         {
            var c = this.charBuffer.Pop();
            switch (c)
            {
               case '\r':
                  if (Peek() == '\n')
                  {
                     Read();
                  }
                  return builder.ToString();
               case '\n':
                  return builder.ToString();
               default:
                  builder.Append(c);
                  break;
            }
         }
         builder.Append(this.internalReader.ReadLine());
         return builder.Length == 0 && Peek() < 0 ? null : builder.ToString();
      }

      /// <summary>
      /// Reads the stream from the current position to the end of the stream.
      /// </summary>
      /// <returns>The rest of the stream as a string, from the current position to the end.</returns>
      public override string ReadToEnd()
      {
         if (this.charBuffer.Count == 0)
         {
            return this.internalReader.ReadToEnd();
         }
         var builder = new StringBuilder();
         builder.Append(this.charBuffer.ToArray());
         ClearBuffer();
         builder.Append(this.internalReader.ReadToEnd());
         return builder.ToString();
      }

      /// <summary>
      /// Reads the next character from the input stream. The character position will not be changed.
      /// </summary>
      /// <returns>The next character from the input stream, or -1 if no more characters are available.</returns>
      public override int Peek()
      {
         return this.charBuffer.Count > 0 ? this.charBuffer.Peek() : this.internalReader.Peek();
      }

      /// <summary>
      /// Pushes a character to the head position of the reader.
      /// </summary>
      /// <param name="character">The character to revert.</param>
      public virtual void Push(char character)
      {
         this.charBuffer.Push(character);
      }

      /// <summary>
      /// Pushes characters to the head position of the reader.
      /// </summary>
      /// <param name="value">The characters to revert.</param>
      /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>Nothing</c>.</exception>
      public virtual void Push(string value)
      {
         if (value == null)
         {
            throw new ArgumentNullException("value");
         }
         for (var index = value.Length - 1; index >= 0; index--)
         {
            this.charBuffer.Push(value[index]);
         }
      }
   }
}
