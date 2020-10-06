﻿using System;
using System.Collections.Generic;

namespace CliFx
{
    /// <summary>
    /// Configuration of an application.
    /// </summary>
    public class ApplicationConfiguration
    {
        /// <summary>
        /// Command types defined in this application.
        /// </summary>
        public IReadOnlyList<Type> CommandTypes { get; }

        /// <summary>
        /// Whether debug mode is allowed in this application.
        /// </summary>
        public bool IsDebugModeAllowed { get; }

        /// <summary>
        /// Whether preview mode is allowed in this application.
        /// </summary>
        public bool IsPreviewModeAllowed { get; }

        /// <summary>
        /// Initializes an instance of <see cref="ApplicationConfiguration"/>.
        /// </summary>
        public ApplicationConfiguration(
            IReadOnlyList<Type> commandTypes,
            bool isDebugModeAllowed,
            bool isPreviewModeAllowed)
        {
            CommandTypes = commandTypes;
            IsDebugModeAllowed = isDebugModeAllowed;
            IsPreviewModeAllowed = isPreviewModeAllowed;
        }
    }
}