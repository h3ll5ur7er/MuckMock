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
    internal static class RuntimeCompiler
    {
        public static IDynamicSourceItem CreateClassFor<T>()
        {
            var t = typeof(T);
            var classItems = new List<IDynamicSourceItem>();
            classItems.AddRange(Type(t));
            return new DynamicClass(t.Name+"__Impl__Dynamic", t.FullName, null, classItems.ToArray());
        }

        private static IEnumerable<IDynamicSourceItem> Type(Type t)
        {
            return Properties(t).Concat(Constructors(t)).Concat(Methods(t)).Concat(BaseType(t.GetInterfaces()));
        }

        private static IEnumerable<IDynamicSourceItem> BaseType(Type[] t)
        {
            if (t == null)return new IDynamicSourceItem[] {};
            var baseImpl = new List<IDynamicSourceItem>();
            foreach (var iface in t)
            {
               baseImpl.AddRange(Properties(iface).Concat(Methods(iface)).Concat(BaseType(iface.GetInterfaces())));
            }
            return baseImpl;
        }

        private static IEnumerable<IDynamicSourceItem> Constructors(Type t)
        {
            return t.GetConstructors().Select(constructor => new DynamicConstructor(constructor.Name, constructor.GetCustomAttribute<MockImplementationAttribute>()?.Body ?? null, constructor.GetParameters().Select(p=>new DynamicParameter {Type = p.ParameterType.FullName, Name = p.Name}).ToArray())).Cast<IDynamicSourceItem>();
        }

        private static IEnumerable<IDynamicSourceItem> Methods(Type t)
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

        private static IEnumerable<IDynamicSourceItem> Properties(Type t)
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

        public static Assembly CompileDynamicClass(IDynamicSourceItem @class, params MetadataReference[] refs)
        {
            var sourceCode = @class.Render();

            var st = CSharpSyntaxTree.ParseText(sourceCode);
            string an = Path.GetRandomFileName();
            MetadataReference[] references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Mock).Assembly.Location)
            }.Concat(refs).ToArray();

            CSharpCompilation compilation = CSharpCompilation.Create(
                an,
                syntaxTrees: new[] { st },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using (var dllStream = new MemoryStream())
            using (var pdbStream = new MemoryStream())
            using (var docStream = new MemoryStream())
            {
                var result = compilation.Emit(dllStream, pdbStream, docStream);

                if (result.Success)
                {
                    return Assembly.Load(dllStream.ToArray());
                }
                else
                {
                    Debug.WriteLine(string.Join("\r\n", result.Diagnostics.Select(x => x.ToString())));
                    throw new ArgumentException($"Compilation failed : \r\n {string.Join("\r\n", result.Diagnostics.Select(x=>x.ToString()))}");
                }
            }
        }
        
        internal interface IDynamicSourceItem
        {
            string Name { get; }
            string Render();
        }
        internal interface ITypeNamePair
        {
            string Type { get; }
            string Name { get; }
        }

        internal class PredefinedClass : IDynamicSourceItem
        {
            public string Name { get; set; }
            public string Code { get; set; }
            public string Render()
            {
                return Code;
            }
        }

        internal class DynamicClass : IDynamicSourceItem
        {
            public string Name { get; }
            public string BaseClass { get; }
            public string[] Interfaces { get; }
            public List<IDynamicSourceItem> BodyItems { get; }

            public string DefaultSignaturePattern { get; } = "public class {0} : {1}, {2}";
            public string SignaturePatternWithoutBaseClass { get; } = "public class {0} : {2}";
            public string SignaturePatternWithoutInterfaces { get; } = "public class {0} : {1}";
            public string SignaturePatternWithoutInheritance { get; } = "public class {0}";
            public string SignaturePattern { get; }

            public DynamicClass(string name, string baseClass = "", string[] interfaces = null,
                params IDynamicSourceItem[] bodyItems)
            {
                Name = name;
                BaseClass = baseClass;
                Interfaces = interfaces;
                BodyItems = new List<IDynamicSourceItem>(bodyItems);
                SignaturePattern = string.IsNullOrWhiteSpace(BaseClass)
                    ? (interfaces == null || interfaces.Length == 0 ? SignaturePatternWithoutInheritance : SignaturePatternWithoutBaseClass)
                    : (interfaces == null || interfaces.Length == 0 ? SignaturePatternWithoutInterfaces : DefaultSignaturePattern);

                //BodyItems.Add(new DynamicProperty());

            }

            public string Render()
            {
                var sig = string.Format(SignaturePattern, Name, BaseClass, string.Join(", ", Interfaces??new string[] {}));
                return $"{sig}\r\n{{\r\n\t{string.Join("\r\n\t", BodyItems.Select(x => x.Render()))}\r\n}}";
            }
        }

        public class DynamicProperty : IDynamicSourceItem
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string DefaultValue { get; set; }
            public bool HasGetter { get; set; }
            public bool HasSetter { get; set; }

            public DynamicProperty(string name, string type, string defaultValue = null, bool hasGetter = true, bool hasSetter = true)
            {
                Name = name;
                Type = type;
                DefaultValue = string.IsNullOrWhiteSpace(defaultValue)?"":" = "+defaultValue+";";
                HasGetter = hasGetter;
                HasSetter = hasSetter;
            }

            public string Render()
            {
                return HasGetter
                    ? HasSetter ? $"public {Type} {Name} {{ get; set; }}{DefaultValue}" : $"public {Type} {Name} {{ get; }}{DefaultValue}"
                    : HasSetter ? $"public {Type} {Name} {{ set; }}" : "";
            }
        }

        public class DynamicMethod : IDynamicSourceItem
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public DynamicParameter[] Parameters { get; set; }
            public string Body { get; set; }
            public bool HasGetter { get; set; }
            public bool HasSetter { get; set; }

            public DynamicMethod(string name, string type, string body = null, params DynamicParameter[] parameters)
            {
                Parameters = parameters;
                Type = type == "System.Void" ? "void":type;
                Body = body??(Type == "void"?"":$"return default({Type});");
                Name = name;
            }

            public string Render()
            {
                return $"public {Type} {Name}({string.Join(", ", Parameters.Select(x => x.ToString()))}) {{ {Body} }}";
            }
        }

        public class DynamicConstructor : IDynamicSourceItem
        {
            public string Name { get; set; }
            public DynamicParameter[] Parameters { get; set; }
            public string Body { get; set; }
            public bool HasGetter { get; set; }
            public bool HasSetter { get; set; }

            public DynamicConstructor(string name, string body, params DynamicParameter[] parameters)
            {
                Parameters = parameters;
                Body = body;
                Name = name;
            }

            public string Render()
            {
                return $"public {Name}({string.Join(", ", Parameters.Select(x => x.ToString()))}) {{ {Body} }}";
            }
        }

        public class DynamicParameter : ITypeNamePair
        {
            private string name;
            private string type;

            public string Name
            {
                get { return name; }
                set { name = value; }
            }

            public string Type
            {
                get { return type; }
                set { type = value == "System.Void" ? "void" : type = value; }
            }

            public override string ToString()
            {
                return $"{Type} {Name}";
            }
        }
    }
}