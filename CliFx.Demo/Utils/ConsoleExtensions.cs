using System;
using CliFx.Demo.Domain;
using CliFx.Infrastructure;

namespace CliFx.Demo.Utils;

internal static class ConsoleExtensions
{
    public static void WriteBook(this ConsoleWriter writer, Book book)
    {
        // Title
        using (writer.Console.WithForegroundColor(ConsoleColor.White))
            writer.WriteLine(book.Title);

        // Author
        writer.Write("  ");
        writer.Write("Author: ");

        using (writer.Console.WithForegroundColor(ConsoleColor.White))
            writer.WriteLine(book.Author);

        // Published
        writer.Write("  ");
        writer.Write("Published: ");

        using (writer.Console.WithForegroundColor(ConsoleColor.White))
            writer.WriteLine($"{book.Published:d}");

        // ISBN
        writer.Write("  ");
        writer.Write("ISBN: ");

        using (writer.Console.WithForegroundColor(ConsoleColor.White))
            writer.WriteLine(book.Isbn);
    }
}