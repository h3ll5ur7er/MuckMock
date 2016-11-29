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
        
        internal static object Cast(this object o, Type targetType)
        {
            return typeof(ReflectionHelpers).GetMethod("CastT").MakeGenericMethod(targetType).Invoke(null, new []{o});
        }
        private static T CastT<T>(object o)
        {
            return (T)o;
        }
        internal static object SecureCast(this object o, Type targetType)
        {
            if(targetType.IsClass)
            return typeof(ReflectionHelpers).GetMethod("CastT").MakeGenericMethod(targetType).Invoke(null, new []{o});
            throw new ArgumentException("targetType of SecureCast has to be a class");
        }
        private static T SecureCastT<T>(object o) where T : class
        {
            return o as T;
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