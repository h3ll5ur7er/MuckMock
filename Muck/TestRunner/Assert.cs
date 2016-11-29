using System.Runtime.CompilerServices;

namespace Muck
{
    public static class Assert
    {
        public static void AssertNotNull<T1, T2>(this T1 v1, string message = "", [CallerFilePath]string callerFile = null, [CallerMemberName]string callerName = null, [CallerLineNumber]int callerLine = -1)
        {
            if(v1 == null) throw new AssertFailedException(typeof(T1), v1, "NotNull", message, callerFile, callerName, callerLine);
        }

        public static void AssertEquals<T1, T2>(this T1 v1, T2 v2, string message ="", [CallerFilePath]string callerFile = null, [CallerMemberName]string callerName = null, [CallerLineNumber]int callerLine = -1)
        {
            if (v1?.Equals(v2) ?? v2 == null)
                return;
            throw new AssertFailedException(v1, v2, "AreEqual", message, callerFile, callerName, callerLine);
        }
    }
}