using System;

namespace OoLunar.AsyncEvents
{
    /// <summary>
    /// Marks a method/delegate as an async event handler, optionally specifying the priority.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate, AllowMultiple = false)]
    public class AsyncEventHandlerAttribute : Attribute
    {
        /// <summary>
        /// Describes the importance of the event handler, which is used to determine the order in which handlers are invoked.
        /// </summary>
        public AsyncEventPriority Priority { get; init; }

        /// <summary>
        /// Creates a new instance of <see cref="AsyncEventHandlerAttribute"/> with the specified priority.
        /// </summary>
        /// <param name="priority">The priority of the event handler.</param>
        public AsyncEventHandlerAttribute(AsyncEventPriority priority = AsyncEventPriority.Normal) => Priority = priority;
    }
}
