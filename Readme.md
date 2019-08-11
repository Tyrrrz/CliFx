# CliFx

[![Build](https://img.shields.io/appveyor/ci/Tyrrrz/CliFx/master.svg)](https://ci.appveyor.com/project/Tyrrrz/CliFx/branch/master)
[![Tests](https://img.shields.io/appveyor/tests/Tyrrrz/CliFx/master.svg)](https://ci.appveyor.com/project/Tyrrrz/CliFx/branch/master/tests)
[![Coverage](https://img.shields.io/codecov/c/gh/Tyrrrz/CliFx/master.svg)](https://codecov.io/gh/Tyrrrz/CliFx)
[![NuGet](https://img.shields.io/nuget/v/CliFx.svg)](https://nuget.org/packages/CliFx)
[![NuGet](https://img.shields.io/nuget/dt/CliFx.svg)](https://nuget.org/packages/CliFx)
[![Donate](https://img.shields.io/badge/patreon-donate-yellow.svg)](https://patreon.com/tyrrrz)
[![Donate](https://img.shields.io/badge/buymeacoffee-donate-yellow.svg)](https://buymeacoffee.com/tyrrrz)

CliFx is a powerful framework for building command line applications.

## Download

- [NuGet](https://nuget.org/packages/CliFx): `dotnet add package CliFx`
- [Continuous integration](https://ci.appveyor.com/project/Tyrrrz/CliFx)

## Features

- ...to be added with a stable release...
- Targets .NET Framework 4.6+ and .NET Standard 2.0+
- No external dependencies

## Usage

### Configuring application

To turn your application into a command line interface, you need to change your program's `Main` method so that it delegates execution to `CliApplication`.

```c#
public static class Program
{
    public static Task<int> Main(string[] args) =>
        new CliApplicationBuilder()
            .WithCommandsFromThisAssembly()
            .Build()
            .RunAsync(args);
}
```

The above code will create and run a default `CliApplication` that will resolve commands defined in the calling assembly.
If you want to configure different aspects of your application, you can use the fluent interface provided by the `CliApplicationBuilder`.

### Defining a command

To define a command, you need to create a class that implements `ICommand` and decorate it with a `Command` attribute.
To define options, the corresponding properties need to be decorated with a `CommandOption` attribute.

```c#
[Command("log", Description = "Calculates the logarithm of a value.")]
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

Commands may or may not have a name (which is `"log"` in this case). Command that doesn't have a name is the default command, which is executed when the user doesn't specify a command. If your application doesn't define a default command, a stub will be provided at runtime automatically by CliFx.

Commands usually also have options, each with a name (`"value"`, `"base"`), a short name (`'v'`, `'b'`), or both. Properties marked with `CommandOption` attribute need to be public and have an accessible setter. If you want to set a default value for an option, simply set the default value for the corresponding property.

By implementing `ICommand`, you're required to provide an `ExecuteAsync` method. This is the method that will be called when the command is invoked. Its return type is `Task` in order to facilitate asynchronous execution, but if your command runs synchronously you can simply return `Task.CompletedTask` at the end.

When interacting with the console, the command is expected to use the `IConsole` instance provided as a parameter to `ExecuteAsync` instead of using `System.Console`. This makes testing easier because you will be able to substitute or mock `IConsole` in your tests.

Finally, the command defined above can be executed from the command line in one of the following ways:

- `myapp log -v 1000`
- `myapp log --value 8 --base 2`
- `myapp log -v 81 -b 3`

### Dependency injection

CliFx uses an implementation of `ICommandFactory` to initialize commands and by default it only works with types that have parameterless constructors.
In real-life scenarios your commands will most likely have dependencies which need to be injected. CliFx makes it really easy to plug in any dependency container of your choice.

For example, here is how you would configure your application to use [`Microsoft.Extensions.DependencyInjection`](https://nuget.org/packages/Microsoft.Extensions.DependencyInjection/) (aka the built-in dependency container in ASP.NET Core).

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

        var serviceProvider = new DefaultServiceProviderFactory().CreateServiceProvider(services);

        return new CliApplicationBuilder()
            .WithCommandsFromThisAssembly()
            .UseCommandFactory(type => (ICommand) serviceProvider.GetRequiredService(type))
            .Build()
            .RunAsync(args);
    }
}
```

### Resolve specific commands or commands from other assemblies

In most cases, your commands will probably be defined in your main assembly, which is where CliFx will look if you initialize the application using the following code.

```c#
var app = new CliApplicationBuilder().WithCommandsFromThisAssembly().Build();
```

If you want to resolve specific commands or commands from another assembly you can use `WithCommand` and `WithCommandsFrom` methods to do that.

```c#
var app = new CliApplicationBuilder()
    .WithCommand(typeof(CommandA)) // include CommandA specifically
    .WithCommand(typeof(CommandB)) // include CommandB specifically
    .WithCommandsFrom(typeof(CommandC).Assembly) // include all commands from assembly that contains CommandC
    .Build();
```

### Child commands

In a more complex application you may need to build a hierarchy of commands. CliFx takes the approach of resolving hierarchy at runtime using command names so that you don't have to explicitly specify any child-parent relationships.

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

You may have noticed that commands in CliFx don't return exit codes. This is by design, exit codes are handled by `CliApplication`, not by individual commands.

Commands can report execution failure simply by throwing an exception, just like in any other C# code. The exit code will be automatically set to a non-zero value to indicate failure to the calling process.

If you want to communicate a specific error through exit code, you can throw an instance of `CommandErrorException` which takes exit code as a constructor parameter.

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
            // Exit code will be 1337
            throw new CommandErrorException(1337, "Division by zero is not supported");
        }

        var result = Dividend / Divisor;
        console.Output.WriteLine(result);

        return Task.CompletedTask;
    }
}
```

### Testing

Testing of commands is enabled by the `IConsole` interface.

The easiest way to substitute custom stdin, stdout, stderr is by using an instance of `TestConsole` class. It has multiple constructor overloads allowing you to specify the exact set of streams that you want. Streams that are not provided are treated as empty, i.e. `TestConsole` doesn't leak to `System.Console` in any way.

Let's assume you have a simple command such as this one.

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
        var console = new TestConsole(stdout);

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
        var console = new TestConsole(stdout);

        var app = new CliApplicationBuilder()
            .WithCommand(typeof(ConcatCommand))
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

## Libraries used

- [NUnit](https://github.com/nunit/nunit)
- [CliWrap](https://github.com/Tyrrrz/CliWrap)
- [FluentAssertions](https://github.com/fluentassertions/fluentassertions)
- [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet)
- [Coverlet](https://github.com/tonerdo/coverlet)

## Donate

If you really like my projects and want to support me, consider donating to me on [Patreon](https://patreon.com/tyrrrz) or [BuyMeACoffee](https://buymeacoffee.com/tyrrrz). All donations are optional and are greatly appreciated. 🙏