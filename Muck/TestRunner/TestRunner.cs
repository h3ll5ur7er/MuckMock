using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Muck
{
    public static class TestRunner
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0)
                foreach (var arg in args)
                    RunTests(Assembly.LoadFrom(arg));
            else
                Console.WriteLine("Please specify the path to the testassembly");
        }

        public static void RunTests(Assembly assembly = null, int logLevel = 4)
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
                    Log.Instance.AppendLine($"Starting Test execution in {type.Name}", 0, ConsoleColor.DarkCyan);
                    var typeTotal = 0;
                    var typeSuccess = 0;
                    var instance = Activator.CreateInstance(type);
                    foreach (var test in atts)
                    {
                        var testTotal = 0;
                        var testSuccess = 0;
                        Log.Instance.AppendLine($"Starting {test.Name}", 1, ConsoleColor.DarkCyan);
                        var methods = type.GetMethods().Where(x => x.Attributes<RunAttribute>().Any());
                        foreach (var method in methods)
                        {
                            var methodTotal = 0;
                            var methodSuccess = 0;
                            Log.Instance.AppendLine($"{test}", 5);
                            foreach (var run in method.Attributes<RunAttribute>().Where(r => r.TestName == test.Name))
                            {
                                Log.Instance.AppendLine($"{run}", 6);
                                try
                                {
                                    //Start Method execution
                                    var actual = MethodExecutionProxy(method, instance, run.Parameters);
                                    //    if no exception expected
                                    if (run.ExpectedExceptionType == null)
                                    {
                                        //        if result == expected -> pass
                                        if ((actual?.Equals(run.Expected) ?? run.Expected?.Equals(actual) ?? false) ||
                                            (run.Expected == null && actual == null))
                                        {
                                            Log.Instance.AppendLine(
                                                $"    {test.Name}.{method.Name}.{run.Name} ran as expected", 4, ConsoleColor.DarkGreen);
                                            success++;
                                            typeSuccess++;
                                            testSuccess++;
                                            methodSuccess++;
                                        }

                                        //        if result != expected -> fail
                                        else
                                        {
                                            Log.Instance.AppendLine(
                                                $"    Test failed!! {test.Name}.{method.Name}.{run.Name}    expected : {run.Expected}    actual : {actual}",
                                                0, ConsoleColor.Red);
                                        }
                                    }
                                    //    if exception expected but not thrown -> fail
                                    else
                                    {
                                        Log.Instance.AppendLine(
                                            $"    Test failed!! {test.Name}.{method.Name}.{run.Name}    expected : {run.ExpectedExceptionType.Name}",
                                            0, ConsoleColor.Red);
                                    }

                                }
                                // if test execution throws exception
                                catch (Exception e)
                                {
                                    // if no exception expected -> fail
                                    if (run.ExpectedExceptionType == null)
                                    {
                                        Log.Instance.AppendLine(
                                            $"    Test failed!! {test.Name}.{method.Name}.{run.Name}    expected : {run.Expected} actual : {e.GetType().Name}",
                                            0, ConsoleColor.Red);
                                    }
                                    // if exception expected -> pass
                                    else if (e.GetType() == run.ExpectedExceptionType)
                                    {
                                        if (string.IsNullOrWhiteSpace(run.ExpectedExceptionMessage) ||
                                            run.ExpectedExceptionMessage == e.Message)
                                        {
                                            Log.Instance.AppendLine(
                                                $"    {test.Name}.{method.Name}.{run.Name} ran as expected", 4, ConsoleColor.DarkGreen);
                                            testSuccess++;
                                            typeSuccess++;
                                            methodSuccess++;
                                            success++;
                                        }
                                    }
                                    // if assert failed -> fail
                                    else if (e.GetType() == typeof(AssertFailedException))
                                    {
                                        if (string.IsNullOrWhiteSpace(run.ExpectedExceptionMessage) ||
                                            run.ExpectedExceptionMessage == e.Message)
                                        {
                                        Log.Instance.AppendLine(
                                            $"    Test failed!! {((AssertFailedException)e).AssertType} {test.Name}.{method.Name}.{run.Name}    expected : {((AssertFailedException)e).ExpectedValue} actual : {((AssertFailedException)e).ActualValue}",
                                            0, ConsoleColor.Red);

                                        }
                                    }
                                    else
                                    {
                                        Log.Instance.AppendLine(
                                            $"    Test failed!! {test.Name}.{method.Name}.{run.Name}    expected : {run.ExpectedExceptionType.Name}     actual : {e.GetType().Name}",
                                            0, ConsoleColor.Red);
                                    }
                                }

                                total++;
                                testTotal++;
                                typeTotal++;
                                methodTotal++;
                            }
                            Log.Instance.AppendLine(
                                $"Finshed Method {test.Name}.{method.Name} Result: {methodSuccess} of {methodTotal}", 5, ConsoleColor.Green);
                        }
                        Log.Instance.AppendLine($"Finshed {test.Name}  Result: {testSuccess} of {testTotal}", 2, ConsoleColor.Green);
                    }
                    Log.Instance.AppendLine($"Finshed {type.Name}: {typeSuccess} of {typeTotal}", 5, ConsoleColor.Green);
                }
            }
            Log.Instance.AppendLine($"All Tests Executed: {success} of {total} ran as expected", 0, ConsoleColor.Green);
            Console.WriteLine("Execution Finished. Press any key to view Results");
            Console.ReadKey();
            Console.Clear();
            Console.SetCursorPosition(0,0);
            Log.Print(logLevel);
        }
        
        private static object MethodExecutionProxy(this MethodInfo method, object instance, params object[] param)
        {
            Delegate @delegate;
            try
            {
                Func<Type[], Type> getType;
                var types = method.GetParameters().Select(p => p.ParameterType);

                if (method.ReturnType == typeof(void))
                {
                    getType = Expression.GetActionType;
                }
                else
                {
                    getType = Expression.GetFuncType;
                    types = types.Concat(new[] {method.ReturnType});
                }

                if (method.IsStatic)
                {
                    return Delegate.CreateDelegate(getType(types.ToArray()), method);
                }

                @delegate = Delegate.CreateDelegate(getType(types.ToArray()), instance, method.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Debug.WriteLine(ex);
                Debugger.Break();
                throw ex;
            }

            try
            {
                return @delegate.DynamicInvoke(param);
            }
            catch (TargetInvocationException e)
            {
                if(e.InnerException!= null)
                    throw e.InnerException;
                throw;
            }
        }
    }
}