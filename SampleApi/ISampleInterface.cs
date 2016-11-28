using Muck;

namespace SampleApi
{
    public interface ISampleInterface : ISampleBaseInterface
    {
        [MockImplementation("\"Hello from Mock\"")]
        string Name { get; set; }
        [MockImplementation("42")]
        int Value { get; set; }
        [MockImplementation("System.Console.WriteLine(\"Hello from Function\");")]
        void Function();

        [MockImplementation("return \"Hello from FunctionWithReturnValue\";")]
        string FunctionWithReturnValue();

        [MockImplementation("System.Console.WriteLine(input);")]
        void FunctionWithInput(string input);

        [MockImplementation("return inout;")]
        string FunctionWithInputAndReturnValue(string inout);
    }
}