namespace CliFx.Models
{
    public class ApplicationMetadata
    {
        public string Title { get; }

        public string ExecutableName { get; }

        public string VersionText { get; }

        public ApplicationMetadata(string title, string executableName, string versionText)
        {
            Title = title;
            ExecutableName = executableName;
            VersionText = versionText;
        }
    }
}