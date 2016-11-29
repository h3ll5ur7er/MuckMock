using System;

namespace SampleApi
{
    public class CodeUnderTest
    {
        public ISampleInterface Value { get; set; }

        public CodeUnderTest(ISampleInterface value)
        {
            Value = value;
            
        }
        public CodeUnderTest() : this(new SampleClass())
        {
        }

        public string Return(int s) => s.ToString();

        public string Throw()
        {
            throw new NotImplementedException();
        } 
    }
}