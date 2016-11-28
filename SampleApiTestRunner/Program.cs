using Muck;
using System;
using System.Collections.Generic;
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

            Console.WriteLine(TestRunner.RunTests(Assembly.GetAssembly(typeof(SampleApiTests.ISampleInterfaceTests))));
            Console.ReadKey();
        }
    }
}
