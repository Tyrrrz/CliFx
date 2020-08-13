namespace CliFx.Input
{
    /// <summary>
    /// Stores command directive input.
    /// </summary>
    public class DirectiveInput
    {
        /// <summary>
        /// Directive name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes an instance of <see cref="DirectiveInput"/>.
        /// </summary>
        public DirectiveInput(string name)
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