using System;
using CliFx.Infrastructure;
using CliFx.Tests.Utils.Extensions;
using Xunit.Abstractions;

namespace CliFx.Tests;

public abstract class SpecsBase(ITestOutputHelper testOutput) : IDisposable
{
    public ITestOutputHelper TestOutput { get; } = testOutput;

    public FakeInMemoryConsole FakeConsole { get; } = new();

    public void Dispose()
    {
        FakeConsole.DumpToTestOutput(TestOutput);
        FakeConsole.Dispose();
    }
}
