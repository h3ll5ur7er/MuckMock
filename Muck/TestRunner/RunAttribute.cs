using System;
using System.Linq;

namespace Muck
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RunAttribute : Attribute
    {
        public string Name { get; }
        public string TestName { get; }

        public object[] Parameters { get; }

        public object Expected { get; set; }

        public Type ExpectedExceptionType { get; }
        public string ExpectedExceptionMessage { get; }

        public RunAttribute(string name = "", Type expectedExceptionType = null, string expectedExceptionMessage = "", string testName = "", object expected = null, params object[] param)
        {
            Name = name;
            ExpectedExceptionType = expectedExceptionType;
            ExpectedExceptionMessage = expectedExceptionMessage;
            Expected = expected;
            TestName = testName;
            Parameters = param;
        }

        public override string ToString()
        {
            var nl = "\r\n";
            var nlt = "\r\n\t";
            return $"{TestName}.{Name}{nl}({nlt}{string.Join(nlt, Parameters.Select(x=>x.ToString()))}{nl}){nl}" +
                   $"Expected return value : {Expected}" +
                   $"Expected Exception Type: {ExpectedExceptionType?.Name}" +
                   $"Expected Exception Message: {ExpectedExceptionMessage}";
        }
    }
}