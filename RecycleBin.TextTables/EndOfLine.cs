using System.ComponentModel;

namespace RecycleBin.TextTables
{
   /// <summary>
   /// Signifies the end of a line of text.
   /// </summary>
   public enum EndOfLine
   {
      /// <summary>
      /// Carriage return and line feed.
      /// </summary>
      /// <remarks>
      /// Typically used on Microsoft Windows.
      /// </remarks>
      CRLF,

      /// <summary>
      /// Line feed.
      /// </summary>
      /// <remarks>
      /// Typically used on Unix and Unix-like systems.
      /// </remarks>
      LF,

      /// <summary>
      /// Carriage return.
      /// </summary>
      /// <remarks>
      /// Typically used on old Mac OS.
      /// </remarks>
      CR,
   }

   /// <summary>
   /// Provides extension methods of <see cref="EndOfLine"/>.
   /// </summary>
   public static class EndOfLineExtension
   {
      /// <summary>
      /// Gets the string representation of the specified end-of-line.
      /// </summary>
      /// <param name="eol">The end-of-line enumeration.</param>
      /// <returns>The newline.</returns>
      public static string AsNewline(this EndOfLine eol)
      {
         switch (eol)
         {
            case EndOfLine.CRLF:
               return "\r\n";
            case EndOfLine.LF:
               return "\n";
            case EndOfLine.CR:
               return "\r";
            default:
               throw new InvalidEnumArgumentException("eol", (int)eol, typeof(EndOfLine));
         }
      }
   }
}
