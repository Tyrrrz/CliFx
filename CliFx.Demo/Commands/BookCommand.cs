using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Demo.Domain;
using CliFx.Demo.Utils;
using CliFx.Exceptions;
using CliFx.Infrastructure;

namespace CliFx.Demo.Commands
{
    [Command("book", Description = "View, list, add or remove books.")]
    public class BookCommand : ICommand
    {
        private readonly LibraryProvider _libraryProvider;

        [CommandParameter(0, Name = "title", Description = "Book title.")]
        public string Title { get; init; } = "";

        public BookCommand(LibraryProvider libraryProvider)
        {
            _libraryProvider = libraryProvider;
        }

        public ValueTask ExecuteAsync(IConsole console)
        {
            var book = _libraryProvider.TryGetBook(Title);

            if (book is null)
                throw new CommandException("Book not found.", 10);

            console.RenderBook(book);

            return default;
        }
    }
}