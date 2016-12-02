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
    public interface IDynamicMockObject
    {
        DynamicClassInvokeCounter InvokeCounter { get; }
    }

    internal static class RuntimeCompiler
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
                BodyItems.Add(new DynamicCounter("InvokeCounter"));

            }

            public string Render()
            {
                var sig = string.Format(SignaturePattern, Name, BaseClass, string.Join(", ", Interfaces??new string[] {}));
                return $"{sig}\r\n{{\r\n\t{string.Join("\r\n\t", BodyItems.Select(x => x.Render()).Distinct())}\r\n}}";
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
                DefaultValue = string.IsNullOrWhiteSpace(defaultValue)?"":" = "+defaultValue+"";
                HasGetter = hasGetter;
                HasSetter = hasSetter;
            }

            public string Render()
            {
                var field = $"private {Type} _{Name}{DefaultValue};";
                var property = HasGetter
                    ? HasSetter ? $"public {Type} {Name} {{ get{{InvokeCounter[Muck.DynamicClassContentType.Property][\"{Name}_Get\"]++;return _{Name};}} set{{InvokeCounter[Muck.DynamicClassContentType.Property][\"{Name}_Set\"]++;_{Name} = value;}} }}" : $"public {Type} {Name} {{ get{{InvokeCounter[Muck.DynamicClassContentType.Property][\"{Name}_Get\"]++;return _{Name};}} }}"
                    : HasSetter ? $"public {Type} {Name} {{ set{{InvokeCounter[Muck.DynamicClassContentType.Property][\"{Name}_Set\"]++;_{Name} = value;}} }}" : "";
                return $"{field}\r\n\t{property}";
            }
        }

        public class DynamicIndexer : IDynamicSourceItem
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string DefaultValue { get; set; }
            public bool HasGetter { get; set; }
            public bool HasSetter { get; set; }

            public DynamicIndexer(string name, string type, string defaultValue = null, bool hasGetter = true, bool hasSetter = true)
            {
                Name = name;
                Type = type;
                DefaultValue = string.IsNullOrWhiteSpace(defaultValue)?"":" = "+defaultValue+";";
                HasGetter = hasGetter;
                HasSetter = hasSetter;
            }

            public string Render()
            {
                throw new NotImplementedException();
                return HasGetter
                    ? HasSetter ? $"public {Type} {Name} {{ get; set; }}{DefaultValue}" : $"public {Type} {Name} {{ get; }}{DefaultValue}"
                    : HasSetter ? $"public {Type} {Name} {{ set; }}" : "";
            }
        }

        public class DynamicCounter : IDynamicSourceItem
        {
            public string Name { get; set; }

            public DynamicCounter(string name)
            {
                Name = name;
            }

            public string Render()
            {
                var field =  $"public {typeof(DynamicClassInvokeCounter).FullName} {Name} {{ get; }} = new {typeof(DynamicClassInvokeCounter).FullName}();";
                var indexer =  $"public int this[{typeof(DynamicClassContentType).FullName} memberType, string name] {{ get{{return {Name}[memberType][name];}} set{{{Name}[memberType][name]=value;}} }}";
                return  $"{field}";
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
                    
                return $"public {Type} {Name}({string.Join(", ", Parameters.Select(x => x.ToString()))}) {{ InvokeCounter[Muck.DynamicClassContentType.Method][\"{Name}\"]++;{Body} }}";
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
                return $"public {Name}({string.Join(", ", Parameters.Select(x => x.ToString()))}) {{ InvokeCounter[Muck.DynamicClassContentType.Constructor][\"{Name}\"]++;{Body} }}";
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
    public class DynamicClassInvokeCounter : Dictionary<DynamicClassContentType, InvokeCounter>
    {
        public new InvokeCounter this[DynamicClassContentType key]
        {
            get
            {
                if(!ContainsKey(key))
                    Add(key, new InvokeCounter());
                return base[key];
            }
            set
            {
                if(!ContainsKey(key))
                    Add(key, new InvokeCounter());
                base[key] = value;
            }
        }

        public override string ToString()
        {
            return string.Join("\r\n", this.OrderByDescending(x=>x.Value.Count).Select(x=>$"{x.Key}:\r\n\t{x.Value}"));
        }
    }

    public class InvokeCounter : Dictionary<string, int>
    {
        public new int this[string key]
        {
            get
            {
                if(!ContainsKey(key))
                    Add(key, 0);
                return base[key];
            }
            set
            {
                if(!ContainsKey(key))
                    base.Add(key, 0);
                base[key]+=value;
            }
        }

        public override string ToString()
        {
            return ToString("\r\n\t");
        }

        public string ToString(string sep)
        {
            return string.Join(sep, this.OrderByDescending(x=>x.Value).Select(x=>$"{x.Key} : {x.Value}"));
        }
    }

    public enum DynamicClassContentType
    {
        Property,
        Constructor,
        Method,
    }
}