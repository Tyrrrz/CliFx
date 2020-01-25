using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CliFx.Domain;
using CliFx.Exceptions;
using CliFx.Tests.TestCommands;
using CliFx.Tests.TestCustomTypes;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests.Domain
{
    [TestFixture]
    internal partial class ApplicationSchemaTests
    {
        private static IEnumerable<TestCaseData> GetTestCases_Resolve()
        {
            yield return new TestCaseData(
                new[] { typeof(DivideCommand), typeof(ConcatCommand), typeof(EnvironmentVariableCommand) },
                new[]
                {
                    new CommandSchema(typeof(DivideCommand), "div", "Divide one number by another.",
                        new CommandParameterSchema[0], new[]
                        {
                            new CommandOptionSchema(typeof(DivideCommand).GetProperty(nameof(DivideCommand.Dividend)),
                                "dividend", 'D', null, true, "The number to divide."),
                            new CommandOptionSchema(typeof(DivideCommand).GetProperty(nameof(DivideCommand.Divisor)),
                                "divisor", 'd', null, true, "The number to divide by.")
                        }),
                    new CommandSchema(typeof(ConcatCommand), "concat", "Concatenate strings.",
                        new CommandParameterSchema[0],
                        new[]
                        {
                            new CommandOptionSchema(typeof(ConcatCommand).GetProperty(nameof(ConcatCommand.Inputs)),
                                null, 'i', null, true, "Input strings."),
                            new CommandOptionSchema(typeof(ConcatCommand).GetProperty(nameof(ConcatCommand.Separator)),
                                null, 's', null, false, "String separator.")
                        }),
                    new CommandSchema(typeof(EnvironmentVariableCommand), null, "Reads option values from environment variables.",
                        new CommandParameterSchema[0],
                        new[]
                        {
                            new CommandOptionSchema(typeof(EnvironmentVariableCommand).GetProperty(nameof(EnvironmentVariableCommand.Option)),
                                "opt", null, "ENV_SINGLE_VALUE", false, null)
                        }
                    )
                }
            );

            yield return new TestCaseData(
                new[] { typeof(HelloWorldDefaultCommand) },
                new[]
                {
                    new CommandSchema(typeof(HelloWorldDefaultCommand), null, null, new CommandParameterSchema[0], new CommandOptionSchema[0])
                }
            );
        }

        private static IEnumerable<TestCaseData> GetTestCases_Resolve_Negative()
        {
            yield return new TestCaseData(new object[]
            {
                new Type[0]
            });

            // Command validation failure

            yield return new TestCaseData(new object[]
            {
                new[] {typeof(NonImplementedCommand)}
            });

            yield return new TestCaseData(new object[]
            {
                // Same name
                new[] {typeof(ExceptionCommand), typeof(CommandExceptionCommand)}
            });

            yield return new TestCaseData(new object[]
            {
                new[] {typeof(NonAnnotatedCommand)}
            });

            // Argument validation failure

            yield return new TestCaseData(new object[]
            {
                new[] {typeof(DuplicateParameterOrderCommand)}
            });

            yield return new TestCaseData(new object[]
            {
                new[] {typeof(DuplicateParameterOrderCommand)}
            });

            yield return new TestCaseData(new object[]
            {
                new[] {typeof(MultipleNonScalarParametersCommand)}
            });

            yield return new TestCaseData(new object[]
            {
                new[] {typeof(NonLastNonScalarParameterCommand)}
            });

            // Option validation failure

            yield return new TestCaseData(new object[]
            {
                new[] {typeof(DuplicateOptionNamesCommand)}
            });

            yield return new TestCaseData(new object[]
            {
                new[] {typeof(DuplicateOptionShortNamesCommand)}
            });
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_Resolve))]
        public void Resolve_Test(
            IReadOnlyList<Type> commandTypes,
            IReadOnlyList<CommandSchema> expectedCommandSchemas)
        {
            // Act
            var applicationSchema = ApplicationSchema.Resolve(commandTypes);

            // Assert
            applicationSchema.Commands.Should().BeEquivalentTo(expectedCommandSchemas);
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_Resolve_Negative))]
        public void Resolve_Negative_Test(IReadOnlyList<Type> commandTypes)
        {
            // Act & Assert
            Assert.Throws<CliFxException>(() => ApplicationSchema.Resolve(commandTypes));
        }
    }

    internal partial class ApplicationSchemaTests
    {
        private static IEnumerable<TestCaseData> GetTestCases_TryInitialize()
        {
            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.Object)}", "value"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {Object = "value"}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.String)}", "value"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {String = "value"}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.Bool)}", "true"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {Bool = true}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.Bool)}", "false"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {Bool = false}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.Bool)}"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {Bool = true}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.Char)}", "a"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {Char = 'a'}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.Sbyte)}", "15"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {Sbyte = 15}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.Byte)}", "15"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {Byte = 15}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.Short)}", "15"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {Short = 15}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.Ushort)}", "15"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {Ushort = 15}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.Int)}", "15"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {Int = 15}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.Long)}", "15"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {Long = 15}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.Float)}", "123.45"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {Float = 123.45f}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.Double)}", "123.45"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {Double = 123.45}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.Decimal)}", "123.45"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {Decimal = 123.45m}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.DateTime)}", "28 Apr 1995"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {DateTime = new DateTime(1995, 04, 28)}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.DateTimeOffset)}", "28 Apr 1995"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {DateTimeOffset = new DateTime(1995, 04, 28)}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.TimeSpan)}", "00:14:59"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {TimeSpan = new TimeSpan(00, 14, 59)}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.TestEnum)}", "value2"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {TestEnum = TestEnum.Value2}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.IntNullable)}", "666"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {IntNullable = 666}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.IntNullable)}"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {IntNullable = null}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.TestEnumNullable)}", "value3"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {TestEnumNullable = TestEnum.Value3}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.TestEnumNullable)}"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {TestEnumNullable = null}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.TimeSpanNullable)}", "01:00:00"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {TimeSpanNullable = new TimeSpan(01, 00, 00)}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.TimeSpanNullable)}"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {TimeSpanNullable = null}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.TestStringConstructable)}", "value"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {TestStringConstructable = new TestStringConstructable("value")}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.TestStringParseable)}", "value"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {TestStringParseable = TestStringParseable.Parse("value")}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.TestStringParseableWithFormatProvider)}", "value"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand
                {
                    TestStringParseableWithFormatProvider =
                        TestStringParseableWithFormatProvider.Parse("value", CultureInfo.InvariantCulture)
                }
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.ObjectArray)}", "value1", "value2"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {ObjectArray = new object[] {"value1", "value2"}}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.StringArray)}", "value1", "value2"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {StringArray = new[] {"value1", "value2"}}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.StringArray)}"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {StringArray = new string[0]}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.IntArray)}", "47", "69"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {IntArray = new[] {47, 69}}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.TestEnumArray)}", "value1", "value3"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {TestEnumArray = new[] {TestEnum.Value1, TestEnum.Value3}}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.IntNullableArray)}", "1337", "2441"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {IntNullableArray = new int?[] {1337, 2441}}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.TestStringConstructableArray)}", "value1", "value2"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand
                {
                    TestStringConstructableArray = new []
                    {
                        new TestStringConstructable("value1"),
                        new TestStringConstructable("value2")
                    }
                }
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.Enumerable)}", "value1", "value3"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {Enumerable = new[] {"value1", "value2"}}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.StringEnumerable)}", "value1", "value3"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {StringEnumerable = new[] {"value1", "value2"}}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.StringReadOnlyList)}", "value1", "value3"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {StringReadOnlyList = new[] {"value1", "value2"}}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.StringList)}", "value1", "value3"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {StringList = new List<string> {"value1", "value2"}}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.StringHashSet)}", "value1", "value3"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {StringHashSet = new HashSet<string> {"value1", "value2"}}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.Ulong)}", "15"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {Ulong = 15}
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.Uint)}", "15"},
                new Dictionary<string, string>(),
                new AllSupportedTypesCommand {Uint = 15}
            );

            yield return new TestCaseData(
                typeof(DivideCommand),
                new[] {"div", "--dividend", "13", "--divisor", "8"},
                new Dictionary<string, string>(),
                new DivideCommand {Dividend = 13, Divisor = 8}
            );

            yield return new TestCaseData(
                typeof(DivideCommand),
                new[] {"div", "-D", "13", "-d", "8"},
                new Dictionary<string, string>(),
                new DivideCommand {Dividend = 13, Divisor = 8}
            );

            yield return new TestCaseData(
                typeof(DivideCommand),
                new[] {"div", "--dividend", "13", "-d", "8"},
                new Dictionary<string, string>(),
                new DivideCommand {Dividend = 13, Divisor = 8}
            );

            yield return new TestCaseData(
                typeof(ConcatCommand),
                new[] {"concat", "-i", "foo", " ", "bar"},
                new Dictionary<string, string>(),
                new ConcatCommand {Inputs = new[] {"foo", " ", "bar"}}
            );

            yield return new TestCaseData(
                typeof(ConcatCommand),
                new[] {"concat", "-i", "foo", "bar", "-s", " "},
                new Dictionary<string, string>(),
                new ConcatCommand { Inputs = new[] { "foo", "bar" }, Separator = " " }
            );

            yield return new TestCaseData(
                typeof(EnvironmentVariableCommand),
                new string[0],
                new Dictionary<string, string>
                {
                    ["ENV_SINGLE_VALUE"] = "A"
                },
                new EnvironmentVariableCommand {Option = "A"}
            );

            yield return new TestCaseData(
                typeof(EnvironmentVariableWithMultipleValuesCommand),
                new string[0],
                new Dictionary<string, string>
                {
                    ["ENV_MULTIPLE_VALUES"] = string.Join(Path.PathSeparator, "A", "B", "C")
                },
                new EnvironmentVariableWithMultipleValuesCommand {Option = new[] {"A", "B", "C"}}
            );

            yield return new TestCaseData(
                typeof(EnvironmentVariableCommand),
                new[] {"--opt", "X"},
                new Dictionary<string, string>
                {
                    ["ENV_SINGLE_VALUE"] = "A"
                },
                new EnvironmentVariableCommand {Option = "X"}
            );

            yield return new TestCaseData(
                typeof(EnvironmentVariableWithoutCollectionPropertyCommand),
                new string[0],
                new Dictionary<string, string>
                {
                    ["ENV_MULTIPLE_VALUES"] = string.Join(Path.PathSeparator, "A", "B", "C")
                },
                new EnvironmentVariableWithoutCollectionPropertyCommand {Option = string.Join(Path.PathSeparator, "A", "B", "C")}
            );

            yield return new TestCaseData(
                typeof(ParameterCommand),
                new[] {"abc", "123", "1", "2", "-o", "option value"},
                new Dictionary<string, string>(),
                new ParameterCommand
                    {FirstArgument = "abc", SecondArgument = 123, ThirdArguments = new List<int> {1, 2}, Option = "option value"}
            );

            // TODO: cover all type conversions
        }

        private static IEnumerable<TestCaseData> GetTestCases_TryInitialize_Negative()
        {
            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.Int)}", "1234.5"},
                new Dictionary<string, string>()
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.Int)}", "123", "456"},
                new Dictionary<string, string>()
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.Int)}"},
                new Dictionary<string, string>()
            );

            yield return new TestCaseData(
                typeof(AllSupportedTypesCommand),
                new[] {$"--{nameof(AllSupportedTypesCommand.NonConvertible)}", "123"},
                new Dictionary<string, string>()
            );

            yield return new TestCaseData(
                typeof(DivideCommand),
                new[] {"div"},
                new Dictionary<string, string>()
            );

            yield return new TestCaseData(
                typeof(DivideCommand),
                new[] {"div", "-D", "13"},
                new Dictionary<string, string>()
            );

            yield return new TestCaseData(
                typeof(DivideCommand),
                new[] {"concat"},
                new Dictionary<string, string>()
            );

            yield return new TestCaseData(
                typeof(DivideCommand),
                new[] {"concat", "-s", "_"},
                new Dictionary<string, string>()
            );

            yield return new TestCaseData(
                typeof(ParameterCommand),
                new[] {"-o", "option value"},
                new Dictionary<string, string>()
            );

            yield return new TestCaseData(
                typeof(ParameterCommand),
                new[] {"abc", "123", "invalid", "-o", "option value"},
                new Dictionary<string, string>()
            );

            yield return new TestCaseData(
                typeof(ParameterCommand),
                new[] {"abc", "123", "unused", "-o", "option value"},
                new Dictionary<string, string>()
            );

            // TODO: env vars
        }

        [TestCaseSource(nameof(GetTestCases_TryInitialize))]
        public void TryInitialize_Test(
            Type commandType,
            IReadOnlyList<string> commandLineArguments,
            IReadOnlyDictionary<string, string> environmentVariables,
            ICommand expectedResult)
        {
            // Arrange
            var commandLineInput = CommandLineInput.Parse(commandLineArguments);
            var applicationSchema = ApplicationSchema.Resolve(commandType);
            var typeActivator = new DefaultTypeActivator();

            // Act
            var command = applicationSchema.TryInitializeCommand(commandLineInput, environmentVariables, typeActivator);

            // Assert
            command.Should().NotBeEquivalentTo(expectedResult);
        }

        [TestCaseSource(nameof(GetTestCases_TryInitialize_Negative))]
        public void TryInitialize_Negative_Test(
            Type commandType,
            IReadOnlyList<string> commandLineArguments,
            IReadOnlyDictionary<string, string> environmentVariables)
        {
            // Arrange
            var commandLineInput = CommandLineInput.Parse(commandLineArguments);
            var applicationSchema = ApplicationSchema.Resolve(commandType);
            var typeActivator = new DefaultTypeActivator();

            // Act
            var command = applicationSchema.TryInitializeCommand(commandLineInput, environmentVariables, typeActivator);

            // Assert
            command.Should().BeNull();
        }
    }
}