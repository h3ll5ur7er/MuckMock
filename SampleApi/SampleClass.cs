using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleApi
{
    public class SampleClass : ISampleInterface
    {
        public string Inherited { get; } = "";
        public string InheritedFunction(int i)
        {
            return i+" was entered";
        }

        public string Name { get; set; }
        public int Value { get; set; }

        public void Function()
        {
            Console.WriteLine("FunctionCalled");
            Debug.WriteLine("FunctionCalled");
        }
        public string FunctionWithReturnValue()
        {
            Console.WriteLine("FunctionCalled");
            Debug.WriteLine("FunctionCalled");
            return "ReturnValue";
        }
        public void FunctionWithInput(string input)
        {
            Console.WriteLine(input);
            Debug.WriteLine(input);
        }
        public string FunctionWithInputAndReturnValue(string inout)
        {
            Console.WriteLine(inout);
            Debug.WriteLine(inout);
            return inout;
        }

    }
}
