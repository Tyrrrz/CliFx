using System;
using System.Threading;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Demo.Internal;
using CliFx.Demo.Models;
using CliFx.Demo.Services;
using CliFx.Exceptions;
using CliFx.Services;

namespace CliFx.Demo.Commands
{
    [Command("book add", Description = "Add a book to the library.")]
    public partial class BookAddCommand : ICommand
    {
        private readonly LibraryService _libraryService;

        [CommandOption("title", 't', IsRequired = true, Description = "Book title.")]
        public string Title { get; set; }

        [CommandOption("author", 'a', IsRequired = true, Description = "Book author.")]
        public string Author { get; set; }

        [CommandOption("published", 'p', Description = "Book publish date.")]
        public DateTimeOffset Published { get; set; }

        [CommandOption("isbn", 'n', Description = "Book ISBN.")]
        public Isbn Isbn { get; set; }

        public BookAddCommand(LibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        public Task ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        {
            // To make the demo simpler, we will just generate random publish date and ISBN if they were not set
            if (Published == default)
                Published = CreateRandomDate();
            if (Isbn == default)
                Isbn = CreateRandomIsbn();

            if (_libraryService.GetBook(Title) != null)
                throw new CommandException("Book already exists.", 1);

            var book = new Book(Title, Author, Published, Isbn);
            _libraryService.AddBook(book);

            console.Output.WriteLine("Book added.");
            console.RenderBook(book);

            return Task.CompletedTask;
        }
    }

    public partial class BookAddCommand
    {
        private static readonly Random Random = new Random();

        private static DateTimeOffset CreateRandomDate() => new DateTimeOffset(
            Random.Next(1800, 2020),
            Random.Next(1, 12),
            Random.Next(1, 28),
            Random.Next(1, 23),
            Random.Next(1, 59),
            Random.Next(1, 59),
            TimeSpan.Zero);

        public static Isbn CreateRandomIsbn() => new Isbn(
            Random.Next(0, 999),
            Random.Next(0, 99),
            Random.Next(0, 99999),
            Random.Next(0, 99),
            Random.Next(0, 9));
    }
}