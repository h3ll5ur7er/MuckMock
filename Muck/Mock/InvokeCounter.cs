using System.Collections.Generic;
using System.Linq;

namespace Muck
{
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
}