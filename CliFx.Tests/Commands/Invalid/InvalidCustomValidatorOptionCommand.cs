using CliFx.Attributes;
using CliFx.Extensibility;

namespace CliFx.Tests.Commands.Invalid
{
    [Command]
    public class InvalidCustomValidatorOptionCommand : SelfSerializeCommandBase
    {
        [CommandOption('f', Validators = new[] { typeof(Validator) })]
        public string? Option { get; set; }

        public class Validator
        {
            public ValidationResult Validate(string value) => ValidationResult.Ok();
        }
    }
}