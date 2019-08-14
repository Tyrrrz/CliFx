using System;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Demo.Services;
using CliFx.Exceptions;
using CliFx.Services;

namespace CliFx.Demo.Commands
{
    [Command("book remove", Description = "Remove a book from the library.")]
    public class BookRemoveCommand : ICommand
    {
        private readonly LibraryService _libraryService;

        [CommandOption("title", 't', IsRequired = true, Description = "Book title.")]
        public string Title { get; set; }

        public BookRemoveCommand(LibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        public Task ExecuteAsync(IConsole console)
        {
            var book = _libraryService.GetBook(Title);

            if (book == null)
                throw new CommandErrorException(1, "Book not found.");

            _libraryService.RemoveBook(book);

            console.Output.WriteLine($"Book {Title} removed.");

            return Task.CompletedTask;
        }
    }
}