using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Demo.Domain;
using CliFx.Demo.Utils;
using CliFx.Infrastructure;

namespace CliFx.Demo.Commands
{
    [Command("book list", Description = "List all books in the library.")]
    public class BookListCommand : ICommand
    {
        private readonly LibraryProvider _libraryProvider;

        public BookListCommand(LibraryProvider libraryProvider)
        {
            _libraryProvider = libraryProvider;
        }

        public ValueTask ExecuteAsync(IConsole console)
        {
            var library = _libraryProvider.GetLibrary();

            var isFirst = true;
            foreach (var book in library.Books)
            {
                // Margin
                if (!isFirst)
                {
                    console.Output.WriteLine();
                    isFirst = false;
                }

                // Render book
                console.RenderBook(book);
            }

            return default;
        }
    }
}