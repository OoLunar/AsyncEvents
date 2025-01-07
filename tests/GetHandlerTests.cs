using System.Reflection.Emit;
using System.Reflection;
using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoLunar.AsyncEvents.Tests.Data;
using System.Collections.Generic;

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
            // List to hold dynamically created types
            List<Type> types = [];

            // Create 5000 dynamic types with a unique property for each
            for (int i = 0; i < 5000; i++)
            {
                types.Add(CreateDynamicType(i));
            }

            IAsyncEvent asyncEvent;
            await Parallel.ForAsync(0, types.Count, (i, _) =>
            {
                AsyncEventArgs instance = Activator.CreateInstance(types[i]) as AsyncEventArgs
                    ?? throw new TypeAccessException();
                asyncEvent = _container.GetAsyncEvent(instance.GetType());
                return ValueTask.CompletedTask;
            });

        }

        // Create a dynamic type with a property
        private Type CreateDynamicType(int index)
        {
            // Create a dynamic assembly and module
            AssemblyName assemblyName = new AssemblyName("DynamicTypesAssembly");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

            // Define a type that inherits from BaseClass
            TypeBuilder typeBuilder = moduleBuilder.DefineType(
                $"DynamicType{index}",
                TypeAttributes.Public,
                typeof(AsyncEventArgs));

            // Define a method "CustomMethod" in the dynamic type
            MethodBuilder customMethodBuilder = typeBuilder.DefineMethod(
                "CustomMethod",
                MethodAttributes.Public | MethodAttributes.Virtual,
                typeof(void),
                Type.EmptyTypes);

            ILGenerator ilGenerator = customMethodBuilder.GetILGenerator();
            ilGenerator.EmitWriteLine($"Dynamic Type {index} custom method executed.");
            ilGenerator.Emit(OpCodes.Ret);

            // Create the type
            Type dynamicType = typeBuilder.CreateType();

            return dynamicType;
        }
    }
}
