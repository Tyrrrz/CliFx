# CliFx

[![Build](https://img.shields.io/appveyor/ci/Tyrrrz/CliFx/master.svg)](https://ci.appveyor.com/project/Tyrrrz/CliFx/branch/master)
[![Tests](https://img.shields.io/appveyor/tests/Tyrrrz/CliFx/master.svg)](https://ci.appveyor.com/project/Tyrrrz/CliFx/branch/master/tests)
[![Coverage](https://img.shields.io/codecov/c/gh/Tyrrrz/CliFx/master.svg)](https://codecov.io/gh/Tyrrrz/CliFx)
[![NuGet](https://img.shields.io/nuget/v/CliFx.svg)](https://nuget.org/packages/CliFx)
[![NuGet](https://img.shields.io/nuget/dt/CliFx.svg)](https://nuget.org/packages/CliFx)
[![Donate](https://img.shields.io/badge/patreon-donate-yellow.svg)](https://patreon.com/tyrrrz)
[![Donate](https://img.shields.io/badge/buymeacoffee-donate-yellow.svg)](https://buymeacoffee.com/tyrrrz)

CliFx is a simple to use but powerful framework for building command line applications. Its primary goal is to completely take over the user input layer, letting you focus more on writing your application. This framework uses a declarative approach for defining commands, avoiding excessive boilerplate code and complex configurations.

_CliFx is to command line interfaces what ASP.NET Core is to web applications._

## Download

- [NuGet](https://nuget.org/packages/CliFx): `dotnet add package CliFx`
- [Continuous integration](https://ci.appveyor.com/project/Tyrrrz/CliFx)

## Features

- Complete application framework, not just an argument parser
- Requires minimal amount of code to get started
- Resolves commands using attributes
- Handles options of various types, including custom types
- Supports multi-level command hierarchies
- Generates contextual help text
- Prints errors and routes exit codes on exceptions
- Highly testable and easy to customize
- Targets .NET Framework 4.6+ and .NET Standard 2.0+
- No external dependencies

### Currently not implemented

- Positional arguments (anonymous options)
- Auto-completion support
- Environment variables
- Runtime directives

## Usage

### Configuring application

To turn your application into a command line interface you need to change your program's `Main` method so that it delegates execution to `CliApplication`.

The following code will create and run default `CliApplication` that will resolve commands defined in the calling assembly. Using fluent interface provided by `CliApplicationBuilder` you can easily configure different aspects of your application.

```c#
public static class Program
{
    public static Task<int> Main(string[] args) =>
        new CliApplicationBuilder()
            .AddCommandsFromThisAssembly()
            .Build()
            .RunAsync(args);
}
```

### Defining commands

In order to add functionality to your application you need to define commands. Commands are essentially entry points for the user to interact with your application. You can think of them as something similar to controllers in ASP.NET Core applications.

In CliFx you define a command by making a new class that implements `ICommand` and annotating it with `CommandAttribute`. To specify properties that will be set from command line you need to annotate them with `CommandOptionAttribute`.

Here's an example command that calculates logarithm. It has a name (`"log"`) which the user needs to specify in order to invoke it. It also contains two options, the source value (`"value"`/`'v'`) and logarithm base (`"base"`/`'b'`).

```c#
[Command("log", Description = "Calculate the logarithm of a value.")]
public class LogCommand : ICommand
{
    [CommandOption("value", 'v', IsRequired = true, Description = "Value whose logarithm is to be found.")]
    public double Value { get; set; }

    [CommandOption("base", 'b', Description = "Logarithm base.")]
    public double Base { get; set; } = 10;

    public Task ExecuteAsync(IConsole console)
    {
        var result = Math.Log(Value, Base);
        console.Output.WriteLine(result);

        return Task.CompletedTask;
    }
}
```

By implementing `ICommand` this class also provides `ExecuteAsync` method. This is the method that gets called when the user invokes the command. Its return type is `Task` in order to facilitate asynchronous execution, but if your command runs synchronously you can simply return `Task.CompletedTask`.

The `ExecuteAsync` method also takes an instance of `IConsole` as a parameter. You should use this abstraction to interact with the console instead of calling `System.Console` so that your commands are testable.

Finally, the command defined above can be executed from the command line in one of the following ways:

- `myapp log -v 1000`
- `myapp log --value 8 --base 2`
- `myapp log -v 81 -b 3`

### Option conversion

When resolving options, CliFx can convert string values obtained from the command line to any of the following types:

- Standard types
  - Primitive types (`int`, `bool`, `double`, `ulong`, `char`, etc)
  - Date and time types (`DateTime`, `DateTimeOffset`, `TimeSpan`)
  - Enum types
- String-initializable types
  - Types with constructor that accepts a single `string` parameter (`FileInfo`, `DirectoryInfo`, etc)
  - Types with static method `Parse` that accepts a single `string` parameter and an `IFormatProvider` parameter
  - Types with static method `Parse` that accepts a single `string` parameter
- Nullable versions of all above types (`decimal?`, `TimeSpan?`, etc)
- Collections of all above types
  - Array types (`T[]`)
  - Types that are assignable from arrays (`IReadOnlyList<T>`, `ICollection<T>`, etc)
  - Types with constructor that accepts a single `T[]` parameter (`HashSet<T>`, `List<T>`, etc)

If you want to define an option of your own type, the easiest way to do it is to make sure that your type is string-initializable, as explained above.

### Dependency injection

CliFx uses an implementation of `ICommandFactory` to initialize commands and by default it only works with types that have parameterless constructors.

In real-life scenarios your commands will most likely have dependencies that need to be injected. CliFx doesn't have its own dependency container but it's really easy to set it up to use any 3rd party dependency container of your choice.

For example, here is how you would configure your application to use [`Microsoft.Extensions.DependencyInjection`](https://nuget.org/packages/Microsoft.Extensions.DependencyInjection) (aka the built-in dependency container in ASP.NET Core).

```c#
public static class Program
{
    public static Task<int> Main(string[] args)
    {
        var services = new ServiceCollection();

        // Register services
        services.AddSingleton<MyService>();

        // Register commands
        services.AddTransient<MyCommand>();

        var serviceProvider = services.BuildServiceProvider();

        return new CliApplicationBuilder()
            .AddCommandsFromThisAssembly()
            .UseCommandFactory(schema => (ICommand) serviceProvider.GetRequiredService(schema.Type))
            .Build()
            .RunAsync(args);
    }
}
```

### Resolve commands from other assemblies

In most cases, your commands will be defined in your main assembly which is where CliFx will look if you initialize the application using the following code.

```c#
var app = new CliApplicationBuilder().AddCommandsFromThisAssembly().Build();
```

If you want to configure your application to resolve specific commands or commands from another assembly you can use `AddCommand` and `AddCommandsFrom` methods for that.

```c#
var app = new CliApplicationBuilder()
    .AddCommand(typeof(CommandA)) // include CommandA specifically
    .AddCommand(typeof(CommandB)) // include CommandB specifically
    .AddCommandsFrom(typeof(CommandC).Assembly) // include all commands from assembly that contains CommandC
    .Build();
```

### Child commands

In a more complex application you may need to build a hierarchy of commands. CliFx takes the approach of resolving hierarchy at runtime based on command names, so you don't have to specify any parent-child relationships explicitly.

If you have a command `"cmd"` and you want to define commands `"sub1"` and `"sub2"` as its children, simply name them accordingly.

```c#
[Command("cmd")]
public class ParentCommand : ICommand
{
    // ...
}

[Command("cmd sub1")]
public class FirstSubCommand : ICommand
{
    // ...
}

[Command("cmd sub2")]
public class SecondSubCommand : ICommand
{
    // ...
}
```

### Reporting errors

You may have noticed that commands in CliFx don't return exit codes. This is by design as exit codes are considered a higher-level concern and thus handled by `CliApplication`, not by individual commands.

Commands can report execution failure simply by throwing exceptions just like any other C# code. When an exception is thrown, `CliApplication` will catch it, print the error, and return an appropriate exit code to the calling process.

If you want to communicate a specific error through exit code, you can throw an instance of `CommandException` which takes exit code as a constructor parameter.

```c#
[Command]
public class DivideCommand : ICommand
{
    [CommandOption("dividend", IsRequired = true)]
    public double Dividend { get; set; }

    [CommandOption("divisor", IsRequired = true)]
    public double Divisor { get; set; }

    public Task ExecuteAsync(IConsole console)
    {
        if (Math.Abs(Divisor) < double.Epsilon)
        {
            // This will print the error and set exit code to 1337
            throw new CommandException("Division by zero is not supported.", 1337);
        }

        var result = Dividend / Divisor;
        console.Output.WriteLine(result);

        return Task.CompletedTask;
    }
}
```

### Testing

CliFx makes it really easy to test your commands thanks to the `IConsole` interface.

When writing tests, you can use `VirtualConsole` which lets you provide your own streams in place of your application's stdin, stdout and stderr. It has multiple constructor overloads allowing you to specify the exact set of streams that you want. Streams that are not provided are replaced with stubs, i.e. `VirtualConsole` doesn't leak to `System.Console` in any way.

Let's assume you want to test a simple command such as this one.

```c#
[Command]
public class ConcatCommand : ICommand
{
    [CommandOption("left")]
    public string Left { get; set; } = "Hello";

    [CommandOption("right")]
    public string Right { get; set; } = "world";

    public Task ExecuteAsync(IConsole console)
    {
        console.Output.Write(Left);
        console.Output.Write(' ');
        console.Output.Write(Right);

        return Task.CompletedTask;
    }
}
```

By substituting `IConsole` you can write your test cases like this.

```c#
[Test]
public async Task ConcatCommand_Test()
{
    // Arrange
    using (var stdout = new StringWriter())
    {
        var console = new VirtualConsole(stdout);

        var command = new ConcatCommand
        {
            Left = "foo",
            Right = "bar"
        };

        // Act
        await command.ExecuteAsync(console);

        // Assert
        Assert.That(stdout.ToString(), Is.EqualTo("foo bar"));
    }
}
```

And if you want, you can even test the whole application in a similar fashion.

```c#
[Test]
public async Task ConcatCommand_Test()
{
    // Arrange
    using (var stdout = new StringWriter())
    {
        var console = new VirtualConsole(stdout);

        var app = new CliApplicationBuilder()
            .AddCommand(typeof(ConcatCommand))
            .UseConsole(console)
            .Build();

        var args = new[] {"--left", "foo", "--right", "bar"};

        // Act
        await app.RunAsync(args);

        // Assert
        Assert.That(stdout.ToString(), Is.EqualTo("foo bar"));
    }
}
```

## Benchmarks

CliFx has the smallest performance overhead compared to other command line parsers and frameworks.
Below you can see a table comparing execution times of a simple command across different libraries.

```ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.14393.0 (1607/AnniversaryUpdate/Redstone1)
Intel Core i5-4460 CPU 3.20GHz (Haswell), 1 CPU, 4 logical and 4 physical cores
Frequency=3125008 Hz, Resolution=319.9992 ns, Timer=TSC
.NET Core SDK=2.2.401
  [Host] : .NET Core 2.2.6 (CoreCLR 4.6.27817.03, CoreFX 4.6.27818.02), 64bit RyuJIT
  Core   : .NET Core 2.2.6 (CoreCLR 4.6.27817.03, CoreFX 4.6.27818.02), 64bit RyuJIT

Job=Core  Runtime=Core  

```
|                               Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Rank |
|------------------------------------- |----------:|----------:|----------:|------:|--------:|-----:|
|                                CliFx |  39.47 us | 0.7490 us | 0.9198 us |  1.00 |    0.00 |    1 |
|                   System.CommandLine | 153.98 us | 0.7112 us | 0.6652 us |  3.90 |    0.09 |    2 |
| McMaster.Extensions.CommandLineUtils | 180.36 us | 3.5893 us | 6.7416 us |  4.59 |    0.16 |    3 |
|                            PowerArgs | 427.54 us | 6.9006 us | 6.4548 us | 10.82 |    0.26 |    4 |

## Libraries used

- [NUnit](https://github.com/nunit/nunit)
- [CliWrap](https://github.com/Tyrrrz/CliWrap)
- [FluentAssertions](https://github.com/fluentassertions/fluentassertions)
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet)
- [Coverlet](https://github.com/tonerdo/coverlet)

## Donate

If you really like my projects and want to support me, consider donating to me on [Patreon](https://patreon.com/tyrrrz) or [BuyMeACoffee](https://buymeacoffee.com/tyrrrz). All donations are optional and are greatly appreciated. 🙏