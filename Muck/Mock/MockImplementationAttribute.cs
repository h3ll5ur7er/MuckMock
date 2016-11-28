using System;

namespace Muck
{
    [AttributeUsage(AttributeTargets.Method|AttributeTargets.Property|AttributeTargets.Constructor, AllowMultiple = false)]
    public class MockImplementationAttribute : Attribute
    {
        public string Body { get; }

        public MockImplementationAttribute(string body)
        {
            Body = body;
        }
    }
}