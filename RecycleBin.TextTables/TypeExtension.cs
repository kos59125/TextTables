using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RecycleBin.TextTables
{
   internal static class TypeExtension
   {
      internal static ConstructorInfo GetDefaultConstructor(this Type type)
      {
         return type.GetConstructor(Type.EmptyTypes);
      }

      internal static bool IsNullable(this Type type)
      {
         Contract.Requires(type != null);
         return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
      }

      internal static bool IsStatic(this Type type)
      {
         Contract.Requires(type != null);
         return type.IsAbstract && type.IsSealed;
      }
   }
}
