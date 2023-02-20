using System;

namespace CliFx.Demo.Domain;

public record Book(string Title, string Author, DateTimeOffset Published, Isbn Isbn);