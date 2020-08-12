namespace CliFx.Domain.Input
{
    /// <summary>
    /// Stores command directive input.
    /// </summary>
    public class CommandDirectiveInput
    {
        /// <summary>
        /// Directive name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandDirectiveInput"/>.
        /// </summary>
        public CommandDirectiveInput(string name)
        {
            Name = name;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Concat("[", Name, "]");
        }
    }
}