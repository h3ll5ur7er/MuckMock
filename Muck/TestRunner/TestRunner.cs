using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Muck
{
    public class TestRunner
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0)
                foreach (var arg in args)
                    RunTests(Assembly.LoadFrom(arg));
            else
                Console.WriteLine("Please specify the path to the testassembly");
        }

        public static string RunTests(Assembly assembly = null, int logLevel = 2)
        {
            var total = 0;
            var success = 0;
            var Out = new StringBuilder();
            if (assembly == null) assembly = Assembly.GetExecutingAssembly();
            foreach (var type in assembly.DefinedTypes)
            {
                var atts = type.Attributes<TestAttribute>();
                if (atts != null && atts.Count() != 0)
                {
                    Log.Instance.AppendLine($"Starting Test execution in {type.Name}",4);
                    var typeTotal = 0;
                    var typeSuccess = 0;
                    var instance = Activator.CreateInstance(type);
                    foreach (var test in atts)
                    {
                        var testTotal = 0;
                        var testSuccess = 0;
                        Log.Instance.AppendLine($"Starting {test.Name}",3);
                        var methods = type.GetMethods().Where(x => x.Attributes<RunAttribute>().Any());
                        foreach (var method in methods)
                        {
                            var methodTotal = 0;
                            var methodSuccess = 0;
                            Log.Instance.AppendLine($"{test}",5);
                            foreach (var run in method.Attributes<RunAttribute>().Where(r => r.TestName == test.Name))
                            {
                                Log.Instance.AppendLine($"{run}",6);
                                try
                                {
                                    var testFunc = CreateDelegate(method, instance);
                                    var actual = testFunc.DynamicInvoke(run.Parameters);

                                    if (run.ExpectedExceptionType == null)
                                    {
                                        if ((actual?.Equals(run.Expected) ?? run.Expected?.Equals(actual) ?? false) || (run.Expected == null && actual == null))
                                        {
                                            Log.Instance.AppendLine($"\t{test.Name}.{method.Name}.{run.Name} ran successfully",4);
                                            success++;
                                            typeSuccess++;
                                            testSuccess++;
                                            methodSuccess++;
                                        }
                                        else
                                        {
                                            Log.Instance.AppendLine($"\tTest failed!! {test.Name}.{method.Name}.{run.Name}    expected : {run.Expected}    actual : {actual}",0);
                                        } 
                                    }
                                    else
                                    {
                                        Log.Instance.AppendLine($"\tTest failed!! {test.Name}.{method.Name}.{run.Name}    expected exception : {run.ExpectedExceptionType.Name}",0);
                                    }

                                }
                                catch (TargetInvocationException e)
                                {
                                    if (e.InnerException is AssertFailedException)
                                    {
                                        var ex = e.InnerException as AssertFailedException;
                                        Log.Instance.AppendLine($"\tAssert failed!! {test.Name}.{method.Name}.{run.Name}    expected : {run.Expected} actual : {ex.ActualValue}\r\n{ex.ToString()}",0);
                                    }
                                    else if (e.InnerException.GetType() == run.ExpectedExceptionType)
                                    {
                                        if (string.IsNullOrWhiteSpace(run.ExpectedExceptionMessage) || run.ExpectedExceptionMessage == e.InnerException.Message)
                                        {
                                            Log.Instance.AppendLine($"\t{test.Name}.{method.Name}.{run.Name} ran to completion",4);
                                            testSuccess++;
                                            typeSuccess++;
                                            methodSuccess++;
                                            success++;
                                        }
                                    }
                                    else
                                    {
                                        Log.Instance.AppendLine($"\tExpected exception!! {test.Name}.{method.Name}.{run.Name}    expected : {run.ExpectedExceptionType.Name}     actual : {e.InnerException.GetType().Name}",0);
                                    }
                                }
                                catch (AssertFailedException ex)
                                {
                                    Log.Instance.AppendLine($"\tAssert failed!! {test.Name}.{method.Name}.{run.Name}    expected : {run.Expected} actual : {ex.ActualValue}\r\n{ex.ToString()}",0);
                                }
                                
                                total++;
                                testTotal++;
                                typeTotal++;
                                methodTotal++;
                            }
                            Log.Instance.AppendLine($"Finshed Method {test.Name}.{method.Name} Result: {methodSuccess} of {methodTotal}",3);
                        }
                        Log.Instance.AppendLine($"Finshed Test   {test.Name}  Result: {testSuccess} of {testTotal}",2);
                    }
                    Log.Instance.AppendLine($"Finshed Type   {type.Name}: {typeSuccess} of {typeTotal}",1);
                }
            }
            Log.Instance.AppendLine($"All Tests Executed: {success} of {total}", 0);
            return Log.Instance.ToString(logLevel);
        }


        private static Delegate CreateDelegate(MethodInfo methodInfo, object target)
        {
            Func<Type[], Type> getType;
            var types = methodInfo.GetParameters().Select(p => p.ParameterType);

            if (methodInfo.ReturnType == typeof(void))
            {
                getType = Expression.GetActionType;
            }
            else
            {
                getType = Expression.GetFuncType;
                types = types.Concat(new[] { methodInfo.ReturnType });
            }

            if (methodInfo.IsStatic)
            {
                return Delegate.CreateDelegate(getType(types.ToArray()), methodInfo);
            }

            return Delegate.CreateDelegate(getType(types.ToArray()), target, methodInfo.Name);
        }
    }
}