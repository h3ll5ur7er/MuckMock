using System.Collections.Generic;

namespace Muck
{
    public class EventController : Dictionary<string, IObservableEvent>
    {
        public new IObservableEvent this[string key]
        {
            get
            {
                return !ContainsKey(key) ? null : base[key];
            }
            set
            {
                if(!ContainsKey(key))
                    Add(key, value);
                else
                    base[key]=value;
            }
        }
    }
}