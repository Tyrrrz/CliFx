namespace CliFx
{
    internal interface IArgumentValueValidator
    {
        ValidationResult Validate(object value);
    }
}
