using CliFx.Attributes;
using CliFx.Extensibility;

namespace CliFx.Tests.Commands.Invalid
{
    [Command]
    public class InvalidCustomValidatorParameterCommand : SelfSerializeCommandBase
    {
        [CommandParameter(0, Validators = new[] { typeof(Validator) })]
        public string? Param { get; set; }

        public class Validator
        {
            public ValidationResult Validate(string value) => ValidationResult.Ok();
        }
    }
}