using System;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Demo.Domain;
using CliFx.Demo.Utils;
using CliFx.Exceptions;
using CliFx.Infrastructure;

namespace CliFx.Demo.Commands;

[Command("book add", Description = "Adds a book to the library.")]
public class BookAddCommand(LibraryProvider libraryProvider) : ICommand
{
    [CommandParameter(0, Name = "title", Description = "Book title.")]
    public required string Title { get; init; }

    [CommandOption("author", 'a', Description = "Book author.")]
    public required string Author { get; init; }

    [CommandOption("published", 'p', Description = "Book publish date.")]
    public DateTimeOffset Published { get; init; } =
        new(
            Random.Shared.Next(1800, 2020),
            Random.Shared.Next(1, 12),
            Random.Shared.Next(1, 28),
            Random.Shared.Next(1, 23),
            Random.Shared.Next(1, 59),
            Random.Shared.Next(1, 59),
            TimeSpan.Zero
        );

    [CommandOption("isbn", 'n', Description = "Book ISBN.")]
    public Isbn Isbn { get; init; } =
        new(
            Random.Shared.Next(0, 999),
            Random.Shared.Next(0, 99),
            Random.Shared.Next(0, 99999),
            Random.Shared.Next(0, 99),
            Random.Shared.Next(0, 9)
        );

    public ValueTask ExecuteAsync(IConsole console)
    {
        if (libraryProvider.TryGetBook(Title) is not null)
            throw new CommandException($"Book '{Title}' already exists.", 10);

        var book = new Book(Title, Author, Published, Isbn);
        libraryProvider.AddBook(book);

        console.WriteLine($"Book '{Title}' added.");
        console.WriteBook(book);

        return default;
    }
}
