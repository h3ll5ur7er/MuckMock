using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Muck
{
    public class TestRunner
    {
        public static void Main(string[] args)
        {
            if (args.Length>0)
                foreach (var arg in args)
                    RunTests(Assembly.LoadFrom(arg));
            else
                Console.WriteLine("Please specify the path to the testassembly");
        }

        public static string RunTests(Assembly assembly = null)
        {
            var Out = new StringBuilder();
            if (assembly == null) assembly = Assembly.GetExecutingAssembly();
            foreach (var type in assembly.DefinedTypes)
            {
                var atts = type.Attributes<TestAttribute>();
                if (atts != null && atts.Count() != 0)
                {
                    Out.AppendLine($"Starting Test execution in {assembly.FullName}");
                    var total = 0;
                    var success = 0;
                    var instance = Activator.CreateInstance(type);
                    foreach (var test in atts)
                    {
                        var testTotal = 0;
                        var testSuccess = 0;
                        Out.AppendLine($"Starting {test.Name}");
                        var methods = type.GetMethods().Where(x => x.Attributes<RunAttribute>().Any());
                        foreach (var method in methods)
                        {
                            var methodTotal = 0;
                            var methodSuccess = 0;
                            Out.AppendLine($"\tEntering  {test.Name }.{method.Name}");
                            foreach (var run in method.Attributes<RunAttribute>().Where(r => r.TestName == test.Name))
                            {
                                Out.AppendLine($"\t\tRunning {test.Name }.{method.Name}.{run.Name}");
                                var actual = method.Invoke(instance, run.Parameters.Length > 0 ? run.Parameters : null);

                                if (run.ExpectedExceptionType != null || !String.IsNullOrWhiteSpace(run.ExpectedExceptionMessage))
                                {
                                    if ((actual?.Equals(run.Expected) ?? run.Expected?.Equals(actual) ?? false) || (run.Expected == null && actual == null))
                                    {
                                        Out.AppendLine($"\t\t\t{test.Name}.{method.Name}.{run.Name} failed!!\r\n\t\t\t\texpected exception : {run.ExpectedExceptionType.Name}");
                                    }
                                    else
                                    {
                                        Out.AppendLine($"\t\t\t{test.Name}.{method.Name}.{run.Name} ran to completion");
                                        testSuccess++;
                                        methodSuccess++;
                                        success++;
                                    }
                                }
                                else
                                {
                                    if ((actual?.Equals(run.Expected) ?? run.Expected?.Equals(actual) ?? false) || (run.Expected == null && actual == null))
                                    {
                                        Out.AppendLine($"\t\t\t{test.Name}.{method.Name}.{run.Name} ran to completion");
                                        success++;
                                        testSuccess++;
                                        methodSuccess++;
                                    }
                                    else
                                    {
                                        Out.AppendLine($"\t\t\t{test.Name}.{method.Name}.{run.Name} failed!!\r\n\t\t\t\texpected : {run.Expected}\r\n\t\t\t\tactual : {actual}");
                                    }

                                }
                                total++;
                                testTotal++;
                                methodTotal++;
                            }
                            Out.AppendLine($"\tLeaving {test.Name }.{method.Name} Result: {methodSuccess} of {methodTotal}");
                        }
                        Out.AppendLine($"Completed {test.Name}  Result: {testSuccess} of {testTotal}");
                    }
                    Out.AppendLine($"All Tests Executed: {success} of {total}");
                }
            }
            return Out.ToString();
        }
    }
}