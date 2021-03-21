﻿using System;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Demo.Domain;
using CliFx.Demo.Utils;
using CliFx.Exceptions;
using CliFx.Infrastructure;

namespace CliFx.Demo.Commands
{
    [Command("book add", Description = "Add a book to the library.")]
    public partial class BookAddCommand : ICommand
    {
        private readonly LibraryProvider _libraryProvider;

        [CommandParameter(0, Name = "title", Description = "Book title.")]
        public string Title { get; init; } = "";

        [CommandOption("author", 'a', IsRequired = true, Description = "Book author.")]
        public string Author { get; init; } = "";

        [CommandOption("published", 'p', Description = "Book publish date.")]
        public DateTimeOffset Published { get; init; } = CreateRandomDate();

        [CommandOption("isbn", 'n', Description = "Book ISBN.")]
        public Isbn Isbn { get; init; } = CreateRandomIsbn();

        public BookAddCommand(LibraryProvider libraryProvider)
        {
            _libraryProvider = libraryProvider;
        }

        public ValueTask ExecuteAsync(IConsole console)
        {
            if (_libraryProvider.TryGetBook(Title) is not null)
                throw new CommandException("Book already exists.", 10);

            var book = new Book(Title, Author, Published, Isbn);
            _libraryProvider.AddBook(book);

            console.Output.WriteLine("Book added.");
            console.Output.WriteBook(book);

            return default;
        }
    }

    public partial class BookAddCommand
    {
        private static readonly Random Random = new();

        private static DateTimeOffset CreateRandomDate() => new(
            Random.Next(1800, 2020),
            Random.Next(1, 12),
            Random.Next(1, 28),
            Random.Next(1, 23),
            Random.Next(1, 59),
            Random.Next(1, 59),
            TimeSpan.Zero
        );

        private static Isbn CreateRandomIsbn() => new(
            Random.Next(0, 999),
            Random.Next(0, 99),
            Random.Next(0, 99999),
            Random.Next(0, 99),
            Random.Next(0, 9)
        );
    }
}