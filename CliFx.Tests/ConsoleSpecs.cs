using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using CliFx.Tests.Utils;
using CliFx.Tests.Utils.Extensions;
using CliWrap;
using CliWrap.Buffered;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests;

public class ConsoleSpecs : SpecsBase
{
    public ConsoleSpecs(ITestOutputHelper testOutput)
        : base(testOutput)
    {
    }

    [Fact]
    public async Task Real_console_maps_directly_to_system_console()
    {
        // Can't verify our own console output, so using an
        // external process for this test.

        // Arrange
        var command = "Hello world" | Cli.Wrap("dotnet")
            .WithArguments(a => a
                .Add(Dummy.Program.Location)
                .Add("console-test")
            );

        // Act
        var result = await command.ExecuteBufferedAsync();

        // Assert
        result.StandardOutput.Trim().Should().Be("Hello world");
        result.StandardError.Trim().Should().Be("Hello world");
    }

    [Fact]
    public async Task Fake_console_does_not_leak_to_system_console()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            @"
[Command]
public class Command : ICommand
{   
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.ResetColor();
        console.ForegroundColor = ConsoleColor.DarkMagenta;
        console.BackgroundColor = ConsoleColor.DarkMagenta;
        console.WindowWidth = 100;
        console.WindowHeight = 25;
        console.CursorLeft = 42;
        console.CursorTop = 24;

        console.Output.WriteLine(""Hello "");
        console.Error.WriteLine(""world!"");

        return default;
    }
}
");

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            Array.Empty<string>(),
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        Console.OpenStandardInput().Should().NotBeSameAs(FakeConsole.Input.BaseStream);
        Console.OpenStandardOutput().Should().NotBeSameAs(FakeConsole.Output.BaseStream);
        Console.OpenStandardError().Should().NotBeSameAs(FakeConsole.Error.BaseStream);

        Console.ForegroundColor.Should().NotBe(ConsoleColor.DarkMagenta);
        Console.BackgroundColor.Should().NotBe(ConsoleColor.DarkMagenta);

        // This fails because tests don't spawn a console window
        //Console.WindowWidth.Should().Be(100);
        //Console.WindowHeight.Should().Be(25);
        //Console.CursorLeft.Should().NotBe(42);
        //Console.CursorTop.Should().NotBe(24);

        FakeConsole.IsInputRedirected.Should().BeTrue();
        FakeConsole.IsOutputRedirected.Should().BeTrue();
        FakeConsole.IsErrorRedirected.Should().BeTrue();
    }

    [Fact]
    public async Task Fake_console_can_be_used_with_an_in_memory_backing_store()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            @"
[Command]
public class Command : ICommand
{   
    public ValueTask ExecuteAsync(IConsole console)
    {
        var input = console.Input.ReadToEnd();
        console.Output.WriteLine(input);
        console.Error.WriteLine(input);

        return default;
    }
}
");

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        FakeConsole.WriteInput("Hello world");

        var exitCode = await application.RunAsync(
            Array.Empty<string>(),
            new Dictionary<string, string>()
        );

        var stdOut = FakeConsole.ReadOutputString();
        var stdErr = FakeConsole.ReadErrorString();

        // Assert
        exitCode.Should().Be(0);
        stdOut.Trim().Should().Be("Hello world");
        stdErr.Trim().Should().Be("Hello world");
    }

    [Fact]
    public async Task Fake_console_can_read_key_presses()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            @"
[Command]
public class Command : ICommand
{   
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(console.ReadKey().Key);
        console.Output.WriteLine(console.ReadKey().Key);
        console.Output.WriteLine(console.ReadKey().Key);

        return default;
    }
}
");

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        FakeConsole.EnqueueKey(new ConsoleKeyInfo('0', ConsoleKey.D0, false, false, false));
        FakeConsole.EnqueueKey(new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false));
        FakeConsole.EnqueueKey(new ConsoleKeyInfo('\0', ConsoleKey.Backspace, false, false, false));

        var exitCode = await application.RunAsync(
            Array.Empty<string>(),
            new Dictionary<string, string>()
        );

        var stdOut = FakeConsole.ReadOutputString();

        // Assert
        exitCode.Should().Be(0);
        stdOut.Trim().Should().ConsistOfLines(
            "D0",
            "A",
            "Backspace"
        );
    }

    [Fact]
    public void Console_does_not_emit_preamble_when_used_with_encoding_that_has_it()
    {
        // Arrange
        using var buffer = new MemoryStream();
        using var consoleWriter = new ConsoleWriter(FakeConsole, buffer, Encoding.UTF8);

        // Act
        consoleWriter.Write("Hello world");
        consoleWriter.Flush();

        var output = consoleWriter.Encoding.GetString(buffer.ToArray());

        // Assert
        output.Should().Be("Hello world");
    }
}