using System.IO;
using System.Linq;
using System.Text.Json;

namespace CliFx.Demo.Domain;

public class LibraryProvider
{
    private static string StorageFilePath { get; } =
        Path.Combine(Directory.GetCurrentDirectory(), "Library.json");

    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        TypeInfoResolver = Library.JsonContext.Default,
    };

    private void StoreLibrary(Library library)
    {
        var data = JsonSerializer.Serialize(library, _serializerOptions);
        File.WriteAllText(StorageFilePath, data);
    }

    public Library GetLibrary()
    {
        if (!File.Exists(StorageFilePath))
            return Library.Empty;

        var data = File.ReadAllText(StorageFilePath);

        return JsonSerializer.Deserialize<Library>(data, _serializerOptions) ?? Library.Empty;
    }

    public Book? TryGetBook(string title) =>
        GetLibrary().Books.FirstOrDefault(b => b.Title == title);

    public void AddBook(Book book)
    {
        var updatedLibrary = GetLibrary().WithBook(book);
        StoreLibrary(updatedLibrary);
    }

    public void RemoveBook(Book book)
    {
        var updatedLibrary = GetLibrary().WithoutBook(book);
        StoreLibrary(updatedLibrary);
    }
}
