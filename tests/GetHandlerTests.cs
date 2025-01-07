using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoLunar.AsyncEvents.Tests.Data;

namespace OoLunar.AsyncEvents.Tests
{
    [TestClass]
    public sealed class GetHandlerTests
    {
        private readonly AsyncEventContainer _container;

        public GetHandlerTests()
        {
            _container = new();
            _container.AddHandlers<EventHandlers>(new EventHandlers());
        }

        [TestMethod]
        public async ValueTask GetHandler_Generic_MultithreadedAsync()
        {
            AsyncEvent<TestAsyncEventArgs> asyncEvent;
            await Parallel.ForAsync(0, 1000, async (_, _) =>
            {
                asyncEvent = _container.GetAsyncEvent<TestAsyncEventArgs>();
                await asyncEvent.InvokeAsync(new TestAsyncEventArgs());
            });
        }

        [TestMethod]
        public async ValueTask GetHandler_NonGeneric_MultithreadedAsync()
        {
            IAsyncEvent asyncEvent;
            await Parallel.ForAsync(0, 1000, async (_, _) =>
            {
                asyncEvent = _container.GetAsyncEvent(typeof(TestAsyncEventArgs));
                await asyncEvent.InvokeAsync(new TestAsyncEventArgs());
            });
        }

        [TestMethod]
        public async ValueTask GetHandler_NonGeneric_Multithreaded_DynamicTypesAsync()
        {
            // Create 5000 dynamic types with a unique property for each
            List<Type> types = CreateDynamicTypes(5000);

            // Create a list to store all async events
            List<IAsyncEvent> asyncEvents = [];
            await Parallel.ForAsync(0, types.Count, (i, _) =>
            {
                asyncEvents.Add(_container.GetAsyncEvent(types[i]));
                return ValueTask.CompletedTask;
            });

            // Assert that all async events are unique
            for (int i = 0; i < asyncEvents.Count; i++)
            {
                for (int j = i + 1; j < asyncEvents.Count; j++)
                {
                    Assert.AreNotEqual(asyncEvents[i], asyncEvents[j]);
                }
            }
        }

        // Create a dynamic type with a property
        private static List<Type> CreateDynamicTypes(int totalCount)
        {
            // Create a dynamic assembly and reuse it for all dynamic types
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("DynamicTypesAssembly"), AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

            List<Type> types = [];
            for (int i = 0; i < totalCount; i++)
            {
                // Define a type that inherits from AsyncEventArgs
                TypeBuilder typeBuilder = moduleBuilder.DefineType($"DynamicType_{i}", TypeAttributes.Public, typeof(AsyncEventArgs));

                // Create the type
                types.Add(typeBuilder.CreateType());
            }

            return types;
        }
    }
}
