using System;

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
        public string MockImplementation { get; }

        public RunAttribute(string name = "", Type expectedExceptionType = null, string expectedExceptionMessage = "", string mockImplementation = "", string testName = "", object expected = null, params object[] param)
        {
            Name = name;
            ExpectedExceptionType = expectedExceptionType;
            ExpectedExceptionMessage = expectedExceptionMessage;
            Expected = expected;
            MockImplementation = mockImplementation;
            TestName = testName;
            Parameters = param;
        }
    }
}