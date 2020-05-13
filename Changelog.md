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
