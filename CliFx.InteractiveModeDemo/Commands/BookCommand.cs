using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.InteractiveModeDemo.Internal;
using CliFx.InteractiveModeDemo.Services;
using CliFx.Exceptions;

namespace CliFx.InteractiveModeDemo.Commands
{
    [Command("book", Description = "View, list, add or remove books.")]
    public class BookCommand : ICommand
    {
        private readonly LibraryService _libraryService;

        [CommandParameter(0, Name = "title", Description = "Book title.")]
        public string Title { get; set; } = "";

        public BookCommand(LibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        public ValueTask ExecuteAsync(IConsole console)
        {
            var book = _libraryService.GetBook(Title);

            if (book == null)
                throw new CommandException("Book not found.", 1);

            console.RenderBook(book);

            return default;
        }
    }
}