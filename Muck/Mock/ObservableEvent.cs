using System;
using System.Collections.Generic;
using System.Linq;

namespace Muck
{
    public class ObservableEvent<T> : List<T>, IObservableEvent
    {
        private delegate object InvokeDelegate(params object[] param);

        public ObservableEvent() : this(new T[] {})
        {
            
        }

        public ObservableEvent(T initialValue) : this(new [] {initialValue})
        {
            
        }

        public ObservableEvent(IEnumerable<T> initialValues) : this(initialValues.ToArray())
        {
            
        }
        public ObservableEvent(params T[] initialValues) : base(initialValues)
        {
            
        }
        public object Invoke(params object[] param)
        {
            object returnValue = null;
            foreach (var value in this)
            {
                returnValue = (value as Delegate)?.DynamicInvoke(param);
            }
            return returnValue;
        }
        public IAsyncResult BeginInvoke(object[] param, AsyncCallback acb, object state)
        {
            return ((InvokeDelegate) Invoke).BeginInvoke(param, acb, state);
        }
        public void EndInvoke(IAsyncResult ar)
        {
            ((InvokeDelegate) Invoke).EndInvoke(ar);
        }
    }
}