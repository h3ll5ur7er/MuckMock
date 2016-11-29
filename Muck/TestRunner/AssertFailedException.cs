using System;
using System.IO;

namespace Muck
{
    public class AssertFailedException : Exception
    {
        public TestAttribute Test { get; set; }
        public RunAttribute Run { get; set; }
        public string AssertType { get; set; }
        public string CallerFile { get; set; }
        public string CallerName { get; set; }
        public int CallerLine { get; set; }
        public object ExpectedValue { get; set; }
        public object ActualValue { get; set; }

        public AssertFailedException(object expected, object actual, string assertType = null, string message = "", string callerFile = null, string callerName = null, int callerLine = -1) : base(message)
        {
            ExpectedValue = expected;
            ActualValue = actual;
            AssertType = assertType;
            CallerFile = callerFile;
            CallerName = callerName;
            CallerLine = callerLine;
        }

        public AssertFailedException(TestAttribute test, RunAttribute run, object expectedValue, object actualValue,
            string message = null) : base(message)
        {
            Test = test;
            Run = run;
            ActualValue = actualValue;
            ExpectedValue = expectedValue;
        }

        public override string ToString()
        {
            return $"Assert Failed: {AssertType}\r\nFile:{Path.GetFileName(CallerFile)}.{CallerName}:{CallerLine}\r\n{Test}\r\n{Run}\r\nExpected : {ExpectedValue.ToString()}\r\nActual : {ActualValue.ToString()}\r\nMessage:{Message}";
        }
    }
    
}