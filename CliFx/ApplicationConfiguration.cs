using System;
using System.Collections.Generic;
using CliFx.Internal;

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
        /// Custom directives defined in this application.
        /// </summary>
        public IReadOnlyList<string> CustomDirectives { get; }

        /// <summary>
        /// Whether interactive mode is allowed in this application.
        /// </summary>
        public bool IsInteractiveModeAllowed { get; }

        /// <summary>
        /// Whether debug mode is allowed in this application.
        /// </summary>
        public bool IsDebugModeAllowed { get; }

        /// <summary>
        /// Whether preview mode is allowed in this application.
        /// </summary>
        public bool IsPreviewModeAllowed { get; }

        /// <summary>
        /// Whether help manual text has a fixed length specified by <seealso cref="ManualWidth"/> or dynamic defined as a percentage of console width.
        /// </summary>
        public bool IsManualFixedWidth { get; }

        /// <summary>
        /// Specifies manual width when <seealso cref="ManualWidth"/> is set to true or a percentage of console width (1-100).
        /// </summary>
        public int ManualWidth { get; }

        /// <summary>
        /// Initializes an instance of <see cref="ApplicationConfiguration"/>.
        /// </summary>
        public ApplicationConfiguration(
            IReadOnlyList<Type> commandTypes,
            IReadOnlyList<string> customDirectives,
            bool isDebugModeAllowed,
            bool isPreviewModeAllowed,
            bool isInteractiveModeAllowed,
            bool isManualFixedWidth,
            int manualWidth)
        {
            CommandTypes = commandTypes;
            CustomDirectives = customDirectives;

            IsDebugModeAllowed = isDebugModeAllowed;
            IsPreviewModeAllowed = isPreviewModeAllowed;
            IsInteractiveModeAllowed = isInteractiveModeAllowed;

            IsManualFixedWidth = isManualFixedWidth;
            ManualWidth = IsManualFixedWidth ? manualWidth : MathUtils.Clamp(manualWidth, 1, 100);
        }
    }
}