using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Demo.Domain;
using CliFx.Demo.Utils;
using CliFx.Exceptions;
using CliFx.Infrastructure;

namespace CliFx.Demo.Commands
{
    [Command("book", Description = "Retrieve a book from the library.")]
    public class BookCommand : ICommand
    {
        private readonly LibraryProvider _libraryProvider;

        [CommandParameter(0, Name = "title", Description = "Title of the book to retrieve.")]
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

            console.Output.WriteBook(book);

            return default;
        }
    }
}