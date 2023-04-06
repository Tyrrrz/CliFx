# Changelog

## v2.3.2 (06-Apr-2023)

- Added name-based ordering to subcommands when displayed in the help text. (Thanks [@CartBlanche](https://github.com/CartBlanche))

## v2.3.1 (08-Dec-2022)

- Added support for the `required` keyword introduced in C# 11. Command options bound to required properties are now marked as required by default (equivalent to setting `IsRequired = true` on the attribute). Also added analyzers that prevent marking an option or parameter non-required if it's bound to a required property.

## v2.3 (12-Jul-2022)

- Added console dimension properties `WindowWidth` and `WindowHeight` to `IConsole` interface and implementing classes.
- Improved inline documentation for members of `IConsole` interface.

## v2.2.6 (14-Jun-2022)

- Added an overload of `CliApplicationBuilder.UseTypeActivator(...)` that accepts an instance of `IServiceProvider`. This slightly simplifies integration with many DI containers.
- Fixed minor grammar mistakes in user-facing error messages.

## v2.2.5 (10-May-2022)

- Updated default value resolution for the application executable name. It will now resolve to `myapp.exe` instead of `dotnet myapp.dll` when the application is launched through the EXE apphost on Windows. On other platforms, or when running the application through the .NET CLI, the behavior will be the same as before.

## v2.2.4 (22-Apr-2022)

- Added more contextual information to analyzer diagnostics.
- Fixed an issue where the analyzer incorrectly reported an error on converters that didn't directly match the target type but were compatible through known built-in conversions.
- Fixed an issue where MSBuild produced a lot of analyzer-related warnings in certain circumstances.

## v2.2.3 (17-Apr-2022)

- Changed method signature of `IConsole.ReadKey()` to return `ConsoleKeyInfo` instead of `void`. The return type was originally defined as `void` by mistake. This change is source-backwards-compatible but may break on binary level if you were previously calling this method indirectly (i.e. through a library).
- Added `FakeConsole.EnqueueKey(...)` to facilitate the testing of `IConsole.ReadKey()`. You can use this method to simulate key presses in your application.
- Extended analyzers that verify the correctness of specified converters and validators. They now also ensure that the specified types are compatible with the type of the underlying property.
- Improved diagnostics produced by analyzers. Where relevant, highlighted code is now limited to the property or type identifier, instead of the whole property or type declaration. Also extended the diagnostic messages with additional information.
- Fixed an issue where throwing an exception inside a constructor of a command type resulted in an unrelated error message about the absence of a parameterless constructor.

## v2.2.2 (30-Jan-2022)

- Fixed an issue where `ConsoleWriter` and `ConsoleReader` were not properly thread-safe.
- Fixed an issue where the analyzer failed to load under certain circumstances when running inside Visual Studio.

## v2.2.1 (16-Jan-2022)

- Fixed an issue which caused help text to not show default values for optional parameters. (Thanks [@AliReZa Sabouri](https://github.com/alirezanet))

## v2.2 (11-Jan-2022)

- Added support for optional parameters. A parameter can be marked as optional by setting `IsRequired = false` on the attribute. Only one parameter is allowed to be optional and such parameter must be the last in order. (Thanks [@AliReZa Sabouri](https://github.com/alirezanet))
- Fixed an issue where parameters and options bound to properties implemented as default interface members were not working correctly. (Thanks [@AliReZa Sabouri](https://github.com/alirezanet))

## v2.1 (04-Jan-2022)

- Added `IConsole.Clear()` with corresponding implementations in `SystemConsole`, `FakeConsole`, and `FakeInMemoryConsole`. (Thanks [@Alex Rosenfeld](https://github.com/alexrosenfeld10))
- Added `IConsole.ReadKey()` with corresponding implementations in `SystemConsole`, `FakeConsole`, and `FakeInMemoryConsole`. (Thanks [@Alex Rosenfeld](https://github.com/alexrosenfeld10))
- Fixed an issue that caused parameters to appear out of order in the usage format section of the help text. (Thanks [@David Fallah](https://github.com/TAGC))

## v2.0.6 (17-Jul-2021)

- Fixed an issue where an exception thrown via reflection during parameter or option binding resulted in `Exception has been thrown by the target of an invocation` error instead of a more useful message. Such exceptions will now be unwrapped to provide better user experience.

## v2.0.5 (09-Jul-2021)

- Fixed an issue where calling `IConsole.Output.Encoding.EncodingName` and some other members threw an exception.
- Added readme file to the package.

## v2.0.4 (24-Apr-2021)

- Fixed an issue where output and error streams in `SystemConsole` defaulted to UTF8 encoding with BOM when the application was running with UTF8 codepage. `ConsoleWriter` will now discard preamble from the specified encoding. This fix brings the behavior of `SystemConsole` in line with .NET's own `System.Console` which also discards preamble for output and error streams.
- Fixed an issue where help text tried to show default values for parameters and options whose type does not override `ToString()` method.
- Fixed an issue where help text didn't show default values for parameters and options whose type is an enumerable of nullable enums. (Thanks [@Robert Dailey](https://github.com/rcdailey))
- Fixed an issue where specific parts of the help text weren't legible in some terminals due to low color resolution. Removed the usage of `ConsoleColor.DarkGray` in help text.

## v2.0.3 (09-Apr-2021)

- Improved help text by showing valid values for non-scalar enum parameters and options. (Thanks [@Robert Dailey](https://github.com/rcdailey))

## v2.0.2 (31-Mar-2021)

- Fixed an issue where having a transitive reference to CliFx sometimes resulted in `SystemConsoleShouldBeAvoidedAnalyzer` throwing `NullReferenceException` during build.
- Fixed some documentation typos and inconsistencies.

## v2.0.1 (24-Mar-2021)

- Fixed an issue where some exceptions with async stack traces generated on .NET 3.1 or earlier were not parsed and formatted correctly.
- Fixed an issue where help text applied slightly incorrect formatting when displaying choices for enum-based parameters and properties.

## v2.0 (21-Mar-2021)

> Note: this major release includes many breaking changes.
> Please refer to the readme to find updated instructions and usage examples.

- Renamed property `EnvironmentVariableName` to `EnvironmentVariable` on `CommandOption` attribute.
- Removed most of schema validation checks that used to take place during application startup. Going forward, CliFx will be relying solely on its built-in set of Roslyn analyzers to catch common errors in command configuration.
- Removed `ProgressTicker` utility. The recommended migration path is to use the [Spectre.Console](https://github.com/spectresystems/spectre.console) library which provides a wide array of console widgets and components. See [this wiki page](https://github.com/Tyrrrz/CliFx/wiki/Integrating-with-Spectre.Console) to learn how to integrate Spectre.Console with CliFx.
- Removed `MemoryStreamWriter` utility as it's no longer used within CliFx.
- Improved wording in all error messages.
- Renamed some methods on `CliApplicationBuilder`:
  - `UseTitle()` renamed to `SetTitle()`
  - `UseExecutableName()` renamed to `SetExecutableName()`
  - `UseVersionText()` renamed to `SetVersion()`
  - `UseDescription()` renamed to `SetDescription()`
- Changed the behavior of autogenerated help text:
  - Changed the color scheme to a more neutral set of tones
  - Assigned separate colors to parameters and options to make them visually stand out
  - Usage section no longer lists usage formats of all descendant commands
  - Command section now also lists available subcommands for each of the current command's subcommands
- Changed the behavior of `[preview]` directive. Running the application with this directive will now also print all resolved environment variables, in addition to parsed command-line arguments.
- Reworked `IArgumentValueConverter`/`ArgumentValueConverter` into `BindingConverter`. Method `ConvertFrom(...)` has been renamed to `Convert(...)`.
- Reworked `ArgumentValueValidator` into `BindingValidator`. This class exposes an abstract `Validate(...)` method that returns a nullable `BindingValidationError`. This class also provides utility methods `Ok()` and `Error(...)` to help create corresponding validation results.
- Changed the type of `IConsole.Output` and `IConsole.Error` from `StreamWriter` to `ConsoleWriter`. This type derives from `StreamWriter` and additionally exposes a `Console` property that refers to the console instance that owns the stream. This change enables you to author extension methods scoped specifically to console output and error streams.
- Changed the type of `IConsole.Input` from `StreamReader` to `ConsoleReader`. This type derives from `StreamReader` and additionally exposes a `Console` property that refers to the console instance that owns the stream. This change enables you to author extension methods scoped specifically to the console input stream.
- Changed methods `IConsole.WithForegroundColor(...)`, `IConsole.WithBackgroundColor(...)`, and `IConsole.WithColors(...)` to return `IDisposable`, replacing the delegate parameter they previously had. You can wrap the returned `IDisposable` in a using statement to ensure that the console colors get reset back to their original values once the execution reaches the end of the block.
- Renamed `IConsole.GetCancellationToken()` to `IConsole.RegisterCancellationHandler()`.
- Reworked `VirtualConsole` into `FakeConsole`. This class no longer takes `CancellationToken` as a constructor parameter, but instead encapsulates its own instance of `CancellationTokenSource` that can be triggered using the provided `RequestCancellation()` method.
- Removed `VirtualConsole.CreateBuffered()` and replaced it with the `FakeInMemoryConsole` class. This class derives from `FakeConsole` and uses in-memory standard input, output, and error streams. It also provides methods to easily read the data written to the streams.
- Moved some types to different namespaces:
  - `IConsole`/`FakeConsole`/`FakeInMemoryConsole` moved from `CliFx` to `CliFx.Infrastructure`
  - `ITypeActivator`/`DefaultTypeActivator`/`DelegateTypeActivator` moved from `CliFx` to `CliFx.Infrastructure`
  - `BindingValidator`/`BindingConverter` moved from `CliFx` to `CliFx.Extensibility`

## v1.6 (06-Dec-2020)

- Added support for custom value validators. You can now create a type that inherits from `CliFx.ArgumentValueValidator<T>` to implement reusable validation logic for command arguments. To use a validator, include it in the `Validators` property on the `CommandOption` or `CommandParameter` attribute. (Thanks [@Oleksandr Shustov](https://github.com/AlexandrShustov))
- Added `CliFx.ArgumentValueConverter<T>` class that you can inherit from to implement custom value converters. `CliFx.IArgumentValueConverter` interface is still available, but it is recommended to inherit from the generic class instead, due to the type safety it provides. The interface may become internal or get removed in one of the future major versions.
- Updated requirements for option names and short names: short names now must be letter characters (lowercase or uppercase), while names must now start with a letter character. This means option names can no longer start with a digit or a special character. This change makes it possible to pass negative number values without the need to quote them, i.e. `--my-number -5`.

## v1.5 (23-Oct-2020)

- Added pretty-printing for unhandled exceptions thrown from within the application. This makes the errors easier to parse visually and should help in troubleshooting. This change does not affect `CommandException`, as it already has special treatment. (Thanks [@Mårten Åsberg](https://github.com/89netraM))
- Added support for custom value converters. You can now create a type that implements `CliFx.IArgumentValueConverter` and specify it as a converter for your parameters or options via the `Converter` named property. This should enable conversion between raw argument values and custom types which are not string-initializable. (Thanks [@Oleksandr Shustov](https://github.com/AlexandrShustov))
- Improved help text so that it also shows minimal usage examples for child and descendant commands, besides the actual command it was requested on. This should improve user experience for applications with many nested commands. (Thanks [@Nikiforov Alexey](https://github.com/NikiforovAll))

## v1.4 (20-Aug-2020)

- Added `VirtualConsole.CreateBuffered()` method to simplify test setup when using in-memory backing stores for output and error streams. Please refer to the readme for updated recommendations on how to test applications built with CliFx.
- Added generic `CliApplicationBuilder.AddCommand<TCommand>()`. This overload simplifies adding commands one-by-one as it also checks that the type implements `ICommand`.

## v1.3.2 (31-Jul-2020)

- Fixed an issue where a command was incorrectly allowed to execute when the user did not specify any value for a non-scalar parameter. Since they are always required, a parameter needs to be bound to at least one value. (Thanks [@Daniel Hix](https://github.com/ADustyOldMuffin))
- Fixed an issue where `CliApplication.RunAsync(...)` threw `ArgumentException` if there were two environment variables, whose names differed only in case. Environment variable names are now treated case-sensitively. (Thanks [@Ron Myers](https://github.com/ron-myers))

## v1.3.1 (19-Jul-2020)

- Running the application with the debug directive (`myapp [debug]`) will now also try to launch a debugger instance. In most cases it will save time as you won't need to attach the debugger manually. (Thanks [@Volodymyr Shkolka](https://github.com/BlackGad))
- Fixed an issue where unhandled generic exceptions (i.e. not `CommandException`) sometimes caused the application to incorrectly return successful exit code due to an overflow issue on Unix systems. Starting from this version, all unhandled generic exceptions will produce `1` as the exit code when thrown. Instances of `CommandException` can still be configured to return any specified exit code, but it's recommended to constrain the values between `1` and `255` to avoid overflow issues. (Thanks [@Ihor Nechyporuk](https://github.com/inech))

## v1.3 (23-May-2020)

- Changed analyzers to report errors instead of warnings. If you find that some analyzer works incorrectly, please report it on GitHub. You can also configure inspection severity overrides in your project if you need to.
- Improved help text by showing default values for non-required options. This only works on types that have a custom override for `ToString()` method. Additionally, if the type implements `IFormattable`, the overload with a format provider will be used instead. (Thanks [@Domn Werner](https://github.com/domn1995))
- Changed default version text to only show 3 version components instead of 4, if the last component (revision) is not specified or is zero. This makes the default version text compliant with semantic versioning.
- Fixed an issue where it was possible to define a command with an option that has the same name or short name as built-in help or version options. Previously it would lead to the user-defined option being ignored in favor of the built-in option. Now this will throw an exception instead.
- Changed the underlying representation of `StreamReader`/`StreamWriter` objects used in `SystemConsole` and `VirtualConsole` to be thread-safe.

## v1.2 (11-May-2020)

- Added built-in Roslyn analyzers that help catch incorrect usage of the library. Currently, all analyzers report issues as warnings so as to not prevent the project from building. In the future that may change.
- Added an optional parameter to `new CommandException(...)` called `showHelp` which can be used to instruct CliFx to show help for the current command after printing the error. (Thanks [@Domn Werner](https://github.com/domn1995))
- Improved help text shown for enum options and parameters by providing the list of valid values that the enum can accept. (Thanks [@Domn Werner](https://github.com/domn1995))
- Fixed an issue where it was possible to set an option without providing a value, while the option was marked as required.
- Fixed an issue where it was possible to configure an option with an empty name or a name consisting of a single character. If you want to use a single character as a name, you should set the option's short name instead.
- Added `CursorLeft` and `CursorTop` properties to `IConsole` and its implementations. In `VirtualConsole`, these are just auto-properties.
- Improved exception messages.
- Improved exceptions related to user input by also showing help text after the error message. (Thanks [@Domn Werner](https://github.com/domn1995))

## v1.1 (16-Mar-2020)

- Changed `IConsole` interface (and as a result, `SystemConsole` and `VirtualConsole`) to support writing binary data. Instead of `TextReader`/`TextWriter` instances, the streams are now exposed as `StreamReader`/`StreamWriter` which provide the `BaseStream` property that allows raw access. Existing usages inside commands should remain the same because `StreamReader`/`StreamWriter` are compatible with their base classes `TextReader`/`TextWriter`, but if you were using `VirtualConsole` in tests, you may have to update it to the new API. Refer to the readme for more info.
- Changed argument binding behavior so that an error is produced if the user provides an argument that doesn't match with any parameter or option. This is done in order to improve user experience, as otherwise the user may make a typo without knowing that their input wasn't taken into account.
- Changed argument binding behavior so that options can be set to multiple argument values while specifying them with mixed naming. For example, `--option value1 -o value2 --option value3` would result in the option being set to corresponding three values, assuming `--option` and `-o` match with the same option.