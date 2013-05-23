using System;

namespace RecycleBin.TextTables
{
   internal sealed class LazyParser<T> : IParser
   {
      private readonly ValueAttribute attribute;

      public LazyParser(ValueAttribute attribute)
      {
         this.attribute = attribute;
      }

      public object Parse(string value, IFormatProvider provider)
      {
         Func<T> factory = () => (T)this.attribute.Parse(value, typeof(T));
         return new Lazy<T>(factory);
      }
   }
}
