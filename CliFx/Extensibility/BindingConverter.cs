﻿namespace CliFx.Extensibility
{
    // Used internally to simplify usage from reflection
    internal interface IBindingConverter
    {
        public object? Convert(string? rawValue);
    }

    // TODO: how do we handle non-scalars?
    /// <summary>
    /// Base type for custom converters.
    /// </summary>
    public abstract class BindingConverter<T> : IBindingConverter
    {
        /// <summary>
        /// Parses value from a raw command line argument.
        /// </summary>
        public abstract T Convert(string? rawValue);

        object? IBindingConverter.Convert(string? rawValue) => Convert(rawValue);
    }
}