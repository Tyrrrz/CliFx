using System;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.InteractiveModeDemo.Internal;
using CliFx.InteractiveModeDemo.Models;
using CliFx.InteractiveModeDemo.Services;
using CliFx.Exceptions;

namespace CliFx.InteractiveModeDemo.Commands
{
    [Command("book add", Description = "Add a book to the library.",
            Manual = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer euismod nunc lorem, vitae cursus sem facilisis ut. Cras et nibh justo. Mauris eu elit lectus. Suspendisse potenti. Mauris luctus sapien quis arcu semper, vel venenatis elit ultrices. Quisque suscipit arcu vel massa vestibulum dapibus. Maecenas felis lacus, pharetra sed fermentum in, molestie vel ipsum. Nullam elementum arcu eget est tempor, blandit lacinia odio facilisis. Proin nulla odio, sodales et tellus nec, pulvinar ultrices nunc. Integer ornare, odio vel tincidunt congue, diam lorem facilisis lectus, id tempor sapien nibh vitae justo. Mauris ut odio justo. Etiam sed felis tellus. Nam sollicitudin neque in tempor scelerisque. Praesent sit amet nisi quis justo scelerisque placerat.")]
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