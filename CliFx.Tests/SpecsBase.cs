using System;
using CliFx.Infrastructure;
using CliFx.Tests.Utils.Extensions;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public abstract class SpecsBase : IDisposable
    {
        public ITestOutputHelper TestOutput { get; }

        public FakeInMemoryConsole FakeConsole { get; } = new();

        public NullFileSystem NullFileSystem { get; } = new();

        protected SpecsBase(ITestOutputHelper testOutput) =>
            TestOutput = testOutput;

        public void Dispose()
        {
            FakeConsole.DumpToTestOutput(TestOutput);
            FakeConsole.Dispose();
        }
    }
}