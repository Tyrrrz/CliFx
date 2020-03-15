using System;
using CliFx.Domain;
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
            var args = Array.Empty<string>();

            // Act
            var input = CommandLineInput.Parse(args);

            // Assert
            input.Should().BeEquivalentTo(CommandLineInput.Empty);
        }

        public static object[][] DirectivesTestData => new[]
        {
            new object[]
            {
                new[] {"[preview]"},
                new CommandLineInputBuilder()
                    .AddDirective("preview")
                    .Build()
            },

            new object[]
            {
                new[] {"[preview]", "[debug]"},
                new CommandLineInputBuilder()
                    .AddDirective("preview")
                    .AddDirective("debug")
                    .Build()
            }
        };

        [Theory]
        [MemberData(nameof(DirectivesTestData))]
        internal void Directive_can_be_enabled_by_specifying_its_name_in_square_brackets(string[] arguments, CommandLineInput expectedInput)
        {
            // Act
            var input = CommandLineInput.Parse(arguments);

            // Assert
            input.Should().BeEquivalentTo(expectedInput);
        }

        public static object[][] OptionsTestData => new[]
        {
            new object[]
            {
                new[] {"--option"},
                new CommandLineInputBuilder()
                    .AddOption("option")
                    .Build()
            },

            new object[]
            {
                new[] {"--option", "value"},
                new CommandLineInputBuilder()
                    .AddOption("option", "value")
                    .Build()
            },

            new object[]
            {
                new[] {"--option", "value1", "value2"},
                new CommandLineInputBuilder()
                    .AddOption("option", "value1", "value2")
                    .Build()
            },

            new object[]
            {
                new[] {"--option", "same value"},
                new CommandLineInputBuilder()
                    .AddOption("option", "same value")
                    .Build()
            },

            new object[]
            {
                new[] {"--option1", "--option2"},
                new CommandLineInputBuilder()
                    .AddOption("option1")
                    .AddOption("option2")
                    .Build()
            },

            new object[]
            {
                new[] {"--option1", "value1", "--option2", "value2"},
                new CommandLineInputBuilder()
                    .AddOption("option1", "value1")
                    .AddOption("option2", "value2")
                    .Build()
            },

            new object[]
            {
                new[] {"--option1", "value1", "value2", "--option2", "value3", "value4"},
                new CommandLineInputBuilder()
                    .AddOption("option1", "value1", "value2")
                    .AddOption("option2", "value3", "value4")
                    .Build()
            },

            new object[]
            {
                new[] {"--option1", "value1", "value2", "--option2"},
                new CommandLineInputBuilder()
                    .AddOption("option1", "value1", "value2")
                    .AddOption("option2")
                    .Build()
            }
        };

        [Theory]
        [MemberData(nameof(OptionsTestData))]
        internal void Option_can_be_set_by_specifying_its_name_after_two_dashes(string[] arguments, CommandLineInput expectedInput)
        {
            // Act
            var input = CommandLineInput.Parse(arguments);

            // Assert
            input.Should().BeEquivalentTo(expectedInput);
        }

        public static object[][] ShortOptionsTestData => new[]
        {
            new object[]
            {
                new[] {"-o"},
                new CommandLineInputBuilder()
                    .AddOption("o")
                    .Build()
            },

            new object[]
            {
                new[] {"-o", "value"},
                new CommandLineInputBuilder()
                    .AddOption("o", "value")
                    .Build()
            },

            new object[]
            {
                new[] {"-o", "value1", "value2"},
                new CommandLineInputBuilder()
                    .AddOption("o", "value1", "value2")
                    .Build()
            },

            new object[]
            {
                new[] {"-o", "same value"},
                new CommandLineInputBuilder()
                    .AddOption("o", "same value")
                    .Build()
            },

            new object[]
            {
                new[] {"-a", "-b"},
                new CommandLineInputBuilder()
                    .AddOption("a")
                    .AddOption("b")
                    .Build()
            },

            new object[]
            {
                new[] {"-a", "value1", "-b", "value2"},
                new CommandLineInputBuilder()
                    .AddOption("a", "value1")
                    .AddOption("b", "value2")
                    .Build()
            },

            new object[]
            {
                new[] {"-a", "value1", "value2", "-b", "value3", "value4"},
                new CommandLineInputBuilder()
                    .AddOption("a", "value1", "value2")
                    .AddOption("b", "value3", "value4")
                    .Build()
            },

            new object[]
            {
                new[] {"-a", "value1", "value2", "-b"},
                new CommandLineInputBuilder()
                    .AddOption("a", "value1", "value2")
                    .AddOption("b")
                    .Build()
            },

            new object[]
            {
                new[] {"-abc"},
                new CommandLineInputBuilder()
                    .AddOption("a")
                    .AddOption("b")
                    .AddOption("c")
                    .Build()
            },

            new object[]
            {
                new[] {"-abc", "value"},
                new CommandLineInputBuilder()
                    .AddOption("a")
                    .AddOption("b")
                    .AddOption("c", "value")
                    .Build()
            },

            new object[]
            {
                new[] {"-abc", "value1", "value2"},
                new CommandLineInputBuilder()
                    .AddOption("a")
                    .AddOption("b")
                    .AddOption("c", "value1", "value2")
                    .Build()
            }
        };

        [Theory]
        [MemberData(nameof(ShortOptionsTestData))]
        internal void Option_can_be_set_by_specifying_its_short_name_after_a_single_dash(string[] arguments, CommandLineInput expectedInput)
        {
            // Act
            var input = CommandLineInput.Parse(arguments);

            // Assert
            input.Should().BeEquivalentTo(expectedInput);
        }

        public static object[][] UnboundArgumentsTestData => new[]
        {
            new object[]
            {
                new[] {"foo"},
                new CommandLineInputBuilder()
                    .AddUnboundArgument("foo")
                    .Build()
            },

            new object[]
            {
                new[] {"foo", "bar"},
                new CommandLineInputBuilder()
                    .AddUnboundArgument("foo")
                    .AddUnboundArgument("bar")
                    .Build()
            },

            new object[]
            {
                new[] {"[preview]", "foo"},
                new CommandLineInputBuilder()
                    .AddDirective("preview")
                    .AddUnboundArgument("foo")
                    .Build()
            },

            new object[]
            {
                new[] {"foo", "--option", "value", "-abc"},
                new CommandLineInputBuilder()
                    .AddUnboundArgument("foo")
                    .AddOption("option", "value")
                    .AddOption("a")
                    .AddOption("b")
                    .AddOption("c")
                    .Build()
            },

            new object[]
            {
                new[] {"[preview]", "[debug]", "foo", "bar", "--option", "value", "-abc"},
                new CommandLineInputBuilder()
                    .AddDirective("preview")
                    .AddDirective("debug")
                    .AddUnboundArgument("foo")
                    .AddUnboundArgument("bar")
                    .AddOption("option", "value")
                    .AddOption("a")
                    .AddOption("b")
                    .AddOption("c")
                    .Build()
            }
        };

        [Theory]
        [MemberData(nameof(UnboundArgumentsTestData))]
        internal void Any_remaining_arguments_are_treated_as_unbound_arguments(string[] arguments, CommandLineInput expectedInput)
        {
            // Act
            var input = CommandLineInput.Parse(arguments);

            // Assert
            input.Should().BeEquivalentTo(expectedInput);
        }
    }
}