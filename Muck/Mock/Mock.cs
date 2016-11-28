using System;
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
            var a = RuntimeCompiler.CompileDynamicClass(@class, MetadataReference.CreateFromFile(typeof(T).Assembly.Location));
            return (T)Activator.CreateInstance(a.GetType(@class.Name));
        }

    }
}
