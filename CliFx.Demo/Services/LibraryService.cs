using System.IO;
using System.Linq;
using CliFx.Demo.Models;
using Newtonsoft.Json;

namespace CliFx.Demo.Services
{
    public class LibraryService
    {
        private string StorageFilePath => Path.Combine(Directory.GetCurrentDirectory(), "Data.json");

        private void StoreLibrary(Library library)
        {
            string data = JsonConvert.SerializeObject(library);
            File.WriteAllText(StorageFilePath, data);
        }

        public Library GetLibrary()
        {
            if (!File.Exists(StorageFilePath))
                return Library.Empty;

            string data = File.ReadAllText(StorageFilePath);

            return JsonConvert.DeserializeObject<Library>(data);
        }

        public Book? GetBook(string title)
        {
            return GetLibrary().Books.FirstOrDefault(b => b.Title == title);
        }

        public void AddBook(Book book)
        {
            Library updatedLibrary = GetLibrary().WithBook(book);
            StoreLibrary(updatedLibrary);
        }

        public void RemoveBook(Book book)
        {
            Library updatedLibrary = GetLibrary().WithoutBook(book);
            StoreLibrary(updatedLibrary);
        }
    }
}