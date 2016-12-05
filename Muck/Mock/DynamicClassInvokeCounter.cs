using System.Collections.Generic;
using System.Linq;

namespace Muck
{
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
}