using System.Threading;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Demo.Internal;
using CliFx.Demo.Services;
using CliFx.Exceptions;
using CliFx.Services;

namespace CliFx.Demo.Commands
{
    [Command("book", Description = "View, list, add or remove books.")]
    public class BookCommand : ICommand
    {
        private readonly LibraryService _libraryService;

        [CommandOption("title", 't', IsRequired = true, Description = "Book title.")]
        public string Title { get; set; }

        public BookCommand(LibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        public Task ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        {
            var book = _libraryService.GetBook(Title);

            if (book == null)
                throw new CommandException("Book not found.", 1);

            console.RenderBook(book);

            return Task.CompletedTask;
        }
    }
}