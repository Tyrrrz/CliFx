using System;

namespace CliFx.Demo.Domain
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

        public override string ToString() =>
            $"{EanPrefix:000}-{RegistrationGroup:00}-{Registrant:00000}-{Publication:00}-{CheckDigit:0}";
    }

    public partial class Isbn
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
}