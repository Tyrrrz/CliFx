using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Demo.Domain;
using CliFx.Exceptions;
using CliFx.Infrastructure;

namespace CliFx.Demo.Commands;

[Command("book remove", Description = "Remove a book from the library.")]
public class BookRemoveCommand : ICommand
{
    private readonly LibraryProvider _libraryProvider;

    [CommandParameter(0, Name = "title", Description = "Title of the book to remove.")]
    public required string Title { get; init; }

    public BookRemoveCommand(LibraryProvider libraryProvider)
    {
        _libraryProvider = libraryProvider;
    }

    public ValueTask ExecuteAsync(IConsole console)
    {
        var book = _libraryProvider.TryGetBook(Title);

        if (book is null)
            throw new CommandException("Book not found.", 10);

        _libraryProvider.RemoveBook(book);

        console.Output.WriteLine($"Book {Title} removed.");

        return default;
    }
}