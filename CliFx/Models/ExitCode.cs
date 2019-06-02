using System.Globalization;

namespace CliFx.Models
{
    public partial class ExitCode
    {
        public int Value { get; }

        public string Message { get; }

        public bool IsSuccess => Value == 0;

        public ExitCode(int value, string message = null)
        {
            Value = value;
            Message = message;
        }

        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    }

    public partial class ExitCode
    {
        public static ExitCode Success { get; } = new ExitCode(0);
    }
}