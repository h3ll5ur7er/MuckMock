using System;

namespace Muck
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class TestAttribute : Attribute
    {
        public string Name { get; set; }
        public Type Target { get; set; }

        public TestAttribute(Type target, string name = "")
        {
            Name = name;
            Target = target;
        }

        public override string ToString()
        {
            return $"<:: {Target.Name} ::> {Name}";
        }
    }
}