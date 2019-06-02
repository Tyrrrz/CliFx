namespace CliFx.Services
{
    public static class Extensions
    {
        public static Command ResolveCommand(this ICommandResolver commandResolver) => commandResolver.ResolveCommand(new string[0]);
    }
}