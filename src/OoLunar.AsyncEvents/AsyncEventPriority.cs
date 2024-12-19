namespace OoLunar.AsyncEvents
{
    /// <summary>
    /// An enumeration representing the priority of an asynchronous event handler.
    /// </summary>
    /// <remarks>
    /// The priority of an event handler determines the order in which it is executed.
    /// While you can use any integer value, it is recommended to use the predefined values for consistency.
    /// </remarks>
    public enum AsyncEventPriority
    {
        /// <summary>
        /// This event handler should be executed last.
        /// </summary>
        Lowest = -2,

        /// <summary>
        /// This event handler isn't as important as others.
        /// </summary>
        Low = -1,

        /// <summary>
        /// This is the default priority.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// This event handler is more important than others.
        /// </summary>
        High = 1,

        /// <summary>
        /// This event handler should be executed first.
        /// </summary>
        Highest = 2
    }
}
