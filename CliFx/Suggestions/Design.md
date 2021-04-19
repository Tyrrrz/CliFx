# Introduction

[CliFx](https://github.com/Tyrrrz/CliFx) was developed by [Alexey Golub](https://github.com/Tyrrrz) as a framework for the creation of rich and robust command line applications. 

A CliFx appilcation has a command line syntax as follows.

``` ps
PS C:\> cli.exe [command/subcommand] <parameter> -option(alias) [optionValue] --option(named) [optionValue] 
```

Commands are possible actions that the program can perform. Commands and subcommands provide a way of orgranising actions into sensible groups. For example: 

``` ps
PS C:\> cli.exe book <title>         # command, retrieves a book with title <title>
PS C:\> cli.exe book add <title>     # subcommand, add a book with title <title>
PS C:\> cli.exe book remove <title>  # subcommand, add a book with title <title>
PS C:\> cli.exe book list            # subcommand, add a book with title <title>
```

In the example above, `<title>` is a parameter. There may be any number of parameters, separated by whitespace, and are identified by the order in which they appear. Parameters are be string-initalizable; for instance, Enumerations are accepted from their string or numeric values.

Options always follow parameters, and are delimited with either the - or -- characters. A shorter option `alias` abbreviates the option with a single character (eg `-a`). Aliases can be combined to form a sequence: `-abc` is equivalent to `-a -b -c`.

# Auto completion Feature Description

The auto-completion feature provides useful suggestions to users of the application. Supported types are:

* commands/sub commands
* Enumeration parameters
* fully qualified option names (delimited by --)
* individual aliases (delimited by -). No attempt to suggest single aliases in the same token (eg -abc).

Suggestions are given for only one type at a time; ie. commands and parameters are not suggested together. Suggestions are provided as follows:

* Provides suggestions that **start with** the terms provided.
* Don't provide other completions if there's already a match.
* Be case insensitive when matching provided terms.


# User Experience
Auto-completions must be enabled by user as follows:

1. Ensure that the executable is on the path
2. Modify the terminal's user profile, adding a hook that calls CliFx for suggestions.
3. Reload terminal profile.

Users may use CliFx to install the suggestion hook automatically (step 2), or they do all three steps manually. Once installed, the user experience is varies depending on the terminal in use:

### Powershell (windows):
* tab to cycle through auto-completions
* control-space to select from auto-completions
```ps
# when: user presses tab for auto-complete
PS C:\> cli.exe <tab>
# then: auto-complete fills in the very first match
PS C:\> cli.exe book
# when: user continues to press tab...
PS C:\> cli.exe book<tab>
# then: auto-complete cycles through each match
PS C:\> cli.exe book add
PS C:\> cli.exe book list
PS C:\> cli.exe book remove

# when: the user presses control-space for auto-complete
PS C:\> cli.exe <ctrl-space>
# then: the user is presented with a series of options to select from:
PS C:\> cli.exe book
book        book add        book list       book remove
```

### Powershell (ubuntu):
* tab to view first auto-completion
* tab twice to see possible auto-completions
```ps
# when: user presses tab for auto-complete
PS C:\> cli.exe <tab>
# then: auto-complete fills in the very first match
PS C:\> cli.exe book
# when: user continues to press tab...
PS C:\> cli.exe book<tab>
# then: the user is presented with all options
book        book add        book list       book remove
PS C:\> cli.exe book
```

### Bash (ubuntu):
* tab to view first auto-completion
* tab twice to see possible auto-completions
```bash
# when: user presses tab for auto-complete
mauricel@DESKTOP: ~/$ cli <tab>
# then: auto-complete fills in the very first match
mauricel@DESKTOP: ~/$ cli book
# when: user continues to press tab...
mauricel@DESKTOP: ~/$ cli book<tab>
# then: the user is presented with all options
book        book add        book list       book remove     ...plus any files present
mauricel@DESKTOP: ~/$ cli
```

# Architecture

With CliFx, the auto-complete feature is comprised of the following components

* terminal hook. This is typically an addition to a user's profile. The hook
  * identifies the command to trigger upon 
  * extracts cursor position and command line arguments 
  * calls CliFx to determine the suggestions to provide suggestions
* CliFx implements the `[suggest]` directive, which is called as follows:

```PS
# for CliFx.Demo book add title_of_book

CliFx.Demo.exe [suggest] CliFx.Demo book add title_of_book
# completion suggestions provided, one per line, in standard output
```
* The command line, and the cursor position can also be supplied by environment variable for more accurate results.

```PS
# for CliFx.Demo book add title_of_book

CliFx.Demo.exe [suggest] CliFx.Demo --envar <name_of_variable> --cursor <index_of_cursor>
# completion suggestions provided, one per line, in standard output
```

## Configuration

The `[suggest]` directive (or suggest mode) is configured with CliApplicationBuilder as follows. For now, it is disabled by default.

```cs
new CliApplicationBuilder()
                // other configuration here.
                .AllowSuggestMode(true)
                .Build()
                .RunAsync();
```

## Installation
Installation consists of updating user profile to invoke CliFx suggest mode. Installation can be invoked by user on a per-user basis as follows:

```PS
cli [suggest] --install
# powershell refresh
& $profile

# bash refresh
source ~/.bashrc
```
The following environments are supported

* Windows Powershell, detected by the presence of "C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe"
* Ubuntu Powershell, detected by the presence of "/usr/bin/pwsh"
* Ubuntu Bash, detected by the presence of "/usr/bin/bash"

Platform specific behaviour (eg. File.Exists()) is very different in a Ubuntu WSL environment depending on whether the executable is an .exe or a linux executable, and the location of the executable (ie /mnt/c/src vs ~/src). In short, platform detection does not work unless the appropriate executable is run in a WSL environment. 

Installation works by adding text to the appropriate profile file. To support future upgrade scenarios, and to guarrantee idempotency, the text is sandwitched between the following identifiers:

```
### clifx-suggest-begins-here-{commandName}-{scriptVersion}
<script goes here>
### clifx-suggest-ends-here-{commandName)";
```

A .backup file is always created at the profile location.

## Suggest Mode Workflow

1. `[suggest]` directive detected by `CommandInput`
2. `CliApplication` checks for --install option.
   1. `SuggestShellHookInstaller` detects available environments, modifies profiles.
   2. End.
3. `CliApplication` retrieves suggestions from `SuggestionService`
   1. `SuggestionService` builds the raw user input from information supplied to `[suggest]` mode. See ExtractCommandText().
   2. `SuggestionService` splits the user input into arguments.
   3. `CommandInput.Parse()` parses the arguments, producing a `CommandInput`
   4. The parse information in `CommandInput` is used to create suggestions.
4. `CliApplication` outputs suggestions to standard output.


# Design Considerations

### Cursor positioning and argument delimination

The command line is supplied to `CommandInput` as an array of strings. The raw command line is not made available via the .NET libraries, posing the following challenges:

1. How to behave when the cursor in the command line is not always at the end of the string.

```ps
PS C:\> cmd.exe book a
                # ^ cursor might be here - auto completion should be for "boo"
```
2. How to handle difficult Powershell escaping

Consider the following application:
```cs

public static void Main()
{
    foreach(var arg in Environment.GetCommandLineArgs())
    {
      Console.WriteLine(arg);
    }  
}

```
Consider the following powershell behaviour
```ps

PS C:\> cli a b "a b" 'a b' '"a b"' "`"a b`""
cli
a
b
a b
a b
a b # expected "a b" (quotes inclusive)?
a b # expected "a b" (quotes inclusive)?
```

The behaviour gets stranger when the following auto-completion hook is considered:

```ps
$scriptblock = {
    param($wordToComplete, $commandAst, $cursorPosition)        
        $command = "CliFx.Demo"

        $result = &$command `[suggest`] $commandAst     # note: $commandAst passed as single parameter
        Set-content -path C:\temp\log.txt -value $result 
}

Register-ArgumentCompleter -Native -CommandName "CliFx.Demo" -ScriptBlock $scriptblock
```

We get the following output:

```ps
CliFx.Demo
[suggest]
CliFx.Demo a b a
b 'a b' 'a
b' `a b`"
```

Observations:
1. Text passed to the `[suggest]` directive is split inconsistently when passed via the powershell hook.
2. Cursor positioning seems difficult to predict in relation to the rest of the supplied terms. Note that CliFx is supplied with command line arguments after processing for whitespace. I couldn't find a platform indepdenant way of extracting the raw command line string. 

Workaround: the terminal hooks capture the command string as an environment variable. The name of the environment variable and cursor position is passed to the `[suggest]` directive as options. The hooks restrict the scope of the environment variable to the terminal process tree and the variable is removed after usage.

The command line is then trimmed to cursor length, split into arguments using [System.Environment.GetCommandLineArgs()](https://docs.microsoft.com/en-us/dotnet/api/system.environment.getcommandlineargs?view=net-5.0) rules. See `CliFx.Utils.CommandLineSplitter`. 

There may be some further issues with powershell escape characters, however what is presented currently should be sufficient given the nature of the feature.

### Powershell: whitepace handling in sub-commands
Powershell completions in Windows do not handle the sub command white-space well. For example, `book a` is autocompleted to `book book add`. As such, `SuggestionService.GetSuggestions()` has been written to:

* not to provide suggestions when the terms already match
* provide completions to terms already provided. For the example above, where `book a` returns `add`.

Though this was not explored, it may have implications if changing from a **starts with** to a **contains** strategy.

# Known Issues

### Bash auto-completion does not suggest child commands in empty directories

```
Given: a Bash shell, working in an empty directory
When: user types CliFx.Demo <tab><tab>
Then: suggestions are [book] instead of [book, book add, book list, book remove]
Note: cannot reproduce issue if 'book list' is renamed to 'boom' 
```
Futher exploration of the bash auto-completion hooks is required.