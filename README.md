# OoLunar.AsyncEvents

Async events for .NET

## Features

- `Pre`/`Post` event handler support.
- `AsyncEventHandlerPriority` support.
- Mark your event handlers with `AsyncEventHandlerAttribute`.
- Add, modify, or remove your event handlers even after they've been prepared by calling `AsyncEvent<TEventArgs>.Prepare()` after making your changes.
- `AsyncEvent<TEventArgs>` is thread-safe and can be used in multi-threaded environments.
- A `AsyncEventContainer` to allow for easy management of multiple `AsyncEvent<TEventArgs>` instances and mass-preparation of all event handlers.

## Installation

`OoLunar.AsyncEvents` is available on [NuGet](https://www.nuget.org/packages/OoLunar.AsyncEvents/).

Web documentation can be found at [https://oolunar.github.io/AsyncEvents/](https://oolunar.github.io/AsyncEvents/).

## Usage

The async events design is based off of `DSharpPlus.Commands`'s context check system, and is designed to be used in a similar manner. There are two main concepts to understand: pre/post handlers, and the handler priority system.

### Pre/Post Handlers
Any method that is to be used as a pre or post handler must have the `AsyncEventHandler` attribute applied to it. This attribute takes an optional `HandlerPriority` parameter, which is an integer-based enum that determines the order in which handlers are executed. Handlers with higher/greater priority values are executed first. If two handlers have the same priority, the order in which they are executed is undefined.

```csharp
[AsyncEventHandler(HandlerPriority.Highest)]
public async ValueTask<bool> LogAsync(MyAsyncEventArgs asyncEventArgs)
{
    // Read any data from the event args
    // Can return a boolean to indicate whether the event should continue
    _logger.LogInformation("Event {EventName} fired", asyncEventArgs.EventName);
    return true;
}

[AsyncEventHandler]
public async ValueTask ExecuteAsync(MyAsyncEventArgs asyncEventArgs)
{
    // Do something
}
```

It is important to note that pre handlers can return a boolean value to indicate whether the event should continue. If any pre handler returns `false`, the post handlers will not be executed. All pre handlers will run regardless of whether any of them return `false`. The only exception to this is if a pre handler throws an exception, in which case the behavior is undefined and determined by the caller.