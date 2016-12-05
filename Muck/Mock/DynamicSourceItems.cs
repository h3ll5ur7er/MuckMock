using System.Collections.Generic;
using System.Linq;

namespace Muck
{
    internal static partial class RuntimeCompiler
    {

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
                BodyItems.Add(new DynamicCounter());
                BodyItems.Add(new DynamicEvtMgr());

            }

            public string Render()
            {
                var sig = string.Format(SignaturePattern, Name, BaseClass, string.Join(", ", Interfaces ?? new string[] { }));
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
                DefaultValue = string.IsNullOrWhiteSpace(defaultValue) ? "" : " = " + defaultValue + "";
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

        public class DynamicEvent : IDynamicSourceItem
        {
            public string Name { get; set; }
            public string Handler { get; set; }
            public string HandlerReturnType { get; set; }
            public string HandlerParameters { get; set; }
            public string HandlerParameterValues { get; set; }

            public DynamicEvent(string name, string handler, string handlerReturnType, params DynamicParameter[] handlerParmeters)
            {
                Name = name;
                Handler = handler;
                HandlerReturnType = string.IsNullOrWhiteSpace(handlerReturnType) || handlerReturnType == "System.Void" ? "void" : handlerReturnType;
                HandlerParameters = handlerParmeters == null || handlerParmeters.Length == 0 ? "" : string.Join(", ", handlerParmeters.Select(x => x.ToString()));
                HandlerParameterValues = handlerParmeters == null || handlerParmeters.Length == 0 ? "" : string.Join(", ", handlerParmeters.Select(x => x.Name));
            }

            /*
            public Muck.ObservableEvent<{Handler}> {Name}__EventInvokationList = null;
            public event {Handler} {Name} {{ add{InvokeCounter[DynamicClassContentType.Event][\"{Name}_Add\"]++;{Name}__Mock__Create();{Name}__EventInvokationList.Add(value);}} remove{{InvokeCounter[DynamicClassContentType.Event][\"{Name}_Remove\"]++;{Name}__EventInvokationList.Remove(value);}} }}
            private void {Name}__Mock__Create() {{ if (T{Name}__EventInvokationList == null) {Name}__EventInvokationList = new Muck.ObservableEvent<{Handler}>({Name}__Mock__Impl); }}
            
            if 
            private {HandlerReturnType} {Name}__Mock__Impl({HandlerParameters}) { InvokeCounter[DynamicClassContentType.Event]["{Name}_Invoke"]++; return {Name}__EventInvokationList.Invoke({HandlerParameterValues}); }
            else
            private {HandlerReturnType} {Name}__Mock__Impl({HandlerParameters}) { InvokeCounter[DynamicClassContentType.Event]["{Name}_Invoke"]++; }
            
            public object {Name}__Mock__Invoke(params object[] param) {{ {Name}__Mock__Create(); return {Name}__EventInvokationList.Invoke(param); }}
            */


            public string Render()
            {
                var ilst = $"public Muck.ObservableEvent<{Handler}> {Name}__EventInvokationList = null;";
                var evt = $"public event {Handler} {Name} {{ add{{InvokeCounter[Muck.DynamicClassContentType.Event][\"{Name}_Add\"]++;{Name}__Mock__Create();{Name}__EventInvokationList.Add(value);}} remove{{InvokeCounter[Muck.DynamicClassContentType.Event][\"{Name}_Remove\"]++;{Name}__EventInvokationList.Remove(value);}} }}";
                var cf = $"private void {Name}__Mock__Create() {{ if ({Name}__EventInvokationList == null) {{{Name}__EventInvokationList = new Muck.ObservableEvent<{Handler}>({Name}__Mock__Impl);EvtMgr[\"{Name}\"] = {Name}__EventInvokationList; }} }}";
                var inv = $"public object {Name}__Mock__Invoke(params object[] param) {{ {Name}__Mock__Create(); return {Name}__EventInvokationList.Invoke(param); }}";
                string impl;
                if (HandlerReturnType != "void")
                    impl = $"private {HandlerReturnType} {Name}__Mock__Impl({HandlerParameters}) {{ InvokeCounter[Muck.DynamicClassContentType.Event][\"{Name}_Invoke\"]++; return default({HandlerReturnType}); }}";
                else
                    impl = $"private {HandlerReturnType} {Name}__Mock__Impl({HandlerParameters}) {{ InvokeCounter[Muck.DynamicClassContentType.Event][\"{Name}_Invoke\"]++; }}";


                return string.Join("\r\n\t", new[]
                {
                    ilst,
                    evt,
                    cf,
                    inv,
                    impl
                });
            }
        }

        public class DynamicIndexer : IDynamicSourceItem
        {
            public string Name { get; } = "Indexer";
            public string Type { get; }
            public string Parameters { get; }
            public bool HasGetter { get; }
            public bool HasSetter { get; }
            public string DefaultGetter { get; }
            public string DefaultSetter { get; }

            public DynamicIndexer(string type, string parameters, bool hasGetter = true, bool hasSetter = true, string defaultGetter = null, string defaultSetter = null)
            {
                Type = type;
                Parameters = parameters;
                DefaultGetter = string.IsNullOrWhiteSpace(defaultGetter) ? "" : " = " + defaultGetter + ";";
                DefaultSetter = string.IsNullOrWhiteSpace(defaultSetter) ? "" : " = " + defaultSetter + ";";
                HasGetter = hasGetter;
                HasSetter = hasSetter;
            }

            public string Render()
            {
                return HasGetter
                    ? HasSetter
                        ? $"public {Type} this[{Parameters}] {{ get{{{DefaultGetter}}} set{{{DefaultSetter}}} }}"
                        : $"public {Type} this[{Parameters}] {{ get{{{DefaultGetter}}} }}"
                    : HasSetter
                        ? $"public {Type} this[{Parameters}] {{ set{{{DefaultSetter}}} }}"
                        : "";
            }
        }

        public class DynamicCounter : IDynamicSourceItem
        {
            public string Name { get; set; }

            public DynamicCounter(string name = "InvokeCounter")
            {
                Name = name;
            }

            public string Render()
            {
                var field = $"public {typeof(DynamicClassInvokeCounter).FullName} {Name} {{ get; }} = new {typeof(DynamicClassInvokeCounter).FullName}();";
                var indexer = $"public int this[{typeof(DynamicClassContentType).FullName} memberType, string name] {{ get{{return {Name}[memberType][name];}} set{{{Name}[memberType][name]=value;}} }}";
                return $"{field}";
            }
        }

        public class DynamicEvtMgr : IDynamicSourceItem
        {
            public string Name { get; set; }

            public DynamicEvtMgr(string name = "EvtMgr")
            {
                Name = name;
            }

            public string Render()
            {
                var field = $"public {typeof(EventController).FullName} {Name} {{ get; }} = new {typeof(EventController).FullName}();";

                return $"{field}";
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
                Type = type == "System.Void" ? "void" : type;
                Body = body ?? (Type == "void" ? "" : $"return default({Type});");
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
}