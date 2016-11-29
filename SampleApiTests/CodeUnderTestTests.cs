using System;
using Muck;
using SampleApi;

namespace SampleApiTests
{
    [Test(typeof(CodeUnderTest), "CodeUnderTestTests")]
    public class CodeUnderTestTests
    {
        [Run("MockObjectTest1", testName: "CodeUnderTestTests", expected:"Hello from Mock")]
        public string MockObjectNameTest1()
        {
            var mock = Mock.Create<ISampleInterface>();
            var cut = new CodeUnderTest(mock);
            var actual = cut.Value.Name;
            return actual;
        }

        [Run("MockObjectTest2", testName: "CodeUnderTestTests", param:"Hello from Mock")]
        public void MockObjectNameTest2(string expected)
        {
            var mock = Mock.Create<ISampleInterface>();
            var cut = new CodeUnderTest(mock);
            var actual = cut.Value.Name;
            expected.AssertEquals(actual);
        }

        [Run("MockObjectTest3.1", testName: "CodeUnderTestTests", param:42, expected: "42")]
        [Run("MockObjectTest3.2", testName: "CodeUnderTestTests", param:13, expected: "13")]
        [Run("MockObjectTest3.3", testName: "CodeUnderTestTests", param:-1, expected: "-1")]
        [Run("MockObjectTest3.4", testName: "CodeUnderTestTests", param:0, expected: "0")]
        [Run("MockObjectTest3.5", testName: "CodeUnderTestTests", param:666, expected: "666")]
        public string MockObjectReturnTest3(string expected = "")
        {
            var cut = new CodeUnderTest();

            var actual = cut.Return(int.Parse(expected));
            expected.ToString().AssertEquals(actual);
            return actual;
        }

        [Run("MockObjectTest4", testName: "CodeUnderTestTests", expectedExceptionType:typeof(NotImplementedException))]
        public void MockObjectReturnTest4()
        {
            var cut = new CodeUnderTest();
            cut.Throw();
        }
    }
}