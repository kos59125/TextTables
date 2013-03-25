using System;

namespace RecycleBin.TextTables
{
   /// <summary>
   /// The constructor is used when a new record is created.
   /// </summary>
   [Serializable]
   [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
   public class RecordConstructorAttribute : Attribute
   {
   }
}
