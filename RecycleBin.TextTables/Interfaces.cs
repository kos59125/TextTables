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
      string Format(object value, IFormatProvider provider);
   }
}
