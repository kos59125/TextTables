using System;

namespace RecycleBin.TextTables
{
   /// <summary>
   /// Parser interface.
   /// </summary>
   /// <remarks>
   /// This interface may not be implemented.
   /// </remarks>
   public interface IParser
   {
      /// <summary>
      /// Parses the input value as object representation.
      /// </summary>
      /// <param name="value">The string to parse.</param>
      /// <param name="provider">The provider.</param>
      /// <returns>The result.</returns>
      object Parse(string value, IFormatProvider provider);
   }

   /// <summary>
   /// Formatter interface.
   /// </summary>
   /// <remarks>
   /// This interface may not be implemented.
   /// </remarks>
   public interface IFormatter
   {
      /// <summary>
      /// Formats the input value as string representation.
      /// </summary>
      /// <param name="value">The object to format.</param>
      /// <param name="provider">The provider.</param>
      /// <returns>The string representation.</returns>
      string Format(object value, IFormatProvider provider);
   }
}
