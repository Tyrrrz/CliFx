namespace CliFx.Internal
{
    internal static class GenericExtensions
    {
        public static bool IsTypeDefaultValue(this object obj) =>
            Equals(obj, obj.GetType().GetDefaultValue());
    }
}