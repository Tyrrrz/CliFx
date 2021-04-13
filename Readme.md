# CliFx

[![Build](https://github.com/Tyrrrz/CliFx/workflows/CI/badge.svg?branch=master)](https://github.com/Tyrrrz/CliFx/actions)
[![Coverage](https://codecov.io/gh/Tyrrrz/CliFx/branch/master/graph/badge.svg)](https://codecov.io/gh/Tyrrrz/CliFx)
[![Version](https://img.shields.io/nuget/v/CliFx.svg)](https://nuget.org/packages/CliFx)
[![Downloads](https://img.shields.io/nuget/dt/CliFx.svg)](https://nuget.org/packages/CliFx)
[![Donate](https://img.shields.io/badge/donate-$$$-purple.svg)](https://tyrrrz.me/donate)

✅ **Project status: active**.

CliFx is a simple to use, yet powerful framework for building command line applications.
Its primary goal is to completely take over the user input layer, allowing you to forget about infrastructural concerns and instead focus on writing your application.

## Download

- [NuGet](https://nuget.org/packages/CliFx): `dotnet add package CliFx`

## Features

- Complete application framework, not just an argument parser
- Minimum boilerplate and easy to get started
- Class-first configuration via attributes
- Comprehensive autogenerated help text
- Support for deeply nested command hierarchies
- Graceful cancellation via interrupt signals
- Support for reading and writing binary data
- Testable console interaction layer
- Built-in analyzers to catch configuration issues
- Targets .NET Standard 2.0+
- No external dependencies

## Screenshots

![help screen](.screenshots/help.png)

## Usage

### Quick start

![quick start animated](https://i.imgur.com/uouNh2u.gif)

To turn your program into a command line interface, modify your `Main` method so that it delegates execution to `CliApplication`.
You can use `CliApplicationBuilder` to fluently create and configure an instance of `CliApplication`:

```csharp
public static class Program
{
    public static async Task<int> Main() =>
        await new CliApplicationBuilder()
            .AddCommandsFromThisAssembly()
            .Build()
            .RunAsync();
}
```

> Note: ensure that your `Main()` method returns the integer exit code provided by `CliApplication.RunAsync()`, as shown in the above example.
Exit code is used to communicate execution result to the parent process, so it's important that your program returns it.

The code above calls `AddCommandsFromThisAssembly()` to scan and resolve command types defined within the current assembly.
Commands are entry points, through which the user can interact with your application.

To define a command, create a new class by implementing the `ICommand` interface and annotate it with the `[Command]` attribute:

```csharp
[Command]
public class HelloWorldCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine("Hello world!");

        // Return default task if the command is not asynchronous
        return default;
    }
}
```

In order to implement `ICommand`, the class needs to define an `ExecuteAsync(...)` method.
This is the method that gets called by the framework when the user decides to execute this command.

As a parameter, this method takes an instance of `IConsole`, which is an abstraction around the system console.
Use this abstraction in place of `System.Console` whenever you need to write output, read input, or otherwise interact with the console.

With the basic setup above, the user can now run the application and get a greeting in return:

```sh
> dotnet myapp.dll

Hello world!
```

Out of the box, the application also comes with built-in `--help` and `--version` options.
They can be used to show help text or application version respectively:

```sh
> dotnet myapp.dll --help

MyApp v1.0

Usage
  dotnet myapp.dll [options]

Options
  -h|--help         Shows help text.
  --version         Shows version information.
```

```sh
> dotnet myapp.dll --version

v1.0
```

### Parameters and options

Commands can be configured to take input from command line arguments.
To do that, you need to add properties to the command class and annotate them with special attributes.

In CliFx, there are two types of argument bindings: **parameters** and **options**.
Parameters are positional arguments identified by the order they appear in, while options represent sets of arguments identified by their name.

As an example, here's a command that calculates the logarithm of a value, using a parameter binding to specify the input and an option binding to configure the logarithm base:

```csharp
[Command]
public class LogCommand : ICommand
{
    // Order: 0
    [CommandParameter(0, Description = "Value whose logarithm is to be found.")]
    public double Value { get; init; }

    // Name: --base
    // Short name: -b
    [CommandOption("base", 'b', Description = "Logarithm base.")]
    public double Base { get; init; } = 10;

    public ValueTask ExecuteAsync(IConsole console)
    {
        var result = Math.Log(Value, Base);
        console.Output.WriteLine(result);

        return default;
    }
}
```

In order to execute this command, at a minimum, the user needs to provide the input value:

```sh
> dotnet myapp.dll 10000

4
```

They can also pass the `base` option to override the default logarithm base of 10:

```sh
> dotnet myapp.dll 729 -b 3

6
```

In case the user forgets to specify the `value` parameter, the application will exit with an error:

```sh
> dotnet myapp.dll -b 10

Missing parameter(s):
<value>
```

Available parameters and options are also listed in the command's help text, which can be accessed by passing the `--help` option:

```sh
> dotnet myapp.dll --help

MyApp v1.0

Usage
  dotnet myapp.dll <value> [options]

Parameters
* value             Value whose logarithm is to be found.

Options
  -b|--base         Logarithm base. Default: "10".
  -h|--help         Shows help text.
  --version         Shows version information.
```

Overall, parameters and options are both used to consume input from the command line, but they differ in a few important ways:

- Parameters are identified by their relative order. Options are identified by their name or a single-character short name.
- Parameters technically also have a name, but it's only used in the help text.
- Parameters are always required. Options are normally optional, but can also be configured to require a value.
- Options can be configured to use an environment variable as a fallback.
- Both parameters and options can take multiple values, but there can only be one such parameter in a command and it must be the last in order. Options are not limited in this regard.

As a general guideline, it's recommended to use parameters for required inputs that the command can't function without.
Use options for all other non-required inputs or when specifying the name explicitly makes the usage clearer.

### Argument syntax

This library employs the POSIX argument syntax, which is used in most modern command line tools.
Here are some examples of how it works:

- `myapp --foo bar` sets option `"foo"` to value `"bar"`
- `myapp -f bar` sets option `'f'` to value `"bar"`
- `myapp --switch` sets option `"switch"` without value
- `myapp -s` sets option `'s'` without value
- `myapp -abc` sets options `'a'`, `'b'` and `'c'` without value
- `myapp -xqf bar` sets options `'x'` and `'q'` without value, and option `'f'` to value `"bar"`
- `myapp -i file1.txt file2.txt` sets option `'i'` to a sequence of values `"file1.txt"` and `"file2.txt"`
- `myapp -i file1.txt -i file2.txt` sets option `'i'` to a sequence of values `"file1.txt"` and `"file2.txt"`
- `myapp cmd abc -o` routes to command `cmd` (assuming it's an existing command) with parameter `abc` and sets option `'o'` without value

Additionally, argument parsing in CliFx aims to be as deterministic as possible, ideally yielding the same result regardless of the application configuration.
In fact, the only context-sensitive part in the parser is the command name resolution, which needs to know the list of available commands in order to discern between arguments that correspond to command name and arguments which map as parameters. 

The parser's context-free nature has several implications on how it consumes arguments.
For example, passing `myapp -i file1.txt file2.txt` will always be parsed as an option with multiple values, regardless of the arity of the underlying property it's bound to.
Similarly, unseparated arguments in the form of `myapp -ofile` will be treated as five distinct options `'o'`, `'f'`, `'i'`, `'l'`, `'e'`, instead of `'o'` being set to value `"file"`.

Because of these rules, order of arguments is semantically important and must always follow this pattern:

```ini
[directives] [command name] [parameters] [options]
```

### Value conversion

Parameters and options can have the following underlying types:

- Standard types
  - Primitive types (`int`, `bool`, `double`, `ulong`, `char`, etc.)
  - Date and time types (`DateTime`, `DateTimeOffset`, `TimeSpan`)
  - Enum types (converted from either name or numeric value)
- String-initializable types
  - Types with a constructor accepting `string` (`FileInfo`, `DirectoryInfo`, etc.)
  - Types with a static `Parse(...)` method accepting `string` and optionally `IFormatProvider` (`Guid`, `Uri`, etc.)
- Nullable versions of all above types (`decimal?`, `TimeSpan?`, etc.)
- Any other type if a custom converter is specified
- Collections of all above types
  - Array types (`T[]`)
  - Types that are assignable from arrays (`IReadOnlyList<T>`, `ICollection<T>`, etc.)
  - Types with a constructor accepting an array (`List<T>`, `HashSet<T>`, etc.)

- Example command with a custom converter:

```csharp
// Maps 2D vectors from AxB notation
public class VectorConverter : BindingConverter<Vector2>
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

[Command]
public class SurfaceCalculatorCommand : ICommand
{
    // Custom converter is used to map raw argument values
    [CommandParameter(0, Converter = typeof(VectorConverter))]
    public Vector2 PointA { get; init; }

    [CommandParameter(1, Converter = typeof(VectorConverter))]
    public Vector2 PointB { get; init; }

    [CommandParameter(2, Converter = typeof(VectorConverter))]
    public Vector2 PointC { get; init; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        var a = (PointB - PointA).Length();
        var b = (PointC - PointB).Length();
        var c = (PointA - PointC).Length();

        var p = (a + b + c) / 2;
        var surface = Math.Sqrt(p * (p - a) * (p - b) * (p - c));

        console.Output.WriteLine($"Triangle surface area: {surface}");

        return default;
    }
}
```

```sh
> dotnet myapp.dll 0x0 0x18 24x0

Triangle surface area: 216
```

- Example command with an array-backed parameter:

```csharp
[Command]
public class FileSizeCalculatorCommand : ICommand
{
    // FileInfo is string-initializable and IReadOnlyList<T> can be assgined from an array,
    // so the value of this property can be mapped from a sequence of arguments.
    [CommandParameter(0)]
    public IReadOnlyList<FileInfo> Files { get; init; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        var totalSize = Files.Sum(f => f.Length);

        console.Output.WriteLine($"Total file size: {totalSize} bytes");

        return default;
    }
}
```

```sh
> dotnet myapp.dll file1.bin file2.exe

Total file size: 186368 bytes
```

Same command, but using an option for the list of files instead:

```csharp
[Command]
public class FileSizeCalculatorCommand : ICommand
{
    [CommandOption("files")]
    public IReadOnlyList<FileInfo> Files { get; init; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        var totalSize = Files.Sum(f => f.Length);

        console.Output.WriteLine($"Total file size: {totalSize} bytes");

        return default;
    }
}
```

```sh
> dotnet myapp.dll --files file1.bin file2.exe

Total file size: 186368 bytes
```

### Multiple commands

In order to facilitate a variety of different workflows, command line applications may provide the user with more than just a single command.
Complex applications may also nest commands within each other, employing a multi-level hierarchical structure.

With CliFx, this is achieved by simply giving each command a unique name through the `[Command]` attribute.
Commands that have common name segments are considered to be hierarchically related, which affects how they're listed in the help text.

```csharp
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

Once configured, the user can execute a specific command by including its name in the passed arguments.
For example, running `dotnet myapp.dll cmd1 arg1 -p 42` will execute `FirstCommand` in the above example.

Requesting help will show direct subcommands of the current command:

```sh
> dotnet myapp.dll --help

MyApp v1.0

Usage
  dotnet myapp.dll [options]
  dotnet myapp.dll [command] [...]

Options
  -h|--help         Shows help text.
  --version         Shows version information.

Commands
  cmd1              Subcommands: cmd1 sub.
  cmd2

You can run `dotnet myapp.dll [command] --help` to show help on a specific command.
```

The user can also refine their help request by querying it on a specific command:

```sh
> dotnet myapp.dll cmd1 --help

Usage
  dotnet myapp.dll cmd1 [options]
  dotnet myapp.dll cmd1 [command] [...]

Options
  -h|--help         Shows help text.

Commands
  sub

You can run `dotnet myapp.dll cmd1 [command] --help` to show help on a specific command.
```

> Note that defining a default (unnamed) command is not required.
In the even of its absence, running the application without specifying a command will just show root level help text.

### Reporting errors

Commands in CliFx do not directly return exit codes, but can instead communicate execution errors by throwing `CommandException`.
This special exception can be used to print an error message to the console, return a specific exit code, and also optionally show help text for the current command:

```csharp
[Command]
public class DivideCommand : ICommand
{
    [CommandOption("dividend", IsRequired = true)]
    public double Dividend { get; init; }

    [CommandOption("divisor", IsRequired = true)]
    public double Divisor { get; init; }

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
> dotnet myapp.dll --dividend 10 --divisor 0

Division by zero is not supported.


> $LastExitCode

133
```

> Note that Unix systems rely on 8-bit unsigned integers to represent exit codes, which means that you can only use values between `1` and `255` to indicate an unsuccessful execution result.

### Graceful cancellation

Console applications support the concept of interrupt signals, which can be issued by the user to abort the currently ongoing operation.
If your command performs critical work, you can intercept these signals to handle cancellation requests in a graceful way.

To make command cancellation-aware, call `console.RegisterCancellationHandler()`.
This method configures a handler that listens for interrupt signals on the console and returns a `CancellationToken` that represents a potential cancellation request.

```csharp
[Command]
public class CancellableCommand : ICommand
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

> Note that a command may use this approach to delay cancellation only once.
If the user issues a second interrupt signal, the application will be immediately terminated.

### Type activation

Because CliFx takes responsibility for the application's entire lifecycle, it needs to be capable of instantiating various user-defined types at runtime.
To facilitate that, it uses an interface called `ITypeActivator` that determines how to create a new instance of a given type.

The default implementation of `ITypeActivator` only supports types that have public parameterless constructors, which is sufficient for majority of scenarios.
However, in some cases you may also want to define a custom initializer, for example when integrating with an external dependency container.

The following snippet shows how to configure your application to use [`Microsoft.Extensions.DependencyInjection`](https://nuget.org/packages/Microsoft.Extensions.DependencyInjection) as the type activator:

```csharp
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

Thanks to the `IConsole` abstraction, CliFx commands can be easily tested in isolation.
While an application running in production would rely on `SystemConsole` to interact with the real console, you can use `FakeConsole` and `FakeInMemoryConsole` in your tests to execute your commands in a simulated environment.

For example, imagine you have the following command:

```csharp
[Command]
public class ConcatCommand : ICommand
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

To test it, you can instantiate the command in code with the required values, and then pass an instance of `FakeInMemoryConsole` as parameter to `ExecuteAsync(...)`:

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
    
    var stdOut = console.ReadOutputString();

    // Assert
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

    var app = new CliApplicationBuilder()
        .AddCommand<ConcatCommand>()
        .UseConsole(console)
        .Build();

    var args = new[] {"--left", "foo", "--right", "bar"};
    var envVars = new Dictionary<string, string>();

    // Act
    await app.RunAsync(args, envVars);
    
    var stdOut = console.ReadOutputString();

    // Assert
    Assert.That(stdOut, Is.EqualTo("foo bar"));
}
```

### Debug and preview mode

When troubleshooting issues, you may find it useful to run your app in debug or preview mode.
To do that, you need to pass pass the corresponding directive before any other arguments.

In order to run the application in debug mode, use the `[debug]` directive.
This will cause the program to launch in a suspended state, waiting for debugger to be attached to the process:

```sh
> dotnet myapp.dll [debug] cmd -o

Attach debugger to PID 3148 to continue.
```

To run the application in preview mode, use the `[preview]` directive.
This will short-circuit the execution and instead print the consumed command line arguments as they were parsed, along with resolved environment variables:

```sh
> dotnet myapp.dll [preview] cmd arg1 arg2 -o foo --option bar1 bar2

Command line:
  cmd <arg1> <arg2> [-o foo] [--option bar1 bar2]
  
Environment:
  FOO="123"
  BAR="xyz"
```

You can also disallow these directives, e.g. when running in production, by calling `AllowDebugMode(...)` and `AllowPreviewMode(...)` methods on `CliApplicationBuilder`:

```csharp
var app = new CliApplicationBuilder()
    .AddCommandsFromThisAssembly()
    .AllowDebugMode(true) // allow debug mode
    .AllowPreviewMode(false) // disallow preview mode
    .Build();
```

### Environment variables

An option can be configured to use a specific environment variable as fallback.
If the user does not provide value for such option through command line arguments, the current value of the environment variable will be used instead.

```csharp
[Command]
public class AuthCommand : ICommand
{
    [CommandOption("token", IsRequired = true, EnvironmentVariable = "AUTH_TOKEN")]
    public string AuthToken { get; init; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(AuthToken);

        return default;
    }
}
```

```sh
> $env:AUTH_TOKEN="test"

> dotnet myapp.dll

test
```

Environment variables can be configured for options of non-scalar types as well.
In such case, the values of the environment variable will be split by `Path.PathSeparator` (`;` on Windows, `:` on Linux).

### Suggest mode

Suggest mode provides command-line autocompletion for Powershell and Bash. By default, it is disabled, but it can be enabled as follows:

```csharp
var app = new CliApplicationBuilder()
    .AddCommandsFromThisAssembly()
    .AllowSuggestMode(true) // allow suggest mode
    .Build();
```

Once enabled, your shell must be configured to use suggest mode as follows:

1. Add your application to the PATH
2. Add a snippet to your shell's to instruct your shell in how to generate auto-completions. This can be done as follows.
 
   ``` sh
   > cmd [suggest] --install
   ```

For Powershell terminals, code will be appended to the file at the $PROFILE location. A backup is made to the same location prior to modification. The snippet below is provided for users who would prefer to make the change manually.

``` powershell
### clifx-suggest-begins-here-CliFx.Demo-V1
# this block provides auto-complete for the CliFx.Demo command
# and assumes that CliFx.Demo is on the path
$scriptblock = {
    param($wordToComplete, $commandAst, $cursorPosition)        
        $command = "CliFx.Demo"

        $commandCacheId = "clifx-suggest-" + (new-guid).ToString()
        Set-Content -path "ENV:\$commandCacheId" -value $commandAst

        $result = &$command `[suggest`] --envvar $commandCacheId --cursor $cursorPosition | ForEach-Object {
                [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_)
         }

        Remove-Item -Path "ENV:\$commandCacheId"
        $result
}

Register-ArgumentCompleter -Native -CommandName "CliFx.Demo" -ScriptBlock $scriptblock
### clifx-suggest-ends-here-CliFx.Demo

```
For Bash terminals, code will be appended to the ~/.bashrc file. A backup is made to the same location prior to modification. The snippet below is provided for users who would prefer to make the change manually.

``` bash
### clifx-suggest-begins-here-CliFx.Demo-V1
# this block provides auto-complete for the CliFx.Demo command
# and assumes that CliFx.Demo is on the path
_CliFxDemo_complete()
{
local word=${COMP_WORDS[COMP_CWORD]}

# generate unique environment variable
CLIFX_CMD_CACHE="clifx-suggest-$(uuidgen)"
# replace hyphens with underscores to make it valid
CLIFX_CMD_CACHE=${CLIFX_CMD_CACHE//\-/_}

export $CLIFX_CMD_CACHE=${COMP_LINE}

local completions
completions="$(CliFx.Demo "[suggest]" --cursor "${COMP_POINT}" --envvar $CLIFX_CMD_CACHE 2>/dev/null)"
if [ $? -ne 0 ]; then
    completions=""
fi

unset $CLIFX_CMD_CACHE

COMPREPLY=( $(compgen -W "$completions" -- "$word") )
}

complete -f -F _CliFxDemo_complete "CliFx.Demo"

### clifx-suggest-ends-here-CliFx.Demo
```


## Etymology

CliFx is made out of "Cli" for "Command Line Interface" and "Fx" for "Framework". It's pronounced as "cliff ex".
