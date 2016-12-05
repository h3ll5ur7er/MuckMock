using Muck;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SampleApiTestRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var testClass = new TestClass();
            var t = typeof(TestClass);
            var e = t.GetEvents().First().EventHandlerType;

            TestRunner.RunTests(Assembly.GetAssembly(typeof(SampleApiTests.ISampleInterfaceTests)));

            Console.ReadKey();

        }
    }

    public delegate string TestHandler(int iFoo);
    internal class TestClass
    {
        public event TestHandler FooEvt;
    }
}
