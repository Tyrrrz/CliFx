# CliFx

[![Status](https://img.shields.io/badge/status-active-47c219.svg)](https://github.com/Tyrrrz/.github/blob/prime/docs/project-status.md)
[![Made in Ukraine](https://img.shields.io/badge/made_in-ukraine-ffd700.svg?labelColor=0057b7)](https://tyrrrz.me/ukraine)
[![Build](https://img.shields.io/github/actions/workflow/status/Tyrrrz/CliFx/main.yml?branch=prime)](https://github.com/Tyrrrz/CliFx/actions)
[![Coverage](https://img.shields.io/codecov/c/github/Tyrrrz/CliFx/prime)](https://codecov.io/gh/Tyrrrz/CliFx)
[![Version](https://img.shields.io/nuget/v/CliFx.svg)](https://nuget.org/packages/CliFx)
[![Downloads](https://img.shields.io/nuget/dt/CliFx.svg)](https://nuget.org/packages/CliFx)
[![Discord](https://img.shields.io/discord/869237470565392384?label=discord)](https://discord.gg/2SUWKFnHSm)
[![Fuck Russia](https://img.shields.io/badge/fuck-russia-e4181c.svg?labelColor=000000)](https://twitter.com/tyrrrz/status/1495972128977571848)

<table>
    <tr>
        <td width="99999" align="center">Development of this project is entirely funded by the community. <b><a href="https://tyrrrz.me/donate">Consider donating to support!</a></b></td>
    </tr>
</table>

<p align="center">
    <img src="favicon.png" alt="Icon" />
</p>

**CliFx** is an opinionated framework for building command-line applications.
It provides a model to express command interactions directly through classes and properties, skipping all the infrastructural concerns like argument parsing, routing, error handling, and help generation.

## Terms of use<sup>[[?]](https://github.com/Tyrrrz/.github/blob/prime/docs/why-so-political.md)</sup>

By using this project or its source code, for any purpose and in any shape or form, you grant your **implicit agreement** to all the following statements:

- You **condemn Russia and its military aggression against Ukraine**
- You **recognize that Russia is an occupant that unlawfully invaded a sovereign state**
- You **support Ukraine's territorial integrity, including its claims over temporarily occupied territories of Crimea and Donbas**
- You **reject false narratives perpetuated by Russian state propaganda**

To learn more about the war and how you can help, [click here](https://tyrrrz.me/ukraine). Glory to Ukraine! 🇺🇦

## Install

- 📦 [NuGet](https://nuget.org/packages/CliFx): `dotnet add package CliFx`

## Features

- Complete application framework, not just an argument parser
- Minimum boilerplate and easy to get started
- Class-first configuration via attributes
- Comprehensive auto-generated help text
- Support for deeply nested command hierarchies
- Graceful cancellation via interrupt signals
- Support for reading and writing binary data
- Testable console interaction layer
- Compatible with Native AOT and trimming
- Targets .NET Standard 2.0+
- No external dependencies

## Screenshots

![help screen](.assets/help-screen.png)

## Usage

### Quick overview

To turn your program into a command-line interface, modify the `Main()` method so that it delegates its execution to an instance of `CommandLineApplication`.
You can use `CommandLineApplicationBuilder` to create and configure an application in a series of fluent instructions:

```csharp
using CliFx;

public static class Program
{
    public static async Task<int> Main() =>
        await new CommandLineApplicationBuilder()
            // Registers all accessible command types from the current assembly
            .AddCommandsFromThisAssembly()
            // Creates the application instance
            .Build()
            // Runs the application, resolving command-line arguments and
            // environment variables automatically.
            .RunAsync();
}
```

> [!IMPORTANT]
> Ensure that your `Main()` method returns the integer exit code provided by `CommandLineApplication.RunAsync()`, as shown in the example above.
> Exit code is used to communicate execution result to the parent process, so it's important that your program properly propagates it.

> [!NOTE]
> When calling `CommandLineApplication.RunAsync()`, **CliFx** resolves command-line arguments and environment variables from `Environment.GetCommandLineArgs()` and `Environment.GetEnvironmentVariables()` respectively.
> You can also provide them explicitly using one of the alternative overloads.

In **CliFx**, all application functionality is encapsulated within commands, which are logically independent entry points that the user can choose to execute.
The above example uses `AddCommandsFromThisAssembly()` to detect all command types defined in the current assembly and register them with the application.

To define a command, you need to declare a `partial` class, annotate it with the `[Command]` attribute, and implement the `ICommand` interface:

```csharp
using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;

// Default command (no name specified)
[Command(Description = "Calculates the logarithm of a value.")]
public partial class LogCommand : ICommand
{
    // Order: 0
    [CommandParameter(0, Description = "Value whose logarithm is to be found.")]
    public required double Value { get; set; }

    // Name: --base
    // Short name: -b
    [CommandOption("base", 'b', Description = "Logarithm base.")]
    public double Base { get; set; } = 10;

    public ValueTask ExecuteAsync(IConsole console)
    {
        var result = Math.Log(Value, Base);
        console.WriteLine(result);

        // This method supports asynchronous execution.
        // If you don't have any asynchronous work to do, just return
        // the default value, which is a completed ValueTask.
        return default;
    }
}
```

> [!IMPORTANT]
> The command type (along with all its containing types, if applicable) must be declared as `partial` so that **CliFx** can extend it with necessary metadata and behavior.

In order to implement `ICommand`, the class needs to define an `ExecuteAsync(...)` method, which is what gets called when the user chooses to execute the command.
As the only parameter, this method takes an instance of `IConsole` — an abstraction used in place of `System.Console` to write text, read binary data, or otherwise interact with the console in a decoupled fashion.

In most cases, your command will also need some inputs to work with.
This is achieved by defining properties on the command class and annotating them with the `[CommandParameter]` and `[CommandOption]` attributes, which specify how the values of these properties are mapped from the command-line arguments.

The command in the above example serves as a simple logarithm calculator which has two input bindings: a positional parameter for the logarithm value (bound to `Value`) and a named option for the logarithm base (bound to `Base`).
In order to execute this command, at minimum, the user needs to provide an argument for the value:

```console
$ ./myapp 10000

4
```

They can also pass the `-b|--base` option to override the default logarithm base of `10`:

```console
$ ./myapp 729 -b 3

6
```

In case the user forgets to specify the required `value` parameter, the application will instead exit with an error:

```console
$ ./myapp -b 10

Missing required parameter(s):
<value>
```

**CliFx** also automatically provides the conventional `-h|--help` option, which shows a help screen with all available options and parameters, their descriptions, and usage examples:

```console
$ ./myapp --help

MyApp v1.0

USAGE
  myapp <value> [options]

DESCRIPTION
  Calculates the logarithm of a value.

PARAMETERS
* value             Value whose logarithm is to be found.

OPTIONS
  -b|--base         Logarithm base. Default: "10".
  -h|--help         Shows help text. Default: "false".
  --version         Shows version information. Default: "false".
```

Because the command in the above example doesn't have a name, it's treated as the default (i.e. root) command.
More complex applications typically have multiple commands, each with its own name and functionality, allowing the user to choose between different workflows.

To define a named command, simply specify its name in the `[Command]` attribute:

```csharp
[Command("log", Description = "Calculates the logarithm of a value.")]
public partial class LogCommand : ICommand
{
    // Same code as before
}

[Command("sum", Description = "Calculates the sum of all input values.")]
public partial class SumCommand : ICommand
{
    [CommandParameter(0, Description = "Values to be summed.")]
    public required IReadOnlyList<double> Values { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        var result = Values.Sum();
        console.WriteLine(result);

        return default;
    }
}
```

With this setup, the user can run the application in one of the following ways:

To execute the `log` command (bound to `LogCommand`):

```console
$ ./myapp log 100 -b 10

4
```

To execute the `sum` command (bound to `SumCommand`):

```console
$ ./myapp sum 1 2 3

6
```

The root-level help text will also list each available command:

```console
$ ./myapp --help

MyApp v1.0

USAGE
  myapp [options]
  myapp [command] [...]

OPTIONS
  -h|--help         Shows help text. Default: "false".
  --version         Shows version information. Default: "false".

COMMANDS
  log               Calculates the logarithm of a value.
  sum               Calculates the sum of all input values.

You can run `myapp [command] --help` to show help for a specific command.
```

### Input bindings

**CliFx** supports two types of input bindings: **parameters** and **options**.
Parameters are bound from the command-line arguments based on the order they appear in, while options are bound by their name.

These two kinds of bindings are configured through the `[CommandParameter]` and `[CommandOption]` attributes respectively, which can be applied to any property on the command class.
Despite both fulfilling the same purpose of mapping command-line inputs to properties, parameters and options have different semantics and usage patterns.

#### Parameters

Below is an example of a parameter binding:

```csharp
[CommandParameter(0)]
public required double Param { get; set; }

// Usage:
// $ ./myapp <param>
```

> [!IMPORTANT]
> Order values must be unique among the parameters of the same command.

Here, from the attribute, we can see that this parameter is bound with the order of `0`.
This value specifies the parameter's position relative to the other parameters in the command, which determines the order in which they are activated from the command line.

For example, given the following bindings, parameters will receive their inputs in the order of: `First` -> `Second` -> `Third`:

```csharp
[CommandParameter(1)]
public required string Second { get; set; }

[CommandParameter(0)]
public required string First { get; set; }

[CommandParameter(2)]
public required string Third { get; set; }

// Usage:
// $ ./myapp <first> <second> <third>
```

> [!NOTE]
> Order values do not have to be sequential.
> They are not used as absolute indices, but rather as relative indicators of the parameters' positions, so they can be spaced out as needed.

Because parameters are bound positionally, every subsequent parameter relies on the previous one to be set in order to itself receive a value.
This means that they are primarily used for mandatory inputs that the user is always expected to provide to execute the command, hence why the underlying properties in the above examples are all marked as `required`.

If you need to bind a parameter without requiring it to be set, you can do so by dropping the `required` modifier from the property, however this can only be done for the last parameter in the command:

```csharp
[CommandParameter(0)]
public required string First { get; set; }

[CommandParameter(1)]
public required string Second { get; set; }

// This parameter is not required (value can be omitted).
// Only one parameter can be non-required, and it has to be the last one by order.
[CommandParameter(2)]
public string? Third { get; set; }

// Usage:
// $ ./myapp <first> <second> <third?>
```

If a parameter is optional, you can either set a default value on the property, or marks its type as nullable.

For the same reasons, parameters generally cannot be used for inputs that can take more than one value, since it's not always clear how many arguments should be consumed for a given parameter.
Again, the only exception to that rule is when it's the last parameter in the command:

```csharp
[CommandParameter(0)]
public required string First { get; set; }

[CommandParameter(1)]
public required string Second { get; set; }

// This parameter is sequence-based (can take multiple values).
// Only one parameter can be sequence-based, and it has to be the last one by order.
[CommandParameter(2)]
public required IReadOnlyList<string> Third { get; set; }

// Usage:
// $ ./myapp <first> <second> <third...>
```

As a natural consequence of these limitations, **parameters are best used to express fixed scalar inputs, whose presence is essential for the command to execute successfully**.

Besides that, when binding properties to parameters, you can also specify a custom name and description to show in the help text:

```csharp
[CommandParameter(0,
    // These properties are optional and only dictate how the parameter
    // is identified in the auto-generated help text.
    Name = "logval",
    Description = "Value whose logarithm is to be found."
)]
public required double Value { get; set; }

// Usage:
// $ ./myapp <logval>
```

As a general guideline, consider using parameter bindings if the input you're expressing:

- Is essential for the command to execute successfully and doesn't have an obvious default value.
- Is a scalar that doesn't take multiple values.
- Is the only input in the command.

#### Options

Below is an example of an option binding:

```csharp
[CommandOption("opt", 'o')]
public double Option { get; set; }

// Usage:
// $ ./myapp --opt <value?>
// $ ./myapp -o <value?>
```

> [!IMPORTANT]
> Option names and short names must be unique among the options of the same command.
> The comparison is case-insensitive for names and case-sensitive for short names.

> [!NOTE]
> An option binding doesn't have to specify both a name and a short name, but at least one of them needs to be set.

Here, from the attribute, we can see that this option is specified with the name of `opt` and a short name of `o`.
The user can pass the option by specifying either of these identifiers, and the framework will set the provided value to the corresponding property.

If an option is essential to the execution of the command, the underlying property can also be marked as `required`.
This will make it so that the user has to provide a value for this option on every execution of the command, otherwise the application will exit with an error:

Unlike the parameters, though, options can be marked as required and non-required arbitrarily, without any restrictions:

```csharp
// This option is required (value must be provided by the user)
[CommandOption("foo")]
public required string Foo { get; set; }

// This option is not required (value can be omitted)
[CommandOption("bar")]
public string? Bar { get; set; }

// This option is also required
[CommandOption("baz")]
public required string Baz { get; set; }

// Usage:
// $ ./myapp --foo <value> --bar <value?> --baz <value>
```

In the same vein, options can be freely used for inputs that can take multiple values:

```csharp
// This option is sequence-based (can take multiple values)
[CommandOption("foo")]
public required IReadOnlyList<string> Foo { get; set; }

// This option is also sequence-based
[CommandOption("bar")]
public required IReadOnlyList<string> Bar { get; set; }

// Usage:
// $ ./myapp --foo <value...> --bar <value...>
```

Finally, options can also be configured to rely on an environment variable for fallback, which is useful for required inputs that the user doesn't want to specify every time:

```csharp
// If the value is not provided from the command line, it will be read
// from the "ENV_FOO" environment variable instead.
[CommandOption("foo", EnvironmentVariable = "ENV_FOO")]
public required string Foo { get; set; }

// Usage:
// $ ./myapp --foo <value>
// $ ENV_FOO=value ./myapp
```

> [!NOTE]
> If the user provides a value for an option while the corresponding environment variable is also set, the command-line input will take precedence.

Taking all of the above into account, the general guideline is that **options are best used to express inputs that enable alternative execution paths, have obvious and sensible defaults, or otherwise can't be easily expressed as parameters**.

#### Conversion

In order to handle conversion between raw command-line arguments (strings) and the underlying property types, **CliFxx** employs the `IInputConverter<T>` interface, set by the `Converter` property on the binding attributes.
If the converter is not explicitly specified, the framework will try to generate one automatically.

Below are types supported by the default conversion logic:

- `string`
- `bool`
  - `true` if passed `true`, `1`, `yes` or `on` (case-insensitive) or no value at all
  - `false` if passed `false`, `0`, `no` or `off` (case-insensitive)
- `enum`
  - If passed a numeric string, the enum member is selected by its value
  - If passed a non-numeric string, the enum member is selected by its name (case-insensitive)
- Any type that defines a `static T Parse(string, IFormatProvider?)` method
  - Covers a wide range of built-in .NET types, such as `int`, `double`, `DateTime`, `Guid`, etc.
  - Also supports user-defined types that implement their own conversion logic through a `Parse(...)` method
  - The `IFormatProvider?` parameter is optional
- Any type that defines a constructor accepting a single `string` parameter
  - Covers types that don't have a `Parse(...)` method but can still be initialized from a string, such as `FileInfo`, `DirectoryInfo`, etc.
  - Also supports user-defined types that implement their own conversion logic through the constructor
- `Nullable<T>` of above value types
  - If passed a non-empty string, the underlying conversion logic is applied
  - If passed nothing or an empty string, the property is set to `null`
- Arrays of above types
  - This includes types assignable from arrays, such as `IEnumerable<T>`, `IReadOnlyList<T>`, `IReadOnlyCollection<T>`, etc.
- Any type that defines a constructor accepting an array of one of the above types
  - This includes collection types like `List<T>`, `HashSet<T>`, etc.

For example, here is an example command that showcases some of these built-in conversions:

```csharp
[Command("search", Description = "Searches for files matching a pattern.")]
public partial class SearchCommand : ICommand
{
    [CommandParameter(0, Description = "Pattern to search for.")]
    public required string Pattern { get; set; }

    [CommandOption("recursive", 'r', Description = "Whether to search subdirectories.")]
    public bool Recursive { get; set; }

    public enum OutputFormat { Plain, Json, Xml }

    [CommandOption("format", 'f', Description = "Output format.")]
    public OutputFormat Format { get; set; } = OutputFormat.Plain;

    [CommandOption("in", Description = "Directories to search in.")]
    public IReadOnlyList<DirectoryInfo> Paths { get; set; } = [];

    public ValueTask ExecuteAsync(IConsole console)
    {
        // ...
    }
}

// Usage:
// $ ./myapp search "*.log" --recursive --format json --in /var/log /tmp/logs
```

If you need to support a type that isn't covered by the default conversion logic, you can implement a custom converter yourself.
To do that, first create a class that inherits either from `ScalarInputConverter<T>` (for scalar types) or `SequenceInputConverter<T>` (for collection types):

```csharp
using CliFx.Activation;

// Maps 2D vectors from AxB notation
public class VectorConverter
    // ScalarInputConverter<T> for single-value inputs
    // SequenceInputConverter<T> for multiple-value inputs
    : ScalarInputConverter<Vector2>
{
    public override Vector2 Convert(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return default;

        var components = rawValue.Split('x', 'X', ';');
        var x = int.Parse(components[0], CultureInfo.InvariantCulture);
        var y = int.Parse(components[1], CultureInfo.InvariantCulture);

        return new Vector2(x, y);
    }
}
```

> [!IMPORTANT]
> Custom converter types must have a public parameter-less constructor, which is used by the framework to instantiate them at run time.

Then, specify this converter in the `Converter` property of the binding attribute:

```csharp
[Command("surface", Description = "Calculates the surface area of a triangle defined by three points.")]
public partial class SurfaceCalculatorCommand : ICommand
{
    [CommandParameter(0,
        Description = "First point of the triangle, in the format AxB (e.g. 10x20).",
        // Custom converter is used to map raw argument values
        Converter = typeof(VectorConverter)
    )]
    public required Vector2 PointA { get; init; }

    [CommandParameter(1,
        Description = "Second point of the triangle, in the format AxB (e.g. 10x20).",
        // Custom converter is used to map raw argument values
        Converter = typeof(VectorConverter)
    )]
    public required Vector2 PointB { get; init; }

    [CommandParameter(2,
        Description = "Third point of the triangle, in the format AxB (e.g. 10x20).",
        // Custom converter is used to map raw argument values
        Converter = typeof(VectorConverter)
    )]
    public required Vector2 PointC { get; init; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        // ...
    }
}

// Usage:
// $ ./myapp surface 0x0 0x10 10x0
```

#### Validation

Besides custom converters, you can also implement custom validators to verify that your inputs meet certain criteria before the command is executed.
Unlike converters, validators work with already converted values rather than raw command-line arguments.

To implement a custom validator, create a class that inherits from `InputValidator<T>`:

```csharp
using CliFx.Activation;

public class PositiveNumberValidator : InputValidator<double>
{
    public InputValidationError? Validate(double value)
    {
        if (value <= 0)
            return Error("Value must be positive.");

        return Ok();
    }
}
```

Then, include this validator in the `Validators` property of the binding attribute:

```csharp
[Command("sqrt", Description = "Calculates the square root of a value.")]
public partial class SqrtCommand : ICommand
{
    [CommandParameter(0,
        Description = "Value whose square root is to be found.",
        Validators = [typeof(PositiveNumberValidator)]
    )]
    public required double Value { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        // ...
    }
}

// Usage:
// $ ./myapp sqrt 25
```

The `Validators` property is an array, so you can specify more than one validator for the same input.
In that case, all their errors (if any) are aggregated and shown together to the user:

```csharp
[Command("passwd", Description = "Changes the user password.")]
public partial class ChangePasswordCommand : ICommand
{
    [CommandParameter(0, Description = "Username whose password is to be changed.")]
    public required string Username { get; set; }

    [CommandParameter(1,
        Description = "New password.",
        Validators =
        [
            typeof(PasswordLengthValidator),
            typeof(PasswordComplexityValidator)
        ]
    )]
    public required string Password { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        // ...
    }
}

// Usage:
// $ ./myapp passwd alice "verystr0ngpassw0rd!"
```

#### Argument syntax

**CliFx** employs a variation of the POSIX argument syntax, which is used in most modern command-line tools.
Here are some examples of how it works:

- `./myapp --foo bar` sets option `"foo"` to value `"bar"`
- `./myapp -f bar` sets option `'f'` to value `"bar"`
- `./myapp --switch` sets option `"switch"` without value
- `./myapp -s` sets option `'s'` without value
- `./myapp -abc` sets options `'a'`, `'b'` and `'c'` without value
- `./myapp -xqf bar` sets options `'x'` and `'q'` without value, and option `'f'` to value `"bar"`
- `./myapp -i file1.txt file2.txt` sets option `'i'` to a sequence of values `"file1.txt"` and `"file2.txt"`
- `./myapp -i file1.txt -i file2.txt` sets option `'i'` to a sequence of values `"file1.txt"` and `"file2.txt"`
- `./myapp cmd abc -o` routes to command `cmd` (assuming it's a command) with parameter `abc` and sets option `'o'` without value

Additionally, argument parsing in **CliFx** aims to be as deterministic as possible, ideally yielding the same result regardless of the application configuration.
In fact, the only context-sensitive part in the parser is the command name resolution, which needs to know the list of available commands in order to discern them from parameters.

The parser's context-free nature has several implications on how it consumes arguments.
For example, `./myapp -i file1.txt file2.txt` will always be parsed as an option with multiple values, regardless of the arity of the underlying property it's bound to.
Similarly, unseparated arguments in the form of `./myapp -ofile` will be treated as five distinct options `'o'`, `'f'`, `'i'`, `'l'`, `'e'`, instead of `'o'` being set to value `"file"`.

These rules also make the order of arguments important — command-line string is expected to follow this pattern:

```console
$ ./myapp [command] [...parameters] [...options]
```

### Command routing

In order to facilitate a variety of different workflows, command-line applications may provide the user with more than just a single command.
Complex applications may also nest commands underneath each other, employing a multi-level hierarchical structure.

With **CliFx**, this is achieved by simply giving each command a unique name through the `[Command]` attribute.
Commands that have common name segments are considered to be hierarchically related, which affects how they're listed in the help text.

```csharp
// Default command, i.e. command without a name
[Command]
public partial class DefaultCommand : ICommand
{
    // ...
}

// Child of default command
[Command("cmd1")]
public partial class FirstCommand : ICommand
{
    // ...
}

// Child of default command
[Command("cmd2")]
public partial class SecondCommand : ICommand
{
    // ...
}

// Child of FirstCommand
[Command("cmd1 sub")]
public partial class SubCommand : ICommand
{
    // ...
}
```

Once configured, the user can execute a specific command by prepending its name to the passed arguments.
For example, running `dotnet myapp.dll cmd1 arg1 -p 42` will execute `FirstCommand` in the above example.

The user can also find the list of all available top-level commands in the help text:

```console
$ dotnet myapp.dll --help

MyApp v1.0

USAGE
  dotnet myapp.dll [options]
  dotnet myapp.dll [command] [...]

OPTIONS
  -h|--help         Shows help text.
  --version         Shows version information.

COMMANDS
  cmd1              Subcommands: cmd1 sub.
  cmd2

You can run `dotnet myapp.dll [command] --help` to show help for a specific command.
```

To see the list of commands nested under a specific command, the user can refine their help request by specifying the corresponding command name before the help option:

```console
$ dotnet myapp.dll cmd1 --help

USAGE
  dotnet myapp.dll cmd1 [options]
  dotnet myapp.dll cmd1 [command] [...]

OPTIONS
  -h|--help         Shows help text.

COMMANDS
  sub

You can run `dotnet myapp.dll cmd1 [command] --help` to show help for a specific command.
```

> [!NOTE]
> Defining the default (unnamed) command is not required.
> If it's absent, running the application without specifying a command will just show the root-level help text.

### Reporting errors

Commands in **CliFx** do not directly return exit codes, but instead communicate execution errors via `CommandException`.
This special exception type can be used to print an error message to the console, return a specific exit code, and also optionally show help text for the current command:

```csharp
[Command]
public partial class DivideCommand : ICommand
{
    [CommandOption("dividend")]
    public required double Dividend { get; init; }

    [CommandOption("divisor")]
    public required double Divisor { get; init; }

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

```console
$ dotnet myapp.dll --dividend 10 --divisor 0

Division by zero is not supported.

$ echo $?

133
```

> [!WARNING]
> Even though exit codes are represented by 32-bit integers in .NET, using values outside the 8-bit unsigned range will cause overflows on Unix systems.
> To avoid unexpected results, use numbers between 1 and 255 for exit codes that indicate failure.

### Graceful cancellation

Console applications support the concept of interrupt signals, which can be issued by the user to abort the currently ongoing operation.
If your command performs critical work, you can intercept these signals to handle cancellation requests in a graceful way.

In order to make the command cancellation-aware, call `console.RegisterCancellationHandler()` to register the signal handler and obtain the corresponding `CancellationToken`.
Once this method is called, the program will no longer terminate on an interrupt signal but will instead trigger the associated token, which can be used to delay the termination of a command just enough to exit in a controlled manner.

```csharp
[Command]
public partial class CancellableCommand : ICommand
{
    private async ValueTask DoSomethingAsync(CancellationToken cancellation)
    {
        await Task.Delay(TimeSpan.FromMinutes(10), cancellation);
    }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        // Make the command cancellation-aware
        var cancellation = console.RegisterCancellationHandler();

        // Execute some long-running cancellable operation
        await DoSomethingAsync(cancellation);

        console.Output.WriteLine("Done.");
    }
}
```

> [!WARNING]
> Cancellation handler is only respected when the user sends the interrupt signal for the first time.
> If the user decides to issue the signal again, the application will be forcefully terminated without triggering the cancellation token.

### Type activation

Because **CliFx** takes responsibility for the application's entire lifecycle, it needs to be capable of instantiating various user-defined types at run-time.
To facilitate that, it uses an interface called `ITypeActivator` that determines how to create a new instance of a given type.

The default implementation of `ITypeActivator` only supports types that have public parameterless constructors, which is sufficient for the majority of scenarios.
However, in some cases you may also want to define a custom initializer, for example when integrating with an external dependency container.

To do that, pass a custom `ITypeActivator` or a factory delegate to the `UseTypeActivator(...)` method when building the application:

```csharp
public static class Program
{
    public static async Task<int> Main() =>
        await new CommandLineApplicationBuilder()
            .AddCommandsFromThisAssembly()
            .UseTypeActivator(type =>
            {
                var instance = MyTypeFactory.Create(type);
                return instance;
            })
            .Build()
            .RunAsync();
}
```

This method also supports `IServiceProvider` through various overloads, which allows you to directly integrate dependency containers that implement this interface.
For example, this is how to configure your application to use [`Microsoft.Extensions.DependencyInjection`](https://nuget.org/packages/Microsoft.Extensions.DependencyInjection) as the type activator in **CliFx**:

```csharp
public static class Program
{
    public static async Task<int> Main() =>
        await new CommandLineApplicationBuilder()
            .AddCommandsFromThisAssembly()
            .UseTypeActivator(commands =>
            {
                var services = new ServiceCollection();

                // Register services
                services.AddSingleton<MyService>();

                // Register commands
                foreach (var command in commands)
                    services.AddTransient(command.Type);

                return services.BuildServiceProvider();
            })
            .Build()
            .RunAsync();
}
```

> [!NOTE]
> If you want to use certain advanced features provided by `Microsoft.Extensions.DependencyInjection`, you may need to do a bit of extra work to configure the container properly.
> For example, to leverage support for keyed services, you need to [manually register an implementation of `IKeyedServiceProvider`](https://github.com/Tyrrrz/CliFx/issues/148).

### Testing

Thanks to the `IConsole` abstraction, **CliFx** commands can be easily tested in isolation.
While an application running in production would rely on `SystemConsole` to interact with the real console, you can use `FakeConsole` and `FakeInMemoryConsole` in your tests to execute your commands in a simulated environment.

For example, imagine you have the following command:

```csharp
[Command]
public partial class ConcatCommand : ICommand
{
    [CommandOption("left")]
    public string Left { get; init; } = "Hello";

    [CommandOption("right")]
    public string Right { get; init; } = "world";

    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.Write(Left);
        console.Output.Write(' ');
        console.Output.Write(Right);

        return default;
    }
}
```

To test it, you can instantiate the command in code with the required values, and then pass an instance of `FakeInMemoryConsole` to `ExecuteAsync(...)`:

```csharp
// Integration test at the command level
[Test]
public async Task ConcatCommand_executes_successfully()
{
    // Arrange
    using var console = new FakeInMemoryConsole();

    var command = new ConcatCommand
    {
        Left = "foo",
        Right = "bar"
    };

    // Act
    await command.ExecuteAsync(console);

    // Assert
    var stdOut = console.ReadOutputString();
    Assert.That(stdOut, Is.EqualTo("foo bar"));
}
```

Similarly, you can also test your command at a higher level like so:

```csharp
// End-to-end test at the application level
[Test]
public async Task ConcatCommand_executes_successfully()
{
    // Arrange
    using var console = new FakeInMemoryConsole();

    var app = new CommandLineApplicationBuilder()
        .AddCommand(ConcatCommand.Descriptor)
        .UseConsole(console)
        .Build();

    var args = new[]
    {
        "--left", "foo",
        "--right", "bar"
    };

    var envVars = new Dictionary<string, string>();

    // Act
    await app.RunAsync(args, envVars);

    // Assert
    var stdOut = console.ReadOutputString();
    Assert.That(stdOut, Is.EqualTo("foo bar"));
}
```

### Debug and preview mode

When troubleshooting issues, you may find it useful to run your application in debug or preview mode.
These modes are activated through environment variables, which you can configure using `AllowDebugMode(...)` and `AllowPreviewMode(...)` methods on `CommandLineApplicationBuilder`:

```csharp
var application = new CommandLineApplicationBuilder()
    .AddCommandsFromThisAssembly()
    // Enable debug mode via the CLIFX_DEBUG environment variable
    .AllowDebugMode("CLIFX_DEBUG")
    // Enable preview mode via the CLIFX_PREVIEW environment variable
    .AllowPreviewMode("CLIFX_PREVIEW")
    .Build();
```

When the debug mode environment variable is set to `true`, the application will launch in a suspended state, waiting for the debugger to attach to the current process:

```console
$ MYAPP_DEBUG=true dotnet myapp.dll cmd -o

Attach the debugger to process with ID 3148 to continue.
```

When the preview mode environment variable is set to `true`, the application will print the consumed command-line arguments as they were parsed:

```console
$ MYAPP_PREVIEW=true dotnet myapp.dll cmd arg1 arg2 -o foo --option bar1 bar2

cmd <arg1> <arg2> [-o foo] [--option bar1 bar2]
```

To disallow these modes (e.g. for production), simply pass `null` as the environment variable name:

```csharp
var application = new CommandLineApplicationBuilder()
    .AddCommandsFromThisAssembly()
    // Disallow debug mode
    .AllowDebugMode(null)
    // Disallow preview mode
    .AllowPreviewMode(null)
    .Build();
```

## Etymology

**CliFx** is made out of "Cli" for "Command-line Interface" and "Fx" for "Framework".
It's pronounced as "cliff ex".
