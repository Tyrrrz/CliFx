using System;
using System.Collections.Generic;
using CliFx.Domain;
using CliFx.Tests.Internal;
using FluentAssertions;
using Xunit;

namespace CliFx.Tests
{
    public class ArgumentSyntaxSpecs
    {
        [Fact]
        public void Input_is_empty_if_no_arguments_are_provided()
        {
            // Arrange
            var arguments = Array.Empty<string>();
            var commandNames = new HashSet<string>();

            // Act
            var input = CommandInput.Parse(arguments, commandNames);

            // Assert
            input.Should().BeEquivalentTo(CommandInput.Empty);
        }

        public static object[][] DirectivesTestData => new[]
        {
            new object[]
            {
                new[] {"[preview]"},
                new CommandInputBuilder()
                    .AddDirective("preview")
                    .Build()
            },

            new object[]
            {
                new[] {"[preview]", "[debug]"},
                new CommandInputBuilder()
                    .AddDirective("preview")
                    .AddDirective("debug")
                    .Build()
            }
        };

        [Theory]
        [MemberData(nameof(DirectivesTestData))]
        internal void Directive_can_be_enabled_by_specifying_its_name_in_square_brackets(IReadOnlyList<string> arguments, CommandInput expectedInput)
        {
            // Arrange
            var commandNames = new HashSet<string>();

            // Act
            var input = CommandInput.Parse(arguments, commandNames);

            // Assert
            input.Should().BeEquivalentTo(expectedInput);
        }

        public static object[][] OptionsTestData => new[]
        {
            new object[]
            {
                new[] {"--option"},
                new CommandInputBuilder()
                    .AddOption("option")
                    .Build()
            },

            new object[]
            {
                new[] {"--option", "value"},
                new CommandInputBuilder()
                    .AddOption("option", "value")
                    .Build()
            },

            new object[]
            {
                new[] {"--option", "value1", "value2"},
                new CommandInputBuilder()
                    .AddOption("option", "value1", "value2")
                    .Build()
            },

            new object[]
            {
                new[] {"--option", "same value"},
                new CommandInputBuilder()
                    .AddOption("option", "same value")
                    .Build()
            },

            new object[]
            {
                new[] {"--option1", "--option2"},
                new CommandInputBuilder()
                    .AddOption("option1")
                    .AddOption("option2")
                    .Build()
            },

            new object[]
            {
                new[] {"--option1", "value1", "--option2", "value2"},
                new CommandInputBuilder()
                    .AddOption("option1", "value1")
                    .AddOption("option2", "value2")
                    .Build()
            },

            new object[]
            {
                new[] {"--option1", "value1", "value2", "--option2", "value3", "value4"},
                new CommandInputBuilder()
                    .AddOption("option1", "value1", "value2")
                    .AddOption("option2", "value3", "value4")
                    .Build()
            },

            new object[]
            {
                new[] {"--option1", "value1", "value2", "--option2"},
                new CommandInputBuilder()
                    .AddOption("option1", "value1", "value2")
                    .AddOption("option2")
                    .Build()
            }
        };

        [Theory]
        [MemberData(nameof(OptionsTestData))]
        internal void Option_can_be_set_by_specifying_its_name_after_two_dashes(IReadOnlyList<string> arguments, CommandInput expectedInput)
        {
            // Arrange
            var commandNames = new HashSet<string>();

            // Act
            var input = CommandInput.Parse(arguments, commandNames);

            // Assert
            input.Should().BeEquivalentTo(expectedInput);
        }

        public static object[][] ShortOptionsTestData => new[]
        {
            new object[]
            {
                new[] {"-o"},
                new CommandInputBuilder()
                    .AddOption("o")
                    .Build()
            },

            new object[]
            {
                new[] {"-o", "value"},
                new CommandInputBuilder()
                    .AddOption("o", "value")
                    .Build()
            },

            new object[]
            {
                new[] {"-o", "value1", "value2"},
                new CommandInputBuilder()
                    .AddOption("o", "value1", "value2")
                    .Build()
            },

            new object[]
            {
                new[] {"-o", "same value"},
                new CommandInputBuilder()
                    .AddOption("o", "same value")
                    .Build()
            },

            new object[]
            {
                new[] {"-a", "-b"},
                new CommandInputBuilder()
                    .AddOption("a")
                    .AddOption("b")
                    .Build()
            },

            new object[]
            {
                new[] {"-a", "value1", "-b", "value2"},
                new CommandInputBuilder()
                    .AddOption("a", "value1")
                    .AddOption("b", "value2")
                    .Build()
            },

            new object[]
            {
                new[] {"-a", "value1", "value2", "-b", "value3", "value4"},
                new CommandInputBuilder()
                    .AddOption("a", "value1", "value2")
                    .AddOption("b", "value3", "value4")
                    .Build()
            },

            new object[]
            {
                new[] {"-a", "value1", "value2", "-b"},
                new CommandInputBuilder()
                    .AddOption("a", "value1", "value2")
                    .AddOption("b")
                    .Build()
            },

            new object[]
            {
                new[] {"-abc"},
                new CommandInputBuilder()
                    .AddOption("a")
                    .AddOption("b")
                    .AddOption("c")
                    .Build()
            },

            new object[]
            {
                new[] {"-abc", "value"},
                new CommandInputBuilder()
                    .AddOption("a")
                    .AddOption("b")
                    .AddOption("c", "value")
                    .Build()
            },

            new object[]
            {
                new[] {"-abc", "value1", "value2"},
                new CommandInputBuilder()
                    .AddOption("a")
                    .AddOption("b")
                    .AddOption("c", "value1", "value2")
                    .Build()
            }
        };

        [Theory]
        [MemberData(nameof(ShortOptionsTestData))]
        internal void Option_can_be_set_by_specifying_its_short_name_after_a_single_dash(IReadOnlyList<string> arguments, CommandInput expectedInput)
        {
            // Arrange
            var commandNames = new HashSet<string>();

            // Act
            var input = CommandInput.Parse(arguments, commandNames);

            // Assert
            input.Should().BeEquivalentTo(expectedInput);
        }

        public static object[][] ParametersTestData => new[]
        {
            new object[]
            {
                new[] {"foo"},
                new CommandInputBuilder()
                    .AddParameter("foo")
                    .Build()
            },

            new object[]
            {
                new[] {"foo", "bar"},
                new CommandInputBuilder()
                    .AddParameter("foo")
                    .AddParameter("bar")
                    .Build()
            },

            new object[]
            {
                new[] {"[preview]", "foo"},
                new CommandInputBuilder()
                    .AddDirective("preview")
                    .AddParameter("foo")
                    .Build()
            },

            new object[]
            {
                new[] {"foo", "--option", "value", "-abc"},
                new CommandInputBuilder()
                    .AddParameter("foo")
                    .AddOption("option", "value")
                    .AddOption("a")
                    .AddOption("b")
                    .AddOption("c")
                    .Build()
            },

            new object[]
            {
                new[] {"[preview]", "[debug]", "foo", "bar", "--option", "value", "-abc"},
                new CommandInputBuilder()
                    .AddDirective("preview")
                    .AddDirective("debug")
                    .AddParameter("foo")
                    .AddParameter("bar")
                    .AddOption("option", "value")
                    .AddOption("a")
                    .AddOption("b")
                    .AddOption("c")
                    .Build()
            }
        };

        [Theory]
        [MemberData(nameof(ParametersTestData))]
        internal void Parameter_can_be_set_by_specifying_the_value_directly(IReadOnlyList<string> arguments, CommandInput expectedInput)
        {
            // Arrange
            var commandNames = new HashSet<string>();

            // Act
            var input = CommandInput.Parse(arguments, commandNames);

            // Assert
            input.Should().BeEquivalentTo(expectedInput);
        }

        public static object[][] CommandNameTestData => new[]
        {
            new object[]
            {
                new HashSet<string>() {"cmd"},
                new[] {"cmd"},
                new CommandInputBuilder()
                    .SetCommandName("cmd")
                    .Build()
            },

            new object[]
            {
                new HashSet<string>() {"cmd"},
                new[] {"cmd", "foo", "bar", "-o", "value"},
                new CommandInputBuilder()
                    .SetCommandName("cmd")
                    .AddParameter("foo")
                    .AddParameter("bar")
                    .AddOption("o", "value")
                    .Build()
            },

            new object[]
            {
                new HashSet<string>() {"cmd", "cmd sub"},
                new[] {"cmd", "sub", "foo"},
                new CommandInputBuilder()
                    .SetCommandName("cmd sub")
                    .AddParameter("foo")
                    .Build()
            }
        };

        [Theory]
        [MemberData(nameof(CommandNameTestData))]
        internal void Command_name_is_matched_from_arguments_that_come_before_parameters(
            ISet<string> commandNames,
            IReadOnlyList<string> arguments,
            CommandInput expectedInput)
        {
            // Act
            var input = CommandInput.Parse(arguments, commandNames);

            // Assert
            input.Should().BeEquivalentTo(expectedInput);
        }
    }
}