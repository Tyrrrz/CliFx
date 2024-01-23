using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Demo.Domain;
using CliFx.Exceptions;
using CliFx.Infrastructure;

namespace CliFx.Demo.Commands;

[Command("book remove", Description = "Removes a book from the library.")]
public class BookRemoveCommand(LibraryProvider libraryProvider) : ICommand
{
    [CommandParameter(0, Name = "title", Description = "Title of the book to remove.")]
    public required string Title { get; init; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        var book = libraryProvider.TryGetBook(Title);

        if (book is null)
            throw new CommandException($"Book '{Title}' not found.", 10);

        libraryProvider.RemoveBook(book);

        console.WriteLine($"Book '{Title}' removed.");

        return default;
    }
}
