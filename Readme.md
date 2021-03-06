# CliFx

[![Build](https://github.com/Tyrrrz/CliFx/workflows/CI/badge.svg?branch=master)](https://github.com/Tyrrrz/CliFx/actions)
[![Coverage](https://codecov.io/gh/Tyrrrz/CliFx/branch/master/graph/badge.svg)](https://codecov.io/gh/Tyrrrz/CliFx)
[![Version](https://img.shields.io/nuget/v/CliFx.svg)](https://nuget.org/packages/CliFx)
[![Downloads](https://img.shields.io/nuget/dt/CliFx.svg)](https://nuget.org/packages/CliFx)
[![Donate](https://img.shields.io/badge/donate-$$$-purple.svg)](https://tyrrrz.me/donate)

✅ **Project status: active**.

CliFx is a simple to use, yet powerful framework for building command line applications. Its primary goal is to completely take over the user input layer, letting you forget about the infrastructure and instead focus on writing your application. This framework uses a declarative class-first approach for defining commands, avoiding excessive boilerplate code and complex configurations.

## Download

- [NuGet](https://nuget.org/packages/CliFx): `dotnet add package CliFx`

## Features

- Complete application framework, not just an argument parser
- Requires minimal amount of code to get started
- Configuration via attributes
- Handles conversions to various types, including custom types
- Supports multi-level command hierarchies
- Exposes raw input, output, error streams to handle binary data
- Allows graceful command cancellation
- Prints errors and routes exit codes on exceptions
- Provides comprehensive and colorful auto-generated help text
- Highly testable and easy to debug
- Comes with built-in analyzers to help catch common mistakes
- Works with .NET Standard 2.0+, .NET Core 2.0+, .NET Framework 4.6.1+
- No external dependencies

## Screenshots

![help screen](.screenshots/help.png)

## Usage

- [Quick start](#quick-start)
- [Binding arguments](#binding-arguments)
- [Argument syntax](#argument-syntax)
- [Value conversion](#value-conversion)
- [Multiple commands](#multiple-commands)
- [Reporting errors](#reporting-errors)
- [Graceful cancellation](#graceful-cancellation)
- [Dependency injection](#dependency-injection)
- [Testing](#testing)
- [Debug and preview mode](#debug-and-preview-mode)
- [Environment variables](#environment-variables)

### Quick start

![quick start animated](https://i.imgur.com/uouNh2u.gif)

To turn your application into a command line interface you need to change your program's `Main` method so that it delegates execution to `CliApplication`. This is how to do it:

```c#
public static class Program
{
    public static async Task<int> Main() =>
        await new CliApplicationBuilder()
            .AddCommandsFromThisAssembly()
            .Build()
            .RunAsync();
}
```

The above code will create and run a `CliApplication` that will resolve commands defined in the calling assembly. Using fluent interface provided by `CliApplicationBuilder` you can also easily configure other aspects of your application.

In order to add functionality, however, you need to define at least one command. Commands are essentially entry points through which the user can interact with your application. You can think of them as something similar to controllers in ASP.NET Core.

To define a command, just create a new class that implements the `ICommand` interface and annotate it with `[Command]` attribute:

```c#
[Command]
public class HelloWorldCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine("Hello world!");

        // Return empty task because our command executes synchronously
        return default;
    }
}
```

To implement `ICommand`, the class needs to define an `ExecuteAsync()` method. This is the method that gets called by CliFx when the user runs the application.

To facilitate both asynchronous and synchronous execution, this method returns a `ValueTask`. Since the simple command above executes synchronously, we can just put `return default` at the end. In an asynchronous command, however, we would use the `async`/`await` keywords instead.

As a parameter, this method takes an instance of `IConsole`, an abstraction around the system console. You should use this abstraction in places where you would normally interact with `System.Console`, in order to make your command testable.

With this basic setup, the user can execute your application and get a greeting in return:

```sh
> myapp.exe

Hello world!
```

Out of the box, your application now also supports the built-in help and version options:

```sh
> myapp.exe --help

MyApp v1.0

Usage
  myapp.exe

Options
  -h|--help         Shows help text.
  --version         Shows version information.
```

```sh
> myapp.exe --version

v1.0
```

### Binding arguments

Commands can be configured to take input from command line arguments. To do that, we need to add properties to the command and annotate them with special attributes.

In CliFx, there are two types of argument bindings: **parameters** and **options**. Parameters are positional arguments that are identified by the order they appear in, while options are arguments identified by their names.

Here's an example command that calculates a logarithm of a value, which uses both a parameter and an option:

```c#
[Command]
public class LogCommand : ICommand
{
    [CommandParameter(0, Description = "Value whose logarithm is to be found.")]
    public double Value { get; set; }

    [CommandOption("base", 'b', Description = "Logarithm base.")]
    public double Base { get; set; } = 10;

    public ValueTask ExecuteAsync(IConsole console)
    {
        var result = Math.Log(Value, Base);
        console.Output.WriteLine(result);

        return default;
    }
}
```

The above command has two inputs:

- `Value` which is a parameter with order `0`.
- `Base` which is an option with name `base` and short name `b`.

Let's try running `--help` to see how this command is supposed to be used:

```sh
> myapp.exe --help

MyApp v1.0

Usage
  myapp.exe <value> [options]

Parameters
* value             Value whose logarithm is to be found.

Options
  -b|--base         Logarithm base. Default: "10".
  -h|--help         Shows help text.
  --version         Shows version information.
```

As we can see, in order to execute this command, at a minimum, the user has to supply a value:

```sh
> myapp.exe 10000

4
```

They can also set the non-required `base` option to override the default logarithm base of 10:

```sh
> myapp.exe 729 -b 3

6
```

```sh
> myapp.exe 123 --base 4.5

3.199426017362198
```

On the other hand, if the user fails to provide the parameter, they will get an error, as parameters are always required:

```sh
> myapp.exe -b 10

Missing value for parameter <value>.
```

Overall, the difference between parameters and options is as follows:

- Parameters are identified by their relative order. Options are identified by two dashes followed by their name, or a single dash followed by their short name (single character).
- Parameters can't be optional. Options are usually optional (as evident by the name), but can be configured to be required as well.
- Parameters technically have a name, but it's only used in the help text.
- Options can be configured to use the value of an environment variable as a fallback.
- Both parameters and options can take multiple values, but such a parameter must be last in order to avoid ambiguity. Options are not limited in this aspect.

As a general guideline, prefer to use parameters for required inputs that the command can't work without. Use options for non-required inputs, or when the command has too many required inputs, or when specifying the option name explicitly provides a better user experience.

### Argument syntax

This library supports an argument syntax which is based on the POSIX standard. To be fair, nobody really knows what the standard is about and very few tools actually follow it to the letter, so for the purpose of having dashes and spaces, CliFx is using the "standard command line syntax".

More specifically, the following examples are all valid:

- `myapp --foo bar` sets option `"foo"` to value `"bar"`
- `myapp -f bar` sets option `'f'` to value `"bar"`
- `myapp --switch` sets option `"switch"` without value
- `myapp -s` sets option `'s'` without value
- `myapp -abc` sets options `'a'`, `'b'` and `'c'` without value
- `myapp -xqf bar` sets options `'x'` and `'q'` without value, and option `'f'` to value `"bar"`
- `myapp -i file1.txt file2.txt` sets option `'i'` to a sequence of values `"file1.txt"` and `"file2.txt"`
- `myapp -i file1.txt -i file2.txt` sets option `'i'` to a sequence of values `"file1.txt"` and `"file2.txt"`
- `myapp cmd abc -o` routes to command `cmd` (assuming it exists) with parameter `abc` and sets option `'o'` without value

Argument parsing in CliFx aims to be as deterministic as possible, ideally yielding the same result no matter the context. The only context-sensitive part in the parser is the command name resolution which needs to know what commands are available in order to discern between arguments that correspond to the command name and arguments which are parameters.

An option is always parsed the same way, regardless of the arity of the actual property it's bound to. This means that `myapp -i file1.txt file2.txt` will _always_ be parsed as an option set to multiple values, even if the underlying property is not enumerable. For the same reason, unseparated arguments such as `myapp -ofile` will be treated as five distinct options `'o'`, `'f'`, `'i'`, `'l'`, `'e'`, instead of `'o'` being set to `"file"`.

Because of these rules, order of arguments is semantically important and it always goes like this:

```ini
{directives} {command name} {parameters} {options}
```

The above design makes the usage of your applications a lot more intuitive and predictable, providing a better end-user experience.

### Value conversion

Parameters and options can have different underlying types:

- Standard types
  - Primitive types (`int`, `bool`, `double`, `ulong`, `char`, etc.)
  - Date and time types (`DateTime`, `DateTimeOffset`, `TimeSpan`)
  - Enum types (converted from either name or value)
- String-initializable types
  - Types with a constructor that accepts a single `string` parameter (`FileInfo`, `DirectoryInfo`, etc.)
  - Types with a static method `Parse` that accepts a single `string` parameter (and optionally `IFormatProvider`)
- Any other type if a custom converter is specified
- Nullable versions of all above types (`decimal?`, `TimeSpan?`, etc.)
- Collections of all above types
  - Array types (`T[]`)
  - Types that are assignable from arrays (`IReadOnlyList<T>`, `ICollection<T>`, etc.)
  - Types with a constructor that accepts a single `T[]` parameter (`HashSet<T>`, `List<T>`, etc.)

When defining a parameter of an enumerable type, keep in mind that it has to be the only such parameter and it must be the last in order. Options, on the other hand, don't have this limitation.

- Example command with a custom converter:

```c#
// Maps 2D vectors from AxB notation
public class VectorConverter : ArgumentValueConverter<Vector2>
{
    public override Vector2 ConvertFrom(string value)
    {
        var components = value.Split('x', 'X', ';');
        var x = int.Parse(components[0], CultureInfo.InvariantCulture);
        var y = int.Parse(components[1], CultureInfo.InvariantCulture);
        return new Vector2(x, y);
    }
}

[Command]
public class SurfaceCalculatorCommand : ICommand
{
    // Custom converter is used to map raw argument values
    [CommandParameter(0, Converter = typeof(VectorConverter))]
    public Vector2 PointA { get; set; }

    [CommandParameter(1, Converter = typeof(VectorConverter))]
    public Vector2 PointB { get; set; }

    [CommandParameter(2, Converter = typeof(VectorConverter))]
    public Vector2 PointC { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        var a = (PointB - PointA).Length();
        var b = (PointC - PointB).Length();
        var c = (PointA - PointC).Length();

        // Heron's formula
        var p = (a + b + c) / 2;
        var surface = Math.Sqrt(p * (p - a) * (p - b) * (p - c));

        console.Output.WriteLine($"Triangle surface area: {surface}");

        return default;
    }
}
```

```sh
> myapp.exe 0x0 0x18 24x0

Triangle surface area: 216
```

- Example command with an array-backed parameter:

```c#
[Command]
public class FileSizeCalculatorCommand : ICommand
{
    // FileInfo is string-initializable and IReadOnlyList<T> can be assgined from an array,
    // so the value of this property can be mapped from a sequence of arguments.
    [CommandParameter(0)]
    public IReadOnlyList<FileInfo> Files { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        var totalSize = Files.Sum(f => f.Length);

        console.Output.WriteLine($"Total file size: {totalSize} bytes");

        return default;
    }
}
```

```sh
> myapp.exe file1.bin file2.exe

Total file size: 186368 bytes
```

Same command, but using an option for the list of files instead:

```c#
[Command]
public class FileSizeCalculatorCommand : ICommand
{
    [CommandOption("files")]
    public IReadOnlyList<FileInfo> Files { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        var totalSize = Files.Sum(f => f.Length);

        console.Output.WriteLine($"Total file size: {totalSize} bytes");

        return default;
    }
}
```

```sh
> myapp.exe --files file1.bin file2.exe

Total file size: 186368 bytes
```

### Multiple commands

Complex command line applications may have more than a single command in order to facilitate different workflows. In even more complex applications there may be multiple levels of commands, forming a hierarchy.

Whichever case it is, CliFx takes care of everything for you. All you need to do is specify appropriate command names in the attributes:

```c#
// Default command, i.e. command without a name
[Command]
public class DefaultCommand : ICommand
{
    // ...
}

// Child of default command
[Command("cmd1")]
public class FirstCommand : ICommand
{
    // ...
}

// Child of default command
[Command("cmd2")]
public class SecondCommand : ICommand
{
    // ...
}

// Child of FirstCommand
[Command("cmd1 sub")]
public class SubCommand : ICommand
{
    // ...
}
```

There is no limit to the number of commands or the level of their nesting. Once configured, the user can execute a specific command by typing its name before any other arguments, e.g. `myapp.exe cmd1 arg1 -p 42`.

Requesting help on the application above will show:

```sh
> myapp.exe --help

MyApp v1.0

Usage
  myapp.exe [command]

Options
  -h|--help         Shows help text.
  --version         Shows version information.

Commands
  cmd1
  cmd2

You can run `myapp.exe [command] --help` to show help on a specific command.
```

As you can see, only two commands are listed here because `cmd1 sub` is not an immediate child of the default command. We can further refine our help query to get information on `cmd1`:

```sh
> myapp.exe cmd1 --help

Usage
  myapp.exe cmd1 [command]

Options
  -h|--help         Shows help text.

Commands
  sub

You can run `myapp.exe cmd1 [command] --help` to show help on a specific command.
```

In a multi-command application you may also choose to not have a default command and only use named commands. If that's the case, running an application without parameters will simply print help text.

### Reporting errors

You may have noticed that commands in CliFx don't return exit codes. This is by design as exit codes are considered an infrastructural concern and thus handled by `CliApplication`, not by individual commands.

Commands can instead report execution failure by throwing an instance of `CommandException`. Using this exception, you can specify the message printed to stderr and the returned exit code.

Here is an example:

```c#
[Command]
public class DivideCommand : ICommand
{
    [CommandOption("dividend", IsRequired = true)]
    public double Dividend { get; set; }

    [CommandOption("divisor", IsRequired = true)]
    public double Divisor { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        if (Math.Abs(Divisor) < double.Epsilon)
        {
            // This will print the error and set exit code to 133
            throw new CommandException("Division by zero is not supported.", 133);
        }

        var result = Dividend / Divisor;
        console.Output.WriteLine(result);

        return default;
    }
}
```

```sh
> myapp.exe --dividend 10 --divisor 0

Division by zero is not supported.


> $LastExitCode

133
```

You can also specify the `showHelp` parameter to instruct whether to show the help text for the current command after printing the error:

```c#
[Command]
public class ExampleCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        throw new CommandException("Something went wrong.", showHelp: true);
    }
}
```

> Note: Unix systems rely on 8-bit unsigned integers for exit codes, so it's strongly recommended to use values between `1` and `255` when specifying exit code, in order to avoid potential overflow issues.

### Graceful cancellation

The user may abort execution by sending an interrupt signal (Ctrl+C or Ctrl+Break). If your command has critical disposable resources, you can intercept this signal to perform cleanup before exiting.

In order to make a command cancellation-aware, all you need to do is call `console.GetCancellationToken()`. This method returns a `CancellationToken` that will trigger when the user issues an interrupt signal.

Note that any operation which precedes `console.GetCancellationToken()` will not be cancellation-aware and as such will not delay the process termination. Calling this method multiple times is fine, as it will always return the same token.

Here's an example of a command that supports cancellation:

```c#
[Command("cancel")]
public class CancellableCommand : ICommand
{
    private async ValueTask DoSomethingAsync(CancellationToken cancellation)
    {
        await Task.Delay(TimeSpan.FromMinutes(10), cancellation);
    }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        // Make the command cancellation-aware
        var cancellation = console.GetCancellationToken();

        // Execute some long-running cancellable operation
        await DoSomethingAsync(cancellation);

        console.Output.WriteLine("Done.");
    }
}
```

Keep in mind that a command may delay cancellation only once. If the user decides to send an interrupt signal for the second time, the execution will be terminated immediately.

### Dependency injection

CliFx uses an implementation of `ITypeActivator` to initialize commands and by default it only works with types that have parameter-less constructors. This is sufficient for majority of scenarios.

However, in some cases you may also want to initialize commands dynamically with the help of a dependency injection container. CliFx makes it really easy to integrate with any DI framework of your choice.

For example, here is how you would configure your application to use [`Microsoft.Extensions.DependencyInjection`](https://nuget.org/packages/Microsoft.Extensions.DependencyInjection) (aka the built-in dependency container in ASP.NET Core):

```c#
public static class Program
{
    public static async Task<int> Main()
    {
        var services = new ServiceCollection();

        // Register services
        services.AddSingleton<MyService>();

        // Register commands
        services.AddTransient<MyCommand>();

        var serviceProvider = services.BuildServiceProvider();

        return await new CliApplicationBuilder()
            .AddCommandsFromThisAssembly()
            .UseTypeActivator(serviceProvider.GetService)
            .Build()
            .RunAsync();
    }
}
```

### Testing

CliFx provides a convenient way to write functional tests for your applications, thanks to the `IConsole` interface. While a command running in production uses `SystemConsole` for console interactions, you can rely on `VirtualConsole` in your tests to validate these interactions in a simulated environment.

When you initialize an instance of `VirtualConsole`, you can supply your own streams which will be used as the application's stdin, stdout, and stderr. You don't have to supply all of them, however, and any remaining streams will be substituted with a no-op stub.

```c#
var console = new VirtualConsole(
    input: stdIn,
    output: stdOut,
    error: stdErr
);
```

Although `VirtualConsole` can be constructed with all kinds of streams, most of the time you will want to test against in-memory stores. To simplify setup in such scenarios, CliFx also provides a `CreateBuffered` factory method that returns an instance of `IConsole` along with in-memory streams that you can later read from:

```c#
var (console, stdOut, stdErr) = VirtualConsole.CreateBuffered();

// ...

// Get the text that was written so far
var stdOutData = stdOut.GetString();
```

To illustrate how to use this, let's look at an example. Assume you want to test a simple command such as this one:

```c#
[Command]
public class ConcatCommand : ICommand
{
    [CommandOption("left")]
    public string Left { get; set; } = "Hello";

    [CommandOption("right")]
    public string Right { get; set; } = "world";

    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.Write(Left);
        console.Output.Write(' ');
        console.Output.Write(Right);

        return default;
    }
}
```

By substituting `IConsole` you can write your test cases like so:

```c#
// Integration test at the command level
[Test]
public async Task ConcatCommand_executes_successfully()
{
    // Arrange
    var (console, stdOut, _) = VirtualConsole.CreateBuffered();

    var command = new ConcatCommand
    {
        Left = "foo",
        Right = "bar"
    };

    // Act
    await command.ExecuteAsync(console);

    // Assert
    Assert.That(stdOut.GetString(), Is.EqualTo("foo bar"));
}
```

Similarly, you can also test the entire execution end-to-end like so:

```c#
// End-to-end test at the application level
[Test]
public async Task ConcatCommand_executes_successfully()
{
    // Arrange
    var (console, stdOut, _) = VirtualConsole.CreateBuffered();

    var app = new CliApplicationBuilder()
        .AddCommand<ConcatCommand>()
        .UseConsole(console)
        .Build();

    var args = new[] {"--left", "foo", "--right", "bar"};
    var envVars = new Dictionary<string, string>();

    // Act
    await app.RunAsync(args, envVars);

    // Assert
    Assert.That(stdOut.GetString(), Is.EqualTo("foo bar"));
}
```

As a general recommendation, it's always more preferable to test at the application level. While you can validate your command's execution adequately simply by testing its `ExecuteAsync()` method, testing end-to-end also helps you catch bugs related to configuration, such as incorrect option names, parameter order, environment variable names, etc.

Additionally, it's important to remember that commands in CliFx are not constrained to text and can produce binary data. In such cases, you can still use the above setup but call `GetBytes()` instead of `GetString()`:

```c#
// Act
await app.RunAsync(args, envVars);

// Assert
Assert.That(stdOut.GetBytes(), Is.EqualTo(new byte[] {1, 2, 3, 4, 5}));
```

In some scenarios the binary data may be too large to load in-memory. If that's the case, it's recommended to use `VirtualConsole` directly with custom streams.

### Debug and preview mode

When troubleshooting issues, you may find it useful to run your app in debug or preview mode. To do it, simply pass the corresponding directive to your app along with any other command line arguments.

If your application is ran in debug mode (using the `[debug]` directive), it will wait for debugger to be attached before proceeding. This is useful for debugging apps that were ran outside of the IDE.

```sh
> myapp.exe [debug] cmd -o

Attach debugger to PID 3148 to continue.
```

If preview mode is specified (using the `[preview]` directive), the app will short-circuit by printing consumed command line arguments as they were parsed. This is useful when troubleshooting issues related to command routing and argument binding.

```sh
> myapp.exe [preview] cmd arg1 arg2 -o foo --option bar1 bar2

Parser preview:
cmd <arg1> <arg2> [-o foo] [--option bar1 bar2]
```

You can also disallow these directives, e.g. when running in production, by calling `AllowDebugMode` and `AllowPreviewMode` methods on `CliApplicationBuilder`.

```c#
var app = new CliApplicationBuilder()
    .AddCommandsFromThisAssembly()
    .AllowDebugMode(true) // allow debug mode
    .AllowPreviewMode(false) // disallow preview mode
    .Build();
```

### Environment variables

An option can be configured to use the value of an environment variable as a fallback. If the value for such an option is not directly specified in the arguments, it will be extracted from that environment variable instead.

Here's an example of a required option that can be either provided directly or extracted from the environment:

```c#
[Command]
public class AuthCommand : ICommand
{
    [CommandOption("token", IsRequired = true, EnvironmentVariableName = "AUTH_TOKEN")]
    public string AuthToken { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(AuthToken);

        return default;
    }
}
```

```sh
> $env:AUTH_TOKEN="test"

> myapp.exe

test
```

Environment variables can be used as fallback for options of enumerable types too. In this case, the value of the variable will be split by `Path.PathSeparator` (which is `;` on Windows, `:` on Linux).

## Etymology

CliFx is made out of "Cli" for "Command Line Interface" and "Fx" for "Framework". It's pronounced as "cliff ex".
