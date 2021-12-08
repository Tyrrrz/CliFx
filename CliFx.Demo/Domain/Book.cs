using System;

namespace CliFx.Demo.Domain;

public class Book
{
    public string Title { get; }

    public string Author { get; }

    public DateTimeOffset Published { get; }

    public Isbn Isbn { get; }

    public Book(string title, string author, DateTimeOffset published, Isbn isbn)
    {
        Title = title;
        Author = author;
        Published = published;
        Isbn = isbn;
    }
}