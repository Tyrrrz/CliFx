using System;
using System.Collections.Generic;
using System.Linq;

namespace CliFx.Demo.Domain
{
    public partial class Library
    {
        public IReadOnlyList<Book> Books { get; }

        public Library(IReadOnlyList<Book> books)
        {
            Books = books;
        }

        public Library WithBook(Book book)
        {
            var books = Books.ToList();
            books.Add(book);

            return new Library(books);
        }

        public Library WithoutBook(Book book)
        {
            var books = Books.Where(b => b != book).ToArray();

            return new Library(books);
        }
    }

    public partial class Library
    {
        public static Library Empty { get; } = new(Array.Empty<Book>());
    }
}