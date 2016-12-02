using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Muck
{
    public static class Mock
    {
        public static T Create<T>()
        {
            return CreateMockImplementation<T>();
        }

        private static T CreateMockImplementation<T>()
        {
            var @class = RuntimeCompiler.CreateClassFor<T>();
            try
            {
                var a = RuntimeCompiler.CompileDynamicClass(@class,
                    MetadataReference.CreateFromFile(typeof(T).Assembly.Location));
                return (T) Activator.CreateInstance(a.GetType(@class.Name));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return default(T);
            }
        }

        public static int CallCount(object mock, DynamicClassContentType contentType, string name)
        {
            try
            {
                dynamic mockObject = mock;
                IDynamicMockObject dMock = mockObject;
                return dMock.InvokeCounter[contentType][name];
            }
            catch (Exception)
            {
                return -1;
            }
        }
    }
}
