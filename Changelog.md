### v2.0 (XX-XX-2020)

- Added interactive mode `CliInteractiveApplication` and interactive only commands.
- Added `ICliExceptionHandler` and `CliApplicationBuilder.UseExceptionHandler(...)`
- Added	`Manual` property in `CommandAttribute` that can be used to provide a long, extended description of a commmand.
- Added `CliContext` that can be injected to services and commands with DI.
- Added `WindowWidth`, `WindowHeight`, `BufferWidth`, and `BufferHeight` to `IConsole`.
- Added new demo apps and improved existing demo.
- Added `"Debugger attached to PID {processId}.` message after debugger attachment.
- Added benchmarks for multiple commands.
- Added startup message option with macros.
- Rewritten `RootSchema` with HashSet.
- Added tests of the command used in benchmarking to easily check if it executs correctly and won't cause banchmarking freezing.
- Improved code readability.
- Removed `CliApplicationBuilder.UseTypeActivator` and added Microsoft.Extensions.DependencyInjection

### v1.3.2 (31-Jul-2020)

- Fixed an issue where a command was incorrectly allowed to execute when the user did not specify any value for a non-scalar parameter. Since they are always required, a parameter needs to be bound to (at least) one value. (Thanks [@Daniel Hix](https://github.com/ADustyOldMuffin))
- Fixed an issue where `CliApplication.RunAsync(...)` threw `ArgumentException` if there were two environment variables, whose names differed only in case. Environment variable names are now treated case-sensitively. (Thanks [@Ron Myers](https://github.com/ron-myers))

### v1.3.1 (19-Jul-2020)

- Running the application with the debug directive (`myapp [debug]`) will now also try to launch a debugger instance. In most cases it will save time as you won't need to attach the debugger manually. (Thanks [@Volodymyr Shkolka](https://github.com/BlackGad))
- Fixed an issue where unhandled generic exceptions (i.e. not `CommandException`) sometimes caused the application to incorrectly return successful exit code due to an overflow issue on Unix systems. Starting from this version, all unhandled generic exceptions will produce `1` as the exit code when thrown. Instances of `CommandException` can still be configured to return any specified exit code, but it's recommended to constrain the values between `1` and `255` to avoid overflow issues. (Thanks [@Ihor Nechyporuk](https://github.com/inech))

### v1.3 (23-May-2020)

- Changed analyzers to report errors instead of warnings. If you find that some analyzer works incorrectly, please report it on GitHub. You can also configure inspection severity overrides in your project if you need to.
- Improved help text by showing default values for non-required options. This only works on types that have a custom override for `ToString()` method. Additionally, if the type implements `IFormattable`, the overload with a format provider will be used instead. (Thanks [@Domn Werner](https://github.com/domn1995))
- Changed default version text to only show 3 version components instead of 4, if the last component (revision) is not specified or is zero. This makes the default version text compliant with semantic versioning.
- Fixed an issue where it was possible to define a command with an option that has the same name or short name as built-in help or version options. Previously it would lead to the user-defined option being ignored in favor of the built-in option. Now this will throw an exception instead.
- Changed the underlying representation of `StreamReader`/`StreamWriter` objects used in `SystemConsole` and `VirtualConsole` to be thread-safe.

### v1.2 (11-May-2020)

- Added built-in Roslyn analyzers that help catch incorrect usage of the library. Currently, all analyzers report issues as warnings so as to not prevent the project from building. In the future that may change.
- Added an optional parameter to `new CommandException(...)` called `showHelp` which can be used to instruct CliFx to show help for the current command after printing the error. (Thanks [@Domn Werner](https://github.com/domn1995))
- Improved help text shown for enum options and parameters by providing the list of valid values that the enum can accept. (Thanks [@Domn Werner](https://github.com/domn1995))
- Fixed an issue where it was possible to set an option without providing a value, while the option was marked as required.
- Fixed an issue where it was possible to configure an option with an empty name or a name consisting of a single character. If you want to use a single character as a name, you should set the option's short name instead.
- Added `CursorLeft` and `CursorTop` properties to `IConsole` and its implementations. In `VirtualConsole`, these are just auto-properties.
- Improved exception messages.
- Improved exceptions related to user input by also showing help text after the error message. (Thanks [@Domn Werner](https://github.com/domn1995))

### v1.1 (16-Mar-2020)

- Changed `IConsole` interface (and as a result, `SystemConsole` and `VirtualConsole`) to support writing binary data. Instead of `TextReader`/`TextWriter` instances, the streams are now exposed as `StreamReader`/`StreamWriter` which provide the `BaseStream` property that allows raw access. Existing usages inside commands should remain the same because `StreamReader`/`StreamWriter` are compatible with their base classes `TextReader`/`TextWriter`, but if you were using `VirtualConsole` in tests, you may have to update it to the new API. Refer to the readme for more info.
- Changed argument binding behavior so that an error is produced if the user provides an argument that doesn't match with any parameter or option. This is done in order to improve user experience, as otherwise the user may make a typo without knowing that their input wasn't taken into account.
- Changed argument binding behavior so that options can be set to multiple argument values while specifying them with mixed naming. For example, `--option value1 -o value2 --option value3` would result in the option being set to corresponding three values, assuming `--option` and `-o` match with the same option.
