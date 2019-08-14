using CliFx.Internal;

namespace CliFx.Models
{
    /// <summary>
    /// Metadata associated with an application.
    /// </summary>
    public class ApplicationMetadata
    {
        /// <summary>
        /// Application title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Application executable name.
        /// </summary>
        public string ExecutableName { get; }

        /// <summary>
        /// Application version text.
        /// </summary>
        public string VersionText { get; }

        /// <summary>
        /// Initializes an instance of <see cref="ApplicationMetadata"/>.
        /// </summary>
        public ApplicationMetadata(string title, string executableName, string versionText)
        {
            Title = title.GuardNotNull(nameof(title));
            ExecutableName = executableName.GuardNotNull(nameof(executableName));
            VersionText = versionText.GuardNotNull(nameof(versionText));
        }
    }
}