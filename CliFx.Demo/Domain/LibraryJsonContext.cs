using System.Text.Json.Serialization;

namespace CliFx.Demo.Domain;

[JsonSerializable(typeof(Library))]
public partial class LibraryJsonContext : JsonSerializerContext;
