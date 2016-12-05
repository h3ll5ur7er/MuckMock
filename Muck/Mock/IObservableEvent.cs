using System;

namespace Muck
{
    public interface IObservableEvent
    {
        object Invoke(params object[] param);
        IAsyncResult BeginInvoke(object[] param, AsyncCallback acb, object state);
        void EndInvoke(IAsyncResult ar);
    }
}