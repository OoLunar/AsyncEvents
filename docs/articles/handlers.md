# Pre/Post Handlers

When calling `AsyncEvent<T>.InvokeAsync`, there are two parts to the invocation: the pre-handler and the post-handler. The pre-handler is called before the event is invoked, and the post-handler is called after the event is invoked. Both handlers are optional and by default do nothing. The pre-handler is useful for running checks before the event is invoked. All pre-handlers will be executed in order by the handler priority. If any pre-handler returns false, the post-handler will not be executed and the event will not be invoked.

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

Both of the above pre-handlers will always be executed, however the `MyPostHandler` will only be executed if `asyncEventArgs.MyProperty` is not null (as per the `EnsureNotNullPreHandler`).

## Updating Handlers

The `AsyncEvent<T>` object will expose the `PreHandlers` and `PostHandlers` properties as readonly dictionaries, which you can modify using the `Add`/`Remove` methods. After you've made your modifications, you must call the `Prepare` method, which will recompile the delegate internally used upon invocation.

```csharp
var myEvent = new AsyncEvent<MyAsyncEventArgs>();
myEvent.AddPreHandler(EnsureNotNullPreHandler);
myEvent.AddPreHandler(LogPreHandler, AsyncEventPriority.Highest);
myEvent.AddPostHandler(MyPostHandler);
myEvent.Prepare();
```

If the event is being invoked for the first time without being prepared, the `Prepare` method will be called automatically.

## Handler Priority

The `Add` methods have an optional parameter `priority` which is set to `AsyncEventPriority.Normal` by default. The priority is used to determine the order in which the pre-handlers are executed, with the highest priority being executed first. If two pre-handlers have the same priority, the order in which they are executed is undefined.

```csharp
var myEvent = new AsyncEvent<MyAsyncEventArgs>();

// This event will be executed first since it has the highest priority.
myEvent.AddPreHandler(LogPreHandler, AsyncEventPriority.Highest);

// This event will be executed second since it's priority is lower than LogPreHandler.
myEvent.AddPreHandler(EnsureNotNullPreHandler);
```

You may update the priority of an event handler by calling the `Add` method again with the same handler, but with a different priority. The `Prepare` method must be called after updating the priority.

## Register In Bulk

You can register multiple handlers (pre and post) in bulk using the `AddHandlers` method. This method will search a type for methods that match the signature of the event handler and add them to the event.

```csharp
var myEvent = new AsyncEvent<MyAsyncEventArgs>();
myEvent.AddHandlers(typeof(MyHandlers));
```

Alternatively, if you wish to register non-static methods, you can pass an instance of the class to the `AddHandlers` method.

```csharp
var myEvent = new AsyncEvent<MyAsyncEventArgs>();
var myHandlers = new MyHandlers();
myEvent.AddHandlers(myHandlers);
```

It should be noted that the `AddHandlers` method will reuse the same object when binding to the non-static handlers, so keep this in mind if the event handlers modify the object state.