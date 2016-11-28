using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Muck
{
    internal static class ReflectionHelpers
    {
        internal static IEnumerable<TAtt> Attributes<TAtt>(this TypeInfo o) where TAtt:Attribute
        {
            return o.GetCustomAttributes(typeof(TAtt)).Cast<TAtt>();
        }

        internal static IEnumerable<TAtt> Attributes<TAtt>(this MethodInfo o) where TAtt:Attribute
        {
            return o.GetCustomAttributes(typeof(TAtt)).Cast<TAtt>();
        }
        
        internal static object Default(this Type t)
        {
            return typeof(ReflectionHelpers).GetMethod("DefaultT").MakeGenericMethod(t).Invoke(null, null);
        }
        private static T DefaultT<T>()
        {
            return default(T);
        }
    }
}