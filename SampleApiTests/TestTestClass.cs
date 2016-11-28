using System;
using System.Diagnostics;
using Muck;

namespace SampleApiTests
{
    [Test(null, name: "Test1")]
    [Test(null, name: "Test2")]
    public class TestTestClass
    {
        [Run(name: "TestRunRun1", testName: "Test1")]
        [Run(name: "TestRunRun2", testName: "Test2")]
        [Run(name: "TestRunRun3", testName: "Test1")]
        [Run(name: "TestRunRun4", testName: "Test2")]
        public void Run()
        {
            Console.WriteLine("CW from TestRun");
            Debug.WriteLine("CW from TestRun");
        }
        [Run(name: "TestSprint1", testName: "Test2", expected: "CW from TestSprint 42", param:42)]
        [Run(name: "TestSprint2", testName: "Test1", expected: "CW from TestSprint 13", param: 42)]
        [Run(name: "TestSprint3", testName: "Test2", expected: "CW from TestSprint 13", param: 13)]
        [Run(name: "TestSprint4", testName: "Test1", expected: "CW from TestSprint 42", param: 13)]
        public string Sprint(int i)
        {
            var message = $"CW from TestSprint {i}";
            Console.WriteLine(message);
            Debug.WriteLine(message);
            return message;
        }
    }
}