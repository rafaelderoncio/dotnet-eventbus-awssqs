using Project.EventDriven.SQS.EventBus.Events;
using Project.EventDriven.SQS.EventBus.Handlers;

namespace Project.EventDriven.SQS.EventBus.Managers;

public class InMemorySubscriptionManager : ISubscriptionManager
{
    private readonly Dictionary<string, List<SubscriptionInfo>> _handlers = new();
    private readonly List<Type> _eventTypes = new();

    public void Clear()
        => _handlers.Clear();

    public bool IsEmpty()
        => !_handlers.Any();

    public bool HasSubscriptionForEvent<T>(string topic = null) where T : Event
        => _handlers.ContainsKey(topic ?? typeof(T).Name) && _handlers[topic ?? typeof(T).Name].Any();

    public void AddSubscription<T, TH>(string topic = null)
        where T : Event
        where TH : IEventHandler<T>
    {
        if (HasSubscriptionForEvent<T>(topic))
            return;

        string eventName = topic ?? typeof(T).Name;
        _handlers[eventName] = new List<SubscriptionInfo>() { new SubscriptionInfo(typeof(TH)) };
    }

    public void RemoveSubscription<T, TH>(string topic = null)
        where T : Event
        where TH : IEventHandler<T>
    {
        string eventName = topic ?? typeof(T).Name;

        if (!_handlers.ContainsKey(eventName))
            return;

        SubscriptionInfo subscription = _handlers[eventName].FirstOrDefault(s => s.HandlerType == typeof(TH));

        if (subscription != null)
        {
            _handlers[eventName].Remove(subscription);

            if (!_handlers[eventName].Any())
                _handlers.Remove(eventName);
        }
    }

    public IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>(string topic = null) where T : Event
    {
        string eventName = topic ?? typeof(T).Name;

        if (!_handlers.ContainsKey(eventName))
            return Enumerable.Empty<SubscriptionInfo>();

        return _handlers[eventName];
    }
}
