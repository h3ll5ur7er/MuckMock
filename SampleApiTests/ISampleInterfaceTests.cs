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

        [Run("MockObjectCallCountTest", testName:"ISampleInterfaceTests")]
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
        [Run("MockObjectExpectAssertFailTest", testName:"ISampleInterfaceTests", expectedExceptionType:typeof(AssertFailedException), expectedExceptionMessage:"expected")]
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
    [Test(typeof(ISampleInterface3), "ISampleInterface3Tests")]
    public class ISampleInterface3Tests
    {
        [Run("MockObjectEventTest", testName:"ISampleInterface3Tests")]
        public void MockObjectTest()
        {
            var mock = Mock.Create<ISampleInterface3>();
            mock.TestEvent1 += Impl1;
            mock.TestEvent2 += Impl2;
            Mock.InvokeEvent(mock, "TestEvent1", "sender", new EventArgs());
            var res = Mock.InvokeEvent(mock, "TestEvent2", "1234567");
            Console.WriteLine(res);
            Assert.MockEventHandlerAdded(mock, "TestEvent1");
            Assert.MockEventHandlerAdded(mock, "TestEvent2");
        }

        private void Impl1(object sender, EventArgs e)
        {
        }

        private int Impl2(string s)
        {
            return s.Length;
        }
    }
}
