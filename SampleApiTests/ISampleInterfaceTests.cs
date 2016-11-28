using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Muck;
using SampleApi;

namespace SampleApiTests
{
    [Test(typeof(ISampleInterface), "ISampleInterfaceTests")]
    public class ISampleInterfaceTests
    {
        [Run("MockObjectTest", testName:"ISampleInterfaceTests")]
        public void MockObjectTest()
        {
            var mock = Mock.Create<ISampleInterface>();

            mock.Function();
            mock.FunctionWithInput("Test");
            var v1 = mock.FunctionWithReturnValue();
            var v2 = mock.FunctionWithInputAndReturnValue("Test");
            Console.WriteLine(v1);
            Console.WriteLine(v2);
            Console.WriteLine(mock.Name);
            Console.WriteLine(mock.Value);
            Console.WriteLine(mock.Inherited);
            Console.WriteLine(mock.InheritedFunction(42));
            Console.WriteLine();
        }
    }
}
