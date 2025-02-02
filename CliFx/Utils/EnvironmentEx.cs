using System;
using System.Diagnostics;
using System.Reflection;

namespace CliFx.Utils;

internal static class EnvironmentEx
{
    private static readonly Lazy<string?> ProcessPathLazy = new(() =>
    {
        using var process = Process.GetCurrentProcess();
        return process.MainModule?.FileName;
    });

    public static string? ProcessPath => ProcessPathLazy.Value;

    private static readonly Lazy<Assembly?> EntryAssemblyLazy = new(Assembly.GetEntryAssembly);

    public static Assembly? EntryAssembly => EntryAssemblyLazy.Value;
}
