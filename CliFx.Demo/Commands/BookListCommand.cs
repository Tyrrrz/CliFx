using System.Threading;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Demo.Internal;
using CliFx.Demo.Services;
using CliFx.Services;

namespace CliFx.Demo.Commands
{
    [Command("book list", Description = "List all books in the library.")]
    public class BookListCommand : ICommand
    {
        private readonly LibraryService _libraryService;

        public BookListCommand(LibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        public Task ExecuteAsync(IConsole console)
        {
            var library = _libraryService.GetLibrary();

            var isFirst = true;
            foreach (var book in library.Books)
            {
                // Margin
                if (!isFirst)
                    console.Output.WriteLine();
                isFirst = false;

                // Render book
                console.RenderBook(book);
            }

            return Task.CompletedTask;
        }
    }
}