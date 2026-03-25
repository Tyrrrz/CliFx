using System;

namespace CliFx.Demo.Domain;

public partial record Isbn(
    int EanPrefix,
    int RegistrationGroup,
    int Registrant,
    int Publication,
    int CheckDigit
) : IFormattable
{
    // Implement IFormattable so CliFx can render ISBN values in the help text, such as when listing input defaults
    public string ToString(string? format, IFormatProvider? formatProvider) =>
        $"{EanPrefix:000}-{RegistrationGroup:00}-{Registrant:00000}-{Publication:00}-{CheckDigit:0}";

    public override string ToString() => ToString(null, null);
}

public partial record Isbn
{
    public static Isbn Parse(string value, IFormatProvider formatProvider)
    {
        var components = value.Split('-', 5, StringSplitOptions.RemoveEmptyEntries);

        return new Isbn(
            int.Parse(components[0], formatProvider),
            int.Parse(components[1], formatProvider),
            int.Parse(components[2], formatProvider),
            int.Parse(components[3], formatProvider),
            int.Parse(components[4], formatProvider)
        );
    }
}
