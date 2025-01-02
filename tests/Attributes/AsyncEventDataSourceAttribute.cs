using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoLunar.AsyncEvents.DebugAsyncEvents;
using OoLunar.AsyncEvents.ParallelAsyncEvents;
using OoLunar.AsyncEvents.Tests.Data;

namespace OoLunar.AsyncEvents.Tests.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class AsyncEventDataSourceAttribute : Attribute, ITestDataSource
    {
        public bool IncludeDebugEvent { get; init; }

        public IEnumerable<object?[]> GetData(MethodInfo methodInfo)
        {
            AsyncEvent<TestAsyncEventArgs> asyncEvent = new();
            yield return [asyncEvent];

            ParallelAsyncEvent<TestAsyncEventArgs> parallelAsyncEvent = new();
            yield return [parallelAsyncEvent];

            if (IncludeDebugEvent)
            {
                DebugAsyncEvent<TestAsyncEventArgs> debugAsyncEvent = new(new AsyncEvent<TestAsyncEventArgs>(), NullLogger<DebugAsyncEvent<TestAsyncEventArgs>>.Instance);
                yield return [debugAsyncEvent];
            }
        }

        public string? GetDisplayName(MethodInfo methodInfo, object?[]? data) => $"{methodInfo.Name}({data?[0]!.GetType().Name})";
    }
}
