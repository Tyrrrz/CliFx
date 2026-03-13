using System.Linq;
using System.Threading.Tasks;
using CliFx.Demo.Domain;
using CliFx.Infrastructure;

namespace CliFx.Demo.Commands;

[Command("book list", Description = "Lists all books in the library.")]
public partial class BookListCommand(LibraryProvider libraryProvider) : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        var library = libraryProvider.GetLibrary();
        foreach (var (i, book) in library.Books.Index())
        {
            if (i != 0)
                console.WriteLine();

            console.WriteBook(book);
        }

        return default;
    }
}
