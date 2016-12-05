using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Muck
{
    internal static partial class RuntimeCompiler
    {
        public static IDynamicSourceItem CreateClassFor<T>()
        {
            var t = typeof(T);
            var name = t.Name + "__MockImpl";
            var classItems = new List<IDynamicSourceItem>();
            classItems.AddRange(Type(t));
            return new DynamicClass(name, t.FullName, new []{"Muck.IDynamicMockObject"}, classItems.ToArray());
        }

        private static IEnumerable<IDynamicSourceItem> Type(Type t)
        {
            return Methods(t).Concat(Constructors(t)).Concat(Properties(t)).Concat(Events(t)).Concat(BaseType(t.GetInterfaces()));
        }

        private static IEnumerable<IDynamicSourceItem> BaseType(Type[] t)
        {
            if (t == null)return new IDynamicSourceItem[] {};
            var baseImpl = new List<IDynamicSourceItem>();
            foreach (var iface in t)
            {
               baseImpl.AddRange(Methods(iface).Concat(Properties(iface)).Concat(Events(iface)).Concat(BaseType(iface.GetInterfaces())));
            }
            return baseImpl;
        }

        private static IEnumerable<IDynamicSourceItem> Constructors(Type t)
        {
            return t.GetConstructors().Select(constructor => new DynamicConstructor(constructor.Name, constructor.GetCustomAttribute<MockImplementationAttribute>()?.Body ?? null, constructor.GetParameters().Select(p=>new DynamicParameter {Type = p.ParameterType.FullName, Name = p.Name}).ToArray())).Cast<IDynamicSourceItem>();
        }

        private static IEnumerable<IDynamicSourceItem> Properties(Type t)
        {
            return from property in t.GetProperties()
                   where !property.IsSpecialName
                   select new DynamicProperty(
                       property.Name,
                       property.PropertyType.FullName,
                       property.GetCustomAttribute<MockImplementationAttribute>()?.Body ?? null,
                       property.CanRead,
                       property.CanWrite);
        }

        private static IEnumerable<IDynamicSourceItem> Methods(Type t)
        {
            return from method in t.GetMethods()
                   where !method.IsSpecialName
                   select new DynamicMethod(
                       method.Name,
                       method.ReturnType.FullName,
                       method.GetCustomAttribute<MockImplementationAttribute>()?.Body ?? null,
                       method.GetParameters()
                            .Select(p => new DynamicParameter {Type = p.ParameterType.FullName, Name = p.Name})
                            .ToArray());
        }

        private static IEnumerable<IDynamicSourceItem> Events(Type t)
        {
            return from evnt in t.GetEvents()
                   where !evnt.IsSpecialName
                   select new DynamicEvent(
                       evnt.Name,
                       evnt.EventHandlerType.FullName,
                       evnt.EventHandlerType.GetMethod("Invoke").ReturnType.FullName,
                       evnt.EventHandlerType.GetMethod("Invoke").GetParameters()
                            .Select(p => new DynamicParameter { Type = p.ParameterType.FullName, Name = p.Name })
                            .ToArray());
        }

        public static Assembly CompileDynamicClass(IDynamicSourceItem @class, params MetadataReference[] refs)
        {
            var sourceCode = @class.Render();
            Log.Instance.AppendLine(sourceCode,6, ConsoleColor.Cyan);
            var st = CSharpSyntaxTree.ParseText(sourceCode);
            string an = Path.GetRandomFileName();
            MetadataReference[] references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Mock).Assembly.Location)
            }.Concat(refs).ToArray();

            CSharpCompilation assembly = CSharpCompilation.Create(
                an,
                syntaxTrees: new[] { st },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using (var dllStream = new MemoryStream())
            using (var pdbStream = new MemoryStream())
            {
                var result = assembly.Emit(dllStream, pdbStream);

                if (result.Success)
                {
                    return Assembly.Load(dllStream.ToArray(),pdbStream.ToArray());
                }
                else
                {
                    Debug.WriteLine(string.Join("\r\n", result.Diagnostics.Select(x => x.ToString())));
                    throw new ArgumentException($"Compilation failed : \r\n {string.Join("\r\n", result.Diagnostics.Select(x=>x.ToString()))}");
                }
            }
        }
        
    }
}