using System;
using Muck;
using SampleApi;

namespace SampleApiTests
{
    [Test(typeof(CodeUnderTest), "CodeUnderTestTests")]
    [Test(typeof(CodeUnderTest), "CodeUnderTestTestsFail")]
    [Test(typeof(CodeUnderTest), "CodeUnderTestTestsSuccess")]
    public class CodeUnderTestTests
    {
        [Run("MockObjectTest1", testName: "CodeUnderTestTests", expected:"Hello from Mock")]
        [Run("MockObjectTest1", testName: "CodeUnderTestTestsFail", expected:"BlaBla")]
        [Run("MockObjectTest1", testName: "CodeUnderTestTestsSuccess", expected:"Hello from Mock")]
        public string MockObjectNameTest1()
        {
            var mock = Mock.Create<ISampleInterface>();
            var cut = new CodeUnderTest(mock);
            var actual = cut.Value.Name;
            return actual;
        }

        [Run("MockObjectTest2", testName: "CodeUnderTestTests", param:"Hello from Mock")]
        [Run("MockObjectTest2", testName: "CodeUnderTestTestsFail", param:"BlaBla")]
        [Run("MockObjectTest2", testName: "CodeUnderTestTestsSuccess", param:"Hello from Mock")]
        public void MockObjectNameTest2(string expected)
        {
            var mock = Mock.Create<ISampleInterface>();
            var cut = new CodeUnderTest(mock);
            var actual = cut.Value.Name;
            expected.AssertEquals(actual);
        }
        
        [Run("MockObjectTest3.1", testName: "CodeUnderTestTests", expected:"Hello from Mock")]
        [Run("MockObjectTest3.1", testName: "CodeUnderTestTestsSuccess", expected:"Hello from Mock")]
        [Run("MockObjectTest3.1", testName: "CodeUnderTestTestsFail", expected:"FooBar")]
        public string MockObjectNameTest3_1()
        {
            var mock = Mock.Create<ISampleInterface>();
            var cut = new CodeUnderTest(mock);
            var actual = cut.Value.Name;
            return actual;
        }

        [Run("MockObjectTest3.2", testName: "CodeUnderTestTests", param:"Hello from Mock")]
        [Run("MockObjectTest3.2", testName: "CodeUnderTestTestsSuccess", param:"Hello from Mock")]
        [Run("MockObjectTest3.2", testName: "CodeUnderTestTestsFail", param:"FooBar")]
        public void MockObjectNameTest3_2(string expected)
        {
            var mock = Mock.Create<ISampleInterface>();
            var cut = new CodeUnderTest(mock);
            var actual = cut.Value.Name;
            expected.AssertEquals(actual);
        }
        
        [Run("MockObjectTest4.1", testName: "CodeUnderTestTests", expectedExceptionType:typeof(NotImplementedException))]
        [Run("MockObjectTest4.2", testName: "CodeUnderTestTestsSuccess", expectedExceptionType:typeof(NotImplementedException))]
        [Run("MockObjectTest4.3", testName: "CodeUnderTestTestsFail", expectedExceptionType:typeof(ArgumentException))]
        [Run("MockObjectTest4.4", testName: "CodeUnderTestTestsFail")]
        public void MockObjectReturnTest4()
        {
            var cut = new CodeUnderTest();
            cut.Throw();
        }
        [Run("MockObjectTest4.5", testName: "CodeUnderTestTestsFail", expectedExceptionType:typeof(NotImplementedException))]
        public void MockObjectReturnTest4_()
        {
        }
    }
}