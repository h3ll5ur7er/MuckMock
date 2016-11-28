using System;
using Muck;

namespace SampleApi
{
    public interface ISampleBaseInterface
    {
        [MockImplementation("\"FooBar\"")]
        string Inherited { get; }

        [MockImplementation("return $\"Input:{i}\";")]
        string InheritedFunction(int i);
    }
}