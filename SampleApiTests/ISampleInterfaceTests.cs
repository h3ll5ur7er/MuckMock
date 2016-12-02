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

            Assert.MockCallCount(mock, DynamicClassContentType.Method, "Function", 0);
            mock.Function();
            Assert.MockCallCount(mock, DynamicClassContentType.Method, "Function", 1);
            mock.FunctionWithInput("Test");
            var v1 = mock.FunctionWithReturnValue();
            var v2 = mock.FunctionWithInputAndReturnValue("Test");
        }
    }

    [Test(typeof(ISampleInterface), "ISampleInterfaceTests")]
    public class ISampleInterface2Tests
    {
        [Run("MockObjectTest", testName:"ISampleInterfaceTests")]
        public void MockObject_InvokeCounter_FunctionCallIncrementsCounter()
        {
            var mock = Mock.Create<ISampleInterface2>();

            Assert.MockCallCount(mock, DynamicClassContentType.Method, "Function", 0);
            mock.Function();
            Assert.MockCallCount(mock, DynamicClassContentType.Method, "Function", 1);
            mock.FunctionWithInput("Test");
            var v1 = mock.FunctionWithReturnValue();
            var v2 = mock.FunctionWithInputAndReturnValue("Test");
        }

        [Run("MockObjectTest", testName:"ISampleInterfaceTests")]
        public void AssertMockCallCount_CorrectNumberExpected_Passes()
        {
            var mock = Mock.Create<ISampleInterface2>();

            Assert.MockCallCount(mock, DynamicClassContentType.Method, "Function", 0);
            mock.Function();
            Assert.MockCallCount(mock, DynamicClassContentType.Method, "Function", 1);
            mock.FunctionWithInput("Test");
            var v1 = mock.FunctionWithReturnValue();
            var v2 = mock.FunctionWithInputAndReturnValue("Test");
        }
        [Run("MockObjectTest", testName:"ISampleInterfaceTests", expectedExceptionType:typeof(AssertFailedException), expectedExceptionMessage:"expected")]
        public void MockObject_InvokeCounter_CorrectNumberExpected_Passes()
        {
            var mock = Mock.Create<ISampleInterface2>();

            Assert.MockCallCount(mock, DynamicClassContentType.Method, "Function", 0);
            mock.Function();
            Assert.MockCallCount(mock, DynamicClassContentType.Method, "Function", 2, "expected");
            mock.FunctionWithInput("Test");
            var v1 = mock.FunctionWithReturnValue();
            var v2 = mock.FunctionWithInputAndReturnValue("Test");
        }
    }
}
