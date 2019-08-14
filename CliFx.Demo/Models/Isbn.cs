using System;
using System.Globalization;

namespace CliFx.Demo.Models
{
    public partial class Isbn
    {
        public int EanPrefix { get; }

        public int RegistrationGroup { get; }

        public int Registrant { get; }

        public int Publication { get; }

        public int CheckDigit { get; }

        public Isbn(int eanPrefix, int registrationGroup, int registrant, int publication, int checkDigit)
        {
            EanPrefix = eanPrefix;
            RegistrationGroup = registrationGroup;
            Registrant = registrant;
            Publication = publication;
            CheckDigit = checkDigit;
        }

        public override string ToString() => $"{EanPrefix:000}-{RegistrationGroup:00}-{Registrant:00000}-{Publication:00}-{CheckDigit:0}";
    }

    public partial class Isbn
    {
        public static Isbn Parse(string value)
        {
            var components = value.Split('-', 5, StringSplitOptions.RemoveEmptyEntries);

            return new Isbn(
                int.Parse(components[0], CultureInfo.InvariantCulture),
                int.Parse(components[1], CultureInfo.InvariantCulture),
                int.Parse(components[2], CultureInfo.InvariantCulture),
                int.Parse(components[3], CultureInfo.InvariantCulture),
                int.Parse(components[4], CultureInfo.InvariantCulture));
        }
    }
}