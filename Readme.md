# CliFx

[![Build](https://github.com/Tyrrrz/CliFx/workflows/CI/badge.svg?branch=master)](https://github.com/Tyrrrz/CliFx/actions)
[![Coverage](https://codecov.io/gh/Tyrrrz/CliFx/branch/master/graph/badge.svg)](https://codecov.io/gh/Tyrrrz/CliFx)
[![Version](https://img.shields.io/nuget/v/CliFx.svg)](https://nuget.org/packages/CliFx)
[![Downloads](https://img.shields.io/nuget/dt/CliFx.svg)](https://nuget.org/packages/CliFx)
[![Donate](https://img.shields.io/badge/donate-$$$-purple.svg)](https://tyrrrz.me/donate)

CliFx is a simple to use, yet powerful framework for building command line applications. Its primary goal is to completely take over the user input layer, letting you forget about the infrastructure and instead focus on writing your application. This framework uses a declarative class-first approach for defining commands, avoiding excessive boilerplate code and complex configurations.

An important property of CliFx, when compared to some other libraries, is that it's not just a parser -- it's a complete application framework. The main goal of the library is to provide a consistent and enjoyable development experience when building command line applications. At its core, CliFx is highly opinionated, giving preference to convention over configuration, strictness over extensibility, consistency over ambiguity, and so on.

## Download

- [NuGet](https://nuget.org/packages/CliFx): `dotnet add package CliFx`

## Features

- Complete application framework, not just an argument parser
- Requires minimal amount of code to get started
- Configuration via attributes
- Handles conversions to various types, including custom types
- Supports multi-level command hierarchies
- Supports interactive mode
- Exposes raw input, output, error streams to handle binary data
- Allows graceful command cancellation
- Prints errors and routes exit codes on exceptions
- Provides comprehensive and colorful auto-generated help text
- Highly testable and easy to debug
- Comes with built-in analyzers to help catch common mistakes
- Targets .NET Standard 2.0+
- Uses `Microsoft.Extensions.DependencyInjection` but no other external dependencies

## Screenshots

![help screen](.screenshots/help.png)

## Usage

- [CliFx](#clifx)
  - [Download](#download)
  - [Features](#features)
  - [Screenshots](#screenshots)
  - [Usage](#usage)
    - [Quick start](#quick-start)
    - [Binding arguments](#binding-arguments)
    - [Argument syntax](#argument-syntax)
    - [Value conversion](#value-conversion)
    - [Multiple commands](#multiple-commands)
    - [Built-in and custom directives](#built-in-and-custom-directives)
      - [[debug] aka debug mode](#debug-aka-debug-mode)
      - [[preview] aka preview mode](#preview-aka-preview-mode)
      - [[>] aka scope to command(s)](#-aka-scope-to-commands)
      - [[.] aka scope-up](#-aka-scope-up)
      - [[..] aka scope-reset](#-aka-scope-reset)
      - [[default] aka execute default or scoped command](#default-aka-execute-default-or-scoped-command)
      - [Defining custom directives](#defining-custom-directives)
    - [Interactive mode](#interactive-mode)
    - [Reporting errors](#reporting-errors)
    - [Exception handling](#exception-handling)
    - [Graceful cancellation](#graceful-cancellation)
    - [Dependency injection](#dependency-injection)
    - [Testing](#testing)
    - [Environment variables](#environment-variables)
  - [Utilities](#utilities)
    - [ProgressTicker](#progressticker)
    - [TableUtils](#tableutils)
    - [TextUtils](#textutils)
  - [Benchmarks](#benchmarks)
  - [Etymology](#etymology)

### Quick start

![quick start animated](https://i.imgur.com/uouNh2u.gif)

To turn your application into a command line interface you need to change your program's `Main` method so that it delegates execution to `CliApplication`.

The following code will create and run a default `CliApplication` that will resolve commands defined in the calling assembly. Using fluent interface provided by `CliApplicationBuilder` you can easily configure different aspects of your application.

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

It is also possible to use a startup class to configure application.

```c#
public static class Program
{
    public static async Task<int> Main() =>
        await new CliApplicationBuilder()
            .UseStartup<CliStartup>()
            .Build()
            .RunAsync();
}

public class CliStartup : ICliStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register services
        services.AddSingleton<ICustomService, CustomService>();
    }

    public void Configure(CliApplicationBuilder app)
    {
        app.AddCommandsFromThisAssembly()
           .AddDirective<DebugDirective>()
           .AddDirective<PreviewDirective>()
           .UseInteractiveMode();
    }
}
```

In order to add functionality to your application, you need to define at least one command. Commands are essentially entry points through which the user can interact with your application. You can think of them as something similar to controllers in ASP.NET Core.

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

To implement `ICommand`, the class needs to define the `ExecuteAsync` method. This is the method that gets called when the user executes the command.

To facilitate both asynchronous and synchronous execution, this method returns a `ValueTask`. Since the simple command above executes synchronously, we can just put `return default` at the end. In an asynchronous command, however, we would use the `async`/`await` keywords instead.

As a parameter, this method takes an instance of `IConsole`, an abstraction around the system console. You should use this abstraction in places where you would normally use `System.Console`, in order to make your command testable.

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

In CliFx, there are two types of argument bindings: **parameters** and **options**. Parameters are positional arguments that are identified by the order they appear in, while options are identified by their names.

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

They can also set the `base` option to override the default logarithm base of 10:

```sh
> myapp.exe 729 -b 3

6
```

```sh
> myapp.exe 123 --base 4.5

3.199426017362198
```

On the other hand, if the user fails to provide the required parameter, they will get an error:

```sh
> myapp.exe -b 10

Missing value for parameter <value>.
```

Differences between parameters and options:

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

Options are always parsed the same way, disregarding the arity of the actual property it binds to. This means that `myapp -i file1.txt file2.txt` will _always_ be parsed as an option with multiple values, even if the underlying bound property is not enumerable. For the same reason, unseparated arguments such as `myapp -ofile` will be treated as five distinct options `'o'`, `'f'`, `'i'`, `'l'`, `'e'`, instead of `'o'` being set to `"file"`.

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
- Nullable versions of all above types (`decimal?`, `TimeSpan?`, etc.)
- Collections of all above types
  - Array types (`T[]`)
  - Types that are assignable from arrays (`IReadOnlyList<T>`, `ICollection<T>`, etc.)
  - Types with a constructor that accepts a single `T[]` parameter (`HashSet<T>`, `List<T>`, etc.)

When defining a parameter of an enumerable type, keep in mind that it has to be the only such parameter and it must be the last in order. Options, on the other hand, don't have this limitation.

Example command with an array-backed parameter:

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

The user can access other commands by specifying the name before any other arguments, e.g. `myapp.exe cmd1 arg1 -p 42`.

In a multi-command application you may also choose to not have a default command, in which case executing your application without any arguments will just show the help text.

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

In every command it is possible to define a description and a manual with `[Command]` attribute. `[Command]` attribute provides also an easy way for excluding a command from execution in normal mode through `InteractiveModeOnly` property.

### Built-in and custom directives

By default CliFx provides 6 directives, i.e., `[debug]`, `[preview]`, `[>]`, `[.]`, `[..]`, and `[default]`. The last 4 directives are only for interactive mode. Directives are used to change the behaviour of the whole application, e.g., imagine you have an application that logs everything to a fiel, then you can add `[no-logging]` directive to disable logging for a command or whole interactive session.

#### [debug] aka debug mode

When troubleshooting issues, you may find it useful to run your app in debug mode. If your application is ran in debug mode (using the `[debug]` directive), it will wait for debugger to be attached before proceeding. This is useful for debugging apps that were ran outside of the IDE.

```sh
> myapp.exe [debug] cmd -o

Attach debugger to PID 3148 to continue.
```

By default this directive is disallowed. To add a support of this directive, use `AddDirective` method in the `CliApplicationBuilder`:

``` c#
builder.AddDirective<DebugDirective>();
```

#### [preview] aka preview mode

When troubleshooting issues, you may find it useful to run your app in preview mode. If preview mode is specified (using the `[preview]` directive), the app will short-circuit by printing consumed command line arguments as they were parsed. This is useful when troubleshooting issues related to command routing and argument binding.

```sh
> myapp.exe [preview] cmd arg1 arg2 -o foo --option bar1 bar2

Parser preview:
cmd <arg1> <arg2> [-o foo] [--option bar1 bar2]
```

By default this directive is disallowed. To add a support of this directive, use `AddDirective` method in the `CliApplicationBuilder`:

``` c#
builder.AddDirective<PreviewDirective>();
```

#### [>] aka scope to command(s)

If application rans in interactive mode, `[>]` directive followed by command(s) name(s) would scope to the command(s), allowing to ommit specified command name(s).

For example, commands:
```sh
            > [>] cmd1 sub
    cmd1 sub> list
    cmd1 sub> get
            > [>] cmd1
        cmd1> test
        cmd1> -h
```

are an equivalent to:

``` sh
            > cmd1 sub list
            > cmd1 sub get
            > cmd1 test
            > cmd1 -h
```

#### [.] aka scope-up

If application rans in interactive mode, `[.]` directive can be used to remove one command from the scope.

Example:
```sh
            > [>] cmd1 sub
    cmd1 sub> list
    cmd1 sub> [.]
        cmd1>
```

#### [..] aka scope-reset

If application rans in interactive mode, `[..]` directive can be used to reset current scope to default (global scope).

Example:
```sh
            > [>] cmd1 sub
    cmd1 sub> list
    cmd1 sub> [..]
            >
```

#### [default] aka execute default or scoped command

Normally if application rans in interactive mode, an empty line does nothing; but `[default]` will override this behaviour, executing a root (empty) command or scoped command without arguments (parameters and options).

#### Defining custom directives

To define a custom directive, just create a new class that implements the `IDirective` interface and annotate it with `[Directive]` attribute:

```c#
[Directive("example", Description = "Example directive", InteractiveModeOnly = true)]
public class ScopeDirective : IDirective
{
    private readonly CliContext _cliContext;

    public bool ContinueExecution => false;

    public ScopeDirective(ICliContext cliContext)
    {
        _cliContext = (CliContext)cliContext;
    }

    public ValueTask HandleAsync(IConsole console)
    {
        console.Output.WriteLine("Test");
        console.Output.WriteLine(_cliContext.IsInteractiveMode);

        return default;
    }
}
```

To implement `IDirective`, the class needs to define `ContinueExecution` property and the `HandleAsync` method what gets called when the user specifies the directive in a command.

To facilitate both asynchronous and synchronous execution, this method returns a `ValueTask`. Since the simple command above executes synchronously, we can just put `return default` at the end. In an asynchronous command, however, we would use the `async`/`await` keywords instead.

As a parameter, this method takes an instance of `IConsole`, an abstraction around the system console. You should use this abstraction in places where you would normally use `System.Console`, in order to make your command testable.

`ContinueExecution` property is used to determine whether to stop execution of the command after exiting the directive handler. It can be a get only or a get/set property.

Similarly to commands, in every directive it is possible to define a description and a manual with `[Directive]` attribute. `[Directive]` attribute provides also an easy way for excluding a command from execution in normal mode through `InteractiveModeOnly` property.

### Interactive mode

An interactive mode is a special mode of application, which allows passing multiple commands to the application without exiting the application. To start application in interactive mode simply specify `[interactive]` directive when running the application. By default applications do no support interactive mode. This can be changes with:

``` c#
    builder.UseInteractiveMode();
```

Method `UseInteractiveMode` automatically adds the following directives: `[default]`, `[>]`, `[.]`, and `[..]`. If you wish not to add `[>]`, `[.]`, and `[..]` directives, you can use:

``` c#
    builder.UseInteractiveMode(addScopeDirectives: false);
```

Since every empty command in interactive mode will do nothing more than printing a command promt in a new line, a special directive `[default]` can be used to execute default or scoped command without parameters.

In help view, every command and directive that can be executed only in interactive mode are indicated with `@`.

When starting the application in interactive mode it is possible to pass a command that will be executed just after startign the applicaton, e.g., `interactive] -h` will start the application in interactive mode, print help message, and then show the prompt.

### Reporting errors

You may have noticed that commands in CliFx don't return exit codes. This is by design as exit codes are considered a higher-level concern and thus handled by `CliApplication`, not by individual commands.

Commands can report execution failure simply by throwing exceptions just like any other C# code. When an exception is thrown, `CliApplication` will catch it, print the error, and return `1` as the exit code to the calling process.

If you want to communicate a specific error through exit code, you can instead throw an instance of `CommandException` which takes an exit code as a parameter. When a command throws an exception of type `CommandException`, it is assumed that this was a result of a handled error and, as such, only the exception message will be printed to the error stream. If a command throws an exception of any other type, the full stack trace will be printed as well.

> Note: Unix systems rely on 8-bit unsigned integers for exit codes, so it's strongly recommended to use values between `1` and `255` to avoid potential overflow issues.

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

### Exception handling

By default all exceptions are handled using `DefaultExceptionHandler` class instance. Howeever, it is possible to customize the behavior with a custom implementation of `ICliExceptionHandler`.

``` c#
    public class CustomExceptionHandler : ICliExceptionHandler
    {
        public void HandleCliFxException(ICliContext context, CliFxException ex)
        {
            WriteError(context.Console, ex.ToString());
            context.Console.Error.WriteLine();

            ex.ShowHelp = false;
        }

        public void HandleDirectiveException(ICliContext context, DirectiveException ex)
        {
            WriteError(context.Console, ex.ToString());
            context.Console.Error.WriteLine();
        }

        public void HandleCommandException(ICliContext context, CommandException ex)
        {
            WriteError(context.Console, ex.ToString());
            context.Console.Error.WriteLine();
        }

        public void HandleException(ICliContext context, Exception ex)
        {
            WriteError(context.Console, ex.ToString());
            context.Console.Error.WriteLine();
        }

        protected static void WriteError(IConsole console, string message)
        {
            console.WithForegroundColor(ConsoleColor.DarkRed, () => console.Error.WriteLine(message));
        }
    }
```

To register the custom exception handler use:

``` c#
    builder.UseExceptionHandler(new CustomExceptionHandler()))
    //or
    builder.UseExceptionHandler<CustomExceptionHandler>();
```

### Graceful cancellation

It is possible to gracefully cancel execution of a command and preform any necessary cleanup. By default an app gets forcefully killed when it receives an interrupt signal (Ctrl+C or Ctrl+Break), but you can override this behavior.

In order to make a command cancellation-aware, you need to call `console.GetCancellationToken()`. This method returns a `CancellationToken` that will trigger when the user issues an interrupt signal. Note that any code that comes before the first call to `GetCancellationToken()` will not be cancellation-aware and as such will terminate instantly. Any subsequent calls to this method will return the same token.

```c#
[Command("cancel")]
public class CancellableCommand : ICommand
{
    public async ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine("Printed");

        // Long-running cancellable operation that throws when canceled
        await Task.Delay(Timeout.InfiniteTimeSpan, console.GetCancellationToken());

        console.Output.WriteLine("Never printed");
    }
}
```

Keep in mind that a command may delay cancellation only once. If the user decides to press Ctrl+C again after the first time, the execution will be forcefully terminated.

### Dependency injection

CliFx uses [`Microsoft.Extensions.DependencyInjection`](https://nuget.org/packages/Microsoft.Extensions.DependencyInjection) (aka the built-in dependency container in ASP.NET Core) to initialize commands and directives, and to support services injection.

By default the following services are registered:

| Type                     | Lifetime  | Description                                                                     |
|--------------------------|-----------|---------------------------------------------------------------------------------|
| ApplicationMetadata      | Singleton | Metadata associated with the application.                                       |
| ApplicationConfiguration | Singleton | Configuration of the application.                                               |
| ICliContext              | Singleton | Command line application context.                                               |
| IConsole                 | Singleton | Provides interaction with the console.                                          |
| ICommandMiddleware       | Singleton | Represents an async continuation for the next task to execute in the pipeline.  |

Additionally, every directive, middleware, and command is registered using its interface (`IDirective` or `ICommand`) implementation class with a lifetime set ot `Transient`. Thus, it is possible to get an enumeration/list of all directives or commands.

Services can be registerd using `ConfigureServices` or `UseStartup`.

CliFx supports also `Scoped` services. A scope begins after parsing the console input, and ends with the command execution.

```c#
public static class Program
{
    public static async Task<int> Main()
    {
        return await new CliApplicationBuilder()
            .AddCommandsFromThisAssembly()
            .AddDirectivesFromThisAssembly()
            .ConfigureServices((services) =>
            {
                // Register services
                services.AddSingleton<MyService>();
                services.AddTransient<MyCommand>();
            })
            .UseConsole<SystemConsole>()
            .Build()
            .RunAsync();
    }
}
```

### Testing

CliFx provides an easy way to write functional tests for your commands thanks to the `IConsole` interface.

You can use `VirtualConsole` to replace the application's stdin, stdout and stderr with your own streams. It has multiple constructor overloads allowing you to specify the exact set of streams that you want. Streams which are not provided by you are replaced with stubs, i.e. `VirtualConsole` doesn't leak to `System.Console` in any way.

Let's assume you want to test a simple command such as this one.

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

By substituting `IConsole` you can write your test cases like this:

```c#
[Test]
public async Task ConcatCommand_Test()
{
    // Arrange
    await using var stdOut = new MemoryStream();
    var console = new VirtualConsole(output: stdOut);

    var command = new ConcatCommand
    {
        Left = "foo",
        Right = "bar"
    };

    // Act
    await command.ExecuteAsync(console);
    var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray());

    // Assert
    Assert.That(stdOutData, Is.EqualTo("foo bar"));
}
```

And if you want, you can also test the whole application in a similar fashion:

```c#
[Test]
public async Task ConcatCommand_Test()
{
    // Arrange
    await using var stdOut = new MemoryStream();
    var console = new VirtualConsole(output: stdOut);

    var app = new CliApplicationBuilder()
        .AddCommand(typeof(ConcatCommand))
        .UseConsole(console)
        .Build();

    var args = new[] {"--left", "foo", "--right", "bar"};
    var envVars = new Dictionary<string, string>();

    // Act
    await app.RunAsync(args, envVars);
    var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray());

    // Assert
    Assert.That(stdOutData, Is.EqualTo("foo bar"));
}
```

Note that CliFx applications have access to underlying binary streams that allows them to write binary data directly. By using the approach outlined above, we're making the assumption that the application is only expected to produce text output.

### Environment variables

An option can be configured to use the value of an environment variable as a fallback. If an option was not specified by the user, the value will be extracted from that environment variable instead. This also works on options which are marked as required.

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

## Utilities

### ProgressTicker

CliFx comes with a simple utility for reporting progress to the console, `ProgressTicker`, which renders progress in-place on every tick.

It implements a well-known `IProgress<double>` interface so you can pass it to methods that are aware of this abstraction.

To avoid polluting output when it's not bound to a console, `ProgressTicker` will simply no-op if stdout is redirected.

```c#
var progressTicker = console.CreateProgressTicker();

for (var i = 0.0; i <= 1; i += 0.01)
    progressTicker.Report(i);
```

### TableUtils

CliFx comes with a simple utility for writing tables to the console. It can display both ungrouped and single-level grouped collections.
 Its configuration uses expressions to specfiy columns and column values transformations to string.

``` c#
TableUtils.Write(console,
                 _cliContext.Services.OrderBy(x => x.Lifetime)
                                     .ThenBy(x => x.ServiceType.Name)
                                     .ThenBy(x => x.ImplementationType?.Name)
                                     .GroupBy(x => x.Lifetime),
                 new string[] { "Service type", "Implementation type", "F", "I", "Lifetime" },
                 footnotes:
                 "  F - whether implementation factory is used\n" +
                 "  I - whether implementation instanace is used",
                 x => x.ServiceType.Name,
                 x => x.ImplementationType == null ? string.Empty : x.ImplementationType.Name,
                 x => x.ImplementationFactory == null ? string.Empty : "+",
                 x => x.ImplementationInstance == null ? string.Empty : "+",
                 x => x.Lifetime.ToString());
```

``` sh
=============================================================================
 Service type               | Implementation type        | F | I | Lifetime
=============================================================================
-----------------------------------------------------------------------------
                               Singleton (9)
-----------------------------------------------------------------------------
 ApplicationConfiguration   |                            | + |   | Singleton
 ApplicationMetadata        |                            | + |   | Singleton
 ExecutionTimingMiddleware  | ExecutionTimingMiddleware  |   |   | Singleton
 ExitCodeMiddleware         | ExitCodeMiddleware         |   |   | Singleton
 ICliContext                |                            | + |   | Singleton
 ICommandMiddleware         | ExecutionTimingMiddleware  |   |   | Singleton
 ICommandMiddleware         | ExitCodeMiddleware         |   |   | Singleton
 IConsole                   |                            | + |   | Singleton
 LibraryService             | LibraryService             |   |   | Singleton
-----------------------------------------------------------------------------
                               Transient (14)
-----------------------------------------------------------------------------
 BookAddCommand             | BookAddCommand             |   |   | Transient
 BookCommand                | BookCommand                |   |   | Transient
 BookListCommand            | BookListCommand            |   |   | Transient
 BookRemoveCommand          | BookRemoveCommand          |   |   | Transient
 DefaultDirective           | DefaultDirective           |   |   | Transient
 ICommand                   | BookAddCommand             |   |   | Transient
 ICommand                   | BookCommand                |   |   | Transient
 ICommand                   | BookListCommand            |   |   | Transient
 ICommand                   | BookRemoveCommand          |   |   | Transient
 ICommand                   | ServicesCommand            |   |   | Transient
 IDirective                 | DefaultDirective           |   |   | Transient
 IDirective                 | PreviewDirective           |   |   | Transient
 PreviewDirective           | PreviewDirective           |   |   | Transient
 ServicesCommand            | ServicesCommand            |   |   | Transient
=============================================================================
  F - whether implementation factory is used
  I - whether implementation instanace is used
=============================================================================
```

### TextUtils

`TextUtils` contains simple text transformations:

    - `AdjustNewLines` transforms line ending to appropriate format for the current operating system.
    - `ConvertTabsToSpaces` replaces tabs with spaces. By default two spaces are used in place of one tabulator.


## Benchmarks

Here's how CliFx's execution overhead compares to that of other libraries.

```ini
BenchmarkDotNet=v0.12.0, OS=Windows 10.0.14393.3443 (1607/AnniversaryUpdate/Redstone1)
Intel Core i5-4460 CPU 3.20GHz (Haswell), 1 CPU, 4 logical and 4 physical cores
Frequency=3124994 Hz, Resolution=320.0006 ns, Timer=TSC
.NET Core SDK=3.1.100
  [Host]     : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT
  DefaultJob : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT
```

|                               Method |        Mean |     Error |     StdDev | Ratio | RatioSD | Rank |
|------------------------------------- |------------:|----------:|-----------:|------:|--------:|-----:|
|                    CommandLineParser |    24.79 us |  0.166 us |   0.155 us |  0.49 |    0.00 |    1 |
|                                CliFx |    50.27 us |  0.248 us |   0.232 us |  1.00 |    0.00 |    2 |
|                                Clipr |   160.22 us |  0.817 us |   0.764 us |  3.19 |    0.02 |    3 |
| McMaster.Extensions.CommandLineUtils |   166.45 us |  1.111 us |   1.039 us |  3.31 |    0.03 |    4 |
|                   System.CommandLine |   170.27 us |  0.599 us |   0.560 us |  3.39 |    0.02 |    5 |
|                            PowerArgs |   306.12 us |  1.495 us |   1.398 us |  6.09 |    0.03 |    6 |
|                               Cocona | 1,856.07 us | 48.727 us | 141.367 us | 37.88 |    2.60 |    7 |

## Etymology

CliFx is made out of "Cli" for "Command Line Interface" and "Fx" for "Framework". It's pronounced as "cliff ex".
