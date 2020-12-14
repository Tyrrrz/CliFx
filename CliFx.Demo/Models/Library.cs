using System;
using System.Collections.Generic;

namespace CliFx.Demo.Models
{
    public partial class Library
    {
        public IReadOnlyList<Book> Books { get; }

        public Library(IReadOnlyList<Book> books)
        {
            Books = books;
        }
    }

    public partial class Library
    {
        public static Library Empty { get; } = new(Array.Empty<Book>());
    }
}