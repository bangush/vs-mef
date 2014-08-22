﻿namespace Microsoft.VisualStudio.Composition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Validation;

    /// <summary>
    /// Static factory methods for creating .NET Lazy{T} instances.
    /// </summary>
    internal static class LazyServices
    {
        private static readonly MethodInfo createStronglyTypedLazyOfTM = typeof(LazyServices).GetMethod("CreateStronglyTypedLazyOfTM", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo createStronglyTypedLazyOfT = typeof(LazyServices).GetMethod("CreateStronglyTypedLazyOfT", BindingFlags.NonPublic | BindingFlags.Static);

        internal static readonly Type DefaultMetadataViewType = typeof(IDictionary<string, object>);
        internal static readonly Type DefaultExportedValueType = typeof(object);

        /// <summary>
        /// Gets a value indicating whether a type is a Lazy`1 or Lazy`2 type.
        /// </summary>
        /// <param name="type">The type to be tested.</param>
        /// <returns><c>true</c> if <paramref name="type"/> is some Lazy type.</returns>
        internal static bool IsAnyLazyType(this Type type)
        {
            if (type.IsGenericType)
            {
                Type genericTypeDefinition = type.GetGenericTypeDefinition();
                if (typeof(Lazy<>) == genericTypeDefinition || typeof(Lazy<,>) == genericTypeDefinition)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Creates a factory that takes a Func{object} and object-typed metadata
        /// and returns a strongly-typed Lazy{T, TMetadata} instance.
        /// </summary>
        /// <param name="exportType">The type of values created by the Func{object} value factories. Null is interpreted to be <c>typeof(object)</c>.</param>
        /// <param name="metadataViewType">The type of metadata passed to the lazy factory. Null is interpreted to be <c>typeof(IDictionary<string, object>)</c>.</param>
        /// <returns>A function that takes a Func{object} value factory and metadata, and produces a Lazy{T, TMetadata} instance.</returns>
        internal static Func<Func<object>, object, object> CreateStronglyTypedLazyFactory(Type exportType, Type metadataViewType)
        {
            MethodInfo genericMethod;
            if (metadataViewType != null)
            {
                genericMethod = createStronglyTypedLazyOfTM.MakeGenericMethod(exportType ?? DefaultExportedValueType, metadataViewType);
            }
            else
            {
                genericMethod = createStronglyTypedLazyOfT.MakeGenericMethod(exportType ?? DefaultExportedValueType);
            }

            return (Func<Func<object>, object, object>)Delegate.CreateDelegate(typeof(Func<Func<object>, object, object>), genericMethod);
        }

        internal static Func<T> AsFunc<T>(this Lazy<T> lazy)
        {
            Requires.NotNull(lazy, "lazy");

            return new Func<T>(lazy.GetLazyValue);
        }

        private static T GetLazyValue<T>(this Lazy<T> lazy)
        {
            return lazy.Value;
        }

        /// <summary>
        /// Initializes a Lazy instance with a value factory that takes one argument
        /// (for the cost of a delegate, but without incurring the cost of a closure).
        /// </summary>
        /// <typeparam name="TArg">The type of argument to be passed to the value factory. If a value type, this will be boxed.</typeparam>
        /// <typeparam name="T">The type of value created by the value factory.</typeparam>
        /// <param name="valueFactory">The value factory.</param>
        /// <param name="arg">The argument to be passed to the value factory.</param>
        /// <returns>The constructed Lazy instance.</returns>
        private static Lazy<T> FromFactory<TArg, T>(Func<TArg, T> valueFactory, TArg arg)
        {
            return new Lazy<T>(valueFactory.PresupplyArgument(arg), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// Initializes a Lazy instance with a value factory that takes one argument
        /// (for the cost of a delegate, but without incurring the cost of a closure).
        /// </summary>
        /// <typeparam name="TArg">The type of argument to be passed to the value factory. If a value type, this will be boxed.</typeparam>
        /// <typeparam name="T">The type of value created by the value factory.</typeparam>
        /// <typeparam name="TMetadata">The type of metadata exposed by the Lazy instance.</typeparam>
        /// <param name="valueFactory">The value factory.</param>
        /// <param name="arg">The argument to be passed to the value factory.</param>
        /// <param name="metadata">The metadata to pass to the Lazy instance.</param>
        /// <returns>The constructed Lazy instance.</returns>
        private static Lazy<T, TMetadata> FromFactory<TArg, T, TMetadata>(Func<TArg, T> valueFactory, TArg arg, TMetadata metadata)
        {
            return new Lazy<T, TMetadata>(valueFactory.PresupplyArgument(arg), metadata, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private static Lazy<T> CreateStronglyTypedLazyOfT<T>(Func<object> funcOfObject, object metadata)
        {
            Requires.NotNull(funcOfObject, "funcOfObject");

            return FromFactory(Helper<T>.CastResultToTFunc, funcOfObject);
        }

        private static Lazy<T, TMetadata> CreateStronglyTypedLazyOfTM<T, TMetadata>(Func<object> funcOfObject, object metadata)
        {
            Requires.NotNull(funcOfObject, "funcOfObject");
            Requires.NotNullAllowStructs(metadata, "metadata");

            return FromFactory(Helper<T>.CastResultToTFunc, funcOfObject, (TMetadata)metadata);
        }

        private static class Helper<T>
        {
            internal static readonly Func<Func<object>, T> CastResultToTFunc = f => (T)f();
            internal static readonly Func<Lazy<T>, T> GetLazyValueFunc = l => l.Value;
        }
    }
}
