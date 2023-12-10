using System;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Demo.Domain;
using CliFx.Demo.Utils;
using CliFx.Exceptions;
using CliFx.Infrastructure;

namespace CliFx.Demo.Commands;

[Command("book add", Description = "Adds a book to the library.")]
public partial class BookAddCommand(LibraryProvider libraryProvider) : ICommand
{
    [CommandParameter(0, Name = "title", Description = "Book title.")]
    public required string Title { get; init; }

    [CommandOption("author", 'a', Description = "Book author.")]
    public required string Author { get; init; }

    [CommandOption("published", 'p', Description = "Book publish date.")]
    public DateTimeOffset Published { get; init; } = CreateRandomDate();

    [CommandOption("isbn", 'n', Description = "Book ISBN.")]
    public Isbn Isbn { get; init; } = CreateRandomIsbn();

    public ValueTask ExecuteAsync(IConsole console)
    {
        if (libraryProvider.TryGetBook(Title) is not null)
            throw new CommandException("Book already exists.", 10);

        var book = new Book(Title, Author, Published, Isbn);
        libraryProvider.AddBook(book);

        console.Output.WriteLine("Book added.");
        console.Output.WriteBook(book);

        return default;
    }
}

public partial class BookAddCommand
{
    private static readonly Random Random = new();

    private static DateTimeOffset CreateRandomDate() =>
        new(
            Random.Next(1800, 2020),
            Random.Next(1, 12),
            Random.Next(1, 28),
            Random.Next(1, 23),
            Random.Next(1, 59),
            Random.Next(1, 59),
            TimeSpan.Zero
        );

    private static Isbn CreateRandomIsbn() =>
        new(
            Random.Next(0, 999),
            Random.Next(0, 99),
            Random.Next(0, 99999),
            Random.Next(0, 99),
            Random.Next(0, 9)
        );
}
