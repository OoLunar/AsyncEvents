# Async Event Containers

The `AsyncEventContainer` class is a container for multiple `AsyncEvent<T>` objects. It allows you to add and remove event handlers for multiple events at once while caching the events for later. This object is useful for avoiding the clutter of having multiple `AsyncEvent<T>` objects in your class. Alternatively, it could be used for scoped scenarios when you want to manage multiple of the same type of events. Below, we have our handlers:

```csharp
public ValueTask<bool> EnsureNotNullPreHandler(MyAsyncEventArgs asyncEventArgs)
    => new ValueTask<bool>(asyncEventArgs.MyProperty is not null);

public ValueTask<bool> LogPreHandler(MyAsyncEventArgs asyncEventArgs)
{
    _logger.LogInformation("Event {EventName} is about to be invoked", asyncEventArgs.GetType().Name);
    return new ValueTask<bool>(true);
}

public ValueTask MyPostHandler(MyAsyncEventArgs asyncEventArgs)
{
    _logger.LogInformation("Event {EventName} has been invoked", asyncEventArgs.GetType().Name);
    return default;
}
```

And we register them with the `AsyncEventContainer`:

```csharp
AsyncEventContainer<MyAsyncEventArgs> container = new();
container.AddPreHandler(EnsureNotNullPreHandler);
container.AddPreHandler(LogPreHandler, AsyncEventPriority.Highest);
container.AddPostHandler(MyPostHandler);

// Alternatively, we can add handlers in bulk.
container.AddHandlers<MyHandlers>();
```

And request the event from the container:

```csharp
var myEvent = container.GetAsyncEvent<MyAsyncEventArgs>();
```

The `AsyncEventContainer` class is thread-safe and can be used in a multi-threaded environment. When requesting an event from the container, the container will create a new event if it doesn't exist, or return the existing event if it does. The container will also prepare the event if it hasn't been prepared yet. If there are no event handlers for a requested event, it will return an empty event handler which immediately returns on invocation. As with the `AsyncEvent<T>` object, the `Prepare<T>` method must be called on the `AsyncEventContainer` object after updating the handlers.