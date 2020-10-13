namespace CliFx.Domain
{
    public interface IConverter
    {
        public object ConvertFrom(string value);
    }
}
