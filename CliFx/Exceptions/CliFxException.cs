using System;

namespace CliFx.Exceptions
{
    /// <summary>
    /// Domain exception thrown within CliFx.
    /// </summary>
    public class CliFxException : BaseCliFxException
    {
        /// <summary>
        /// Initializes an instance of <see cref="CliFxException"/>.
        /// </summary>
        public CliFxException(string? message, bool showHelp = false) 
            : base(message, showHelp)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CliFxException"/>.
        /// </summary>
        public CliFxException(string? message, Exception? innerException, bool showHelp = false) 
            : base(message, innerException, showHelp)
        {
        }
    }
}