using System;

namespace RecycleBin.TextTables
{
   internal delegate object Parse(string value, IFormatProvider provider);
   internal delegate string Format(object value, IFormatProvider provider);
   internal delegate object GetValue(object instance);
   internal delegate void SetValue(object instance, object value);
   internal delegate object CreateInstance();
}
