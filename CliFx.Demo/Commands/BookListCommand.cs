using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Demo.Domain;
using CliFx.Demo.Utils;
using CliFx.Infrastructure;

namespace CliFx.Demo.Commands;

[Command("book list", Description = "Lists all books in the library.")]
public class BookListCommand(LibraryProvider libraryProvider) : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        var library = libraryProvider.GetLibrary();

        for (var i = 0; i < library.Books.Count; i++)
        {
            // Add margin
            if (i != 0)
                console.WriteLine();

            // Render book
            var book = library.Books[i];
            console.WriteBook(book);
        }

        return default;
    }
}
