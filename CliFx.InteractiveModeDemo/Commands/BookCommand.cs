using System;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.InteractiveModeDemo.Internal;
using CliFx.InteractiveModeDemo.Services;

namespace CliFx.InteractiveModeDemo.Commands
{
    [Command("book", Description = "List all books in the library.")]
    public class BookCommand : ICommand
    {
        private readonly LibraryService _libraryService;

        public BookCommand(LibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        public ValueTask ExecuteAsync(IConsole console)
        {
            var library = _libraryService.GetLibrary();

            var isFirst = true;
            foreach (var book in library.Books)
            {
                // Margin
                if (!isFirst)
                    console.Output.WriteLine();
                isFirst = false;

                // Render book
                console.RenderBook(book);
            }

            if (isFirst)
                console.WithForegroundColor(ConsoleColor.Red, () => console.Output.WriteLine("No books"));

            return default;
        }
    }
}