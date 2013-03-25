using System;

namespace RecycleBin.TextTables
{
   /// <summary>
   /// Provides metainformation to parse string values.
   /// </summary>
   [Serializable]
   [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
   public class ColumnAttribute : ValueAttribute
   {
      private static readonly string[] Empty = new string[0];

      private readonly int index;
      private string name;

      /// <summary>
      /// Creates a new attribute by a specified name.
      /// </summary>
      /// <param name="name">The name.</param>
      public ColumnAttribute(string name)
      {
         if (name == null)
         {
            throw new ArgumentNullException("name");
         }
         this.name = name;
         this.index = -1;
      }

      /// <summary>
      /// Creates a new attribute by a specified index.
      /// </summary>
      /// <param name="index">The index.</param>
      public ColumnAttribute(int index)
      {
         if (index < 0)
         {
            throw new ArgumentOutOfRangeException("index");
         }
         this.index = index;
      }

      /// <summary>
      /// Gets the name of the column.
      /// </summary>
      /// <returns>The name or <c>Nothing</c>.</returns>
      /// <remarks>Returns <c>Nothing</c> if no name is specified.</remarks>
      public string Name
      {
         get { return this.name; }
         set
         {
            if (this.index < 0 && value == null)
            {
               throw new ArgumentNullException("value", "Target must be specified when index is not defined.");
            }
            this.name = value;
         }
      }

      /// <summary>
      /// Gets the index of the column.
      /// </summary>
      /// <returns>The index or <c>-1</c>.</returns>
      /// <remarks>Returns <c>-1</c> if no index is specified.</remarks>
      public int Index
      {
         get { return this.index; }
      }

      /// <summary>
      /// Gets or sets the value indicating field at the column may be omitted.
      /// </summary>
      /// <remarks>
      /// If the <see cref="Omittable"/> is set <c>True</c>, the column should be the last column in records.
      /// </remarks>
      public bool Omittable { get; set; }

      internal int GetIndex(string[] names)
      {
         return Index >= 0 ? Index : Array.IndexOf(names ?? Empty, Name);
      }
   }
}
