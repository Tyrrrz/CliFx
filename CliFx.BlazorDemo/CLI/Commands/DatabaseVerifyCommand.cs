using System;
using System.Threading;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Utilities;
using ShellProgressBar;

namespace CliFx.BlazorDemo.CLI.Commands
{
    [Command("database verify", Description = "Migrates the database.")]
    public class DatabaseVerifyCommand : ICommand
    {
        private readonly ICliContext _cliContext;

        public DatabaseVerifyCommand(ICliContext cliContext)
        {
            _cliContext = cliContext;
        }

        private bool RequestToQuit { get; set; }

        private void TickToCompletion(IProgressBar pbar, int ticks, int sleep = 1750, Action<int> childAction = null)
        {
            var initialMessage = pbar.Message;
            for (var i = 0; i < ticks && !RequestToQuit; i++)
            {
                pbar.Message = $"Start {i + 1} of {ticks} {Console.CursorTop}/{Console.WindowHeight}: {initialMessage}";
                childAction?.Invoke(i);
                Thread.Sleep(sleep);
                pbar.Tick($"End {i + 1} of {ticks} {Console.CursorTop}/{Console.WindowHeight}: {initialMessage}");
            }
        }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            await console.Output.WriteLineAsync("This will normally verify EF Core migrations.");

            // CliFx progress
            {
                var progressTicker = console.CreateProgressTicker();
                for (var i = 0.0; i <= 1.01; i += 0.01)
                {
                    progressTicker.Report(i);
                    await Task.Delay(15);
                }
                console.Output.WriteLine();
            }

            // ShellProgressBar
            {
                const int totalTicks = 10;
                var options = new ProgressBarOptions
                {
                    ForegroundColor = ConsoleColor.Yellow,
                    BackgroundColor = ConsoleColor.DarkGray,
                    ProgressCharacter = '─'
                };
                var childOptions = new ProgressBarOptions
                {
                    ForegroundColor = ConsoleColor.DarkGreen,
                    BackgroundColor = ConsoleColor.DarkGray,
                    ProgressCharacter = '\u2593',
                    CollapseWhenFinished = true
                };
                using (var pbar = new ProgressBar(totalTicks, "main progressbar", options))
                {
                    TickToCompletion(pbar, totalTicks, sleep: 10, childAction: (i) =>
                    {
                        using (var child = pbar.Spawn(totalTicks, "child actions", childOptions))
                        {
                            TickToCompletion(child, totalTicks, sleep: 100);
                        }
                    });
                }
            }
        }
    }
}
