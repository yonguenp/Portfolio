public interface EventListenerBase { };

public interface EventListener<T> : EventListenerBase
{
    void OnEvent(T eventType);
}
