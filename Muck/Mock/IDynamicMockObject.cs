namespace Muck
{
    public interface IDynamicMockObject
    {
        DynamicClassInvokeCounter InvokeCounter { get; }
        EventController EvtMgr { get; }
    }
}