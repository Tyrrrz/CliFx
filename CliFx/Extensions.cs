using System.Threading.Tasks;

namespace CliFx
{
    public static class Extensions
    {
        public static Task<int> RunAsync(this ICliApplication application) => application.RunAsync(new string[0]);
    }
}