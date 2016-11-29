using System;
using System.Diagnostics;
using Muck;

namespace SampleApiTests
{
    [Test(typeof(TestTestClass), name: "Test1")]
    [Test(typeof(TestTestClass), name: "Test2")]
    public class TestTestClass
    {
        [Run(name: "TestRunRun1", testName: "Test1")]
        [Run(name: "TestRunRun2", testName: "Test2")]
        public void Run()
        {
            Console.WriteLine("CW from TestRun");
            Debug.WriteLine("CW from TestRun");
        }
        [Run(name: "TestSprint1", testName: "Test2", expected: "CW from TestSprint 42", param:42)]
        [Run(name: "TestSprint3", testName: "Test2", expected: "CW from TestSprint 13", param: 13)]
        [Run(name: "TestSprint5", testName: "Test1", expected: "CW from TestSprint 666", param: 666)]
        public string Sprint(int i)
        {
            var message = $"CW from TestSprint {i}";
            Console.WriteLine(message);
            Debug.WriteLine(message);
            return message;
        }
    }
}