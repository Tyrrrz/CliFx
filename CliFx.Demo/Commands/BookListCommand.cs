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

            for (var i = 0; i < library.Books.Count; i++)
            {
                // Add margin
                if (i != 0)
                    console.Output.WriteLine();

                // Render book
                var book = library.Books[i];
                console.Output.WriteBook(book);
            }

            return default;
        }
    }
}