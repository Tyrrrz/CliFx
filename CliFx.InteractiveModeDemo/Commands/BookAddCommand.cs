using System;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Demo.Internal;
using CliFx.Demo.Models;
using CliFx.Demo.Services;
using CliFx.Exceptions;

namespace CliFx.Demo.Commands
{
    [Command("book add", Description = "Add a book to the library.")]
    public partial class BookAddCommand : ICommand
    {
        private readonly LibraryService _libraryService;

        [CommandParameter(0, Name = "title", Description = "Book title.")]
        public string Title { get; set; } = "";

        [CommandOption("author", 'a', IsRequired = true, Description = "Book author.")]
        public string Author { get; set; } = "";

        [CommandOption("published", 'p', Description = "Book publish date.")]
        public DateTimeOffset Published { get; set; } = CreateRandomDate();

        [CommandOption("isbn", 'n', Description = "Book ISBN.")]
        public Isbn Isbn { get; set; } = CreateRandomIsbn();

        public BookAddCommand(LibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        public ValueTask ExecuteAsync(IConsole console)
        {
            if (_libraryService.GetBook(Title) != null)
                throw new CommandException("Book already exists.", 1);

            var book = new Book(Title, Author, Published, Isbn);
            _libraryService.AddBook(book);

            console.Output.WriteLine("Book added.");
            console.RenderBook(book);

            return default;
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

        private static Isbn CreateRandomIsbn() => new Isbn(
            Random.Next(0, 999),
            Random.Next(0, 99),
            Random.Next(0, 99999),
            Random.Next(0, 99),
            Random.Next(0, 9));
    }
}