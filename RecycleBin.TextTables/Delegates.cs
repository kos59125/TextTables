namespace RecycleBin.TextTables
{
   internal delegate object GetValue(object instance);
   internal delegate void SetValue(object instance, object value);
   internal delegate object CreateInstance();
}
