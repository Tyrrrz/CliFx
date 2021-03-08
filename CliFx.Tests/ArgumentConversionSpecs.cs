using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using CliFx.Tests.Commands;
using CliFx.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class ArgumentConversionSpecs : IDisposable
    {
        private readonly ITestOutputHelper _testOutput;
        private readonly FakeInMemoryConsole _console = new();

        public ArgumentConversionSpecs(ITestOutputHelper testOutput) =>
            _testOutput = testOutput;

        public void Dispose()
        {
            _console.DumpToTestOutput(_testOutput);
            _console.Dispose();
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_object()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--obj", "value"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Object = "value"
            });
        }

        [Fact]
        public async Task Argument_values_can_be_bound_to_array_of_object()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--obj-array", "foo", "bar"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                ObjectArray = new object[] {"foo", "bar"}
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_string()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--str", "value"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                String = "value"
            });
        }

        [Fact]
        public async Task Argument_values_can_be_bound_to_array_of_string()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--str-array", "foo", "bar"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                StringArray = new[] {"foo", "bar"}
            });
        }

        [Fact]
        public async Task Argument_values_can_be_bound_to_IEnumerable_of_string()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--str-enumerable", "foo", "bar"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                StringEnumerable = new[] {"foo", "bar"}
            });
        }

        [Fact]
        public async Task Argument_values_can_be_bound_to_IReadOnlyList_of_string()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--str-read-only-list", "foo", "bar"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                StringReadOnlyList = new[] {"foo", "bar"}
            });
        }

        [Fact]
        public async Task Argument_values_can_be_bound_to_List_of_string()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--str-list", "foo", "bar"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                StringList = new List<string> {"foo", "bar"}
            });
        }

        [Fact]
        public async Task Argument_values_can_be_bound_to_HashSet_of_string()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--str-set", "foo", "bar"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                StringHashSet = new HashSet<string> {"foo", "bar"}
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_boolean_as_true_if_the_value_is_true()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--bool", "true"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Bool = true
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_boolean_as_false_if_the_value_is_false()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--bool", "false"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Bool = false
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_boolean_as_true_if_the_value_is_not_set()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--bool"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Bool = true
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_char_if_the_value_contains_a_single_character()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--char", "a"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Char = 'a'
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_sbyte()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--sbyte", "15"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Sbyte = 15
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_byte()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--byte", "15"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Byte = 15
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_short()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--short", "15"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Short = 15
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_ushort()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--ushort", "15"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Ushort = 15
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_int()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--int", "15"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Int = 15
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_nullable_of_int_as_actual_value_if_it_is_set()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--int-nullable", "15"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                IntNullable = 15
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_nullable_of_int_as_null_if_it_is_not_set()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--int-nullable"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                IntNullable = null
            });
        }

        [Fact]
        public async Task Argument_values_can_be_bound_to_array_of_int()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--int-array", "3", "15"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                IntArray = new[] {3, 15}
            });
        }

        [Fact]
        public async Task Argument_values_can_be_bound_to_array_of_nullable_of_int()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--int-nullable-array", "3", "15"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                IntNullableArray = new int?[] {3, 15}
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_uint()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--uint", "15"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Uint = 15
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_long()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--long", "15"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Long = 15
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_ulong()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--ulong", "15"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Ulong = 15
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_float()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--float", "3.14"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Float = 3.14f
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_double()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--double", "3.14"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Double = 3.14
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_decimal()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--decimal", "3.14"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Decimal = 3.14m
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_DateTime()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--datetime", "28 Apr 1995"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                DateTime = new DateTime(1995, 04, 28)
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_DateTimeOffset()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--datetime-offset", "28 Apr 1995"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                DateTimeOffset = new DateTime(1995, 04, 28)
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_TimeSpan()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--timespan", "00:14:59"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                TimeSpan = new TimeSpan(00, 14, 59)
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_nullable_of_TimeSpan_as_actual_value_if_it_is_set()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--timespan-nullable", "00:14:59"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                TimeSpanNullable = new TimeSpan(00, 14, 59)
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_nullable_of_TimeSpan_as_null_if_it_is_not_set()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--timespan-nullable"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                TimeSpanNullable = null
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_enum_type_by_name()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--enum", "value2"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Enum = SupportedArgumentTypesCommand.CustomEnum.Value2
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_enum_type_by_id()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--enum", "2"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Enum = SupportedArgumentTypesCommand.CustomEnum.Value2
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_nullable_of_enum_type_by_name_if_it_is_set()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--enum-nullable", "value3"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                EnumNullable = SupportedArgumentTypesCommand.CustomEnum.Value3
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_nullable_of_enum_type_by_id_if_it_is_set()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--enum-nullable", "3"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                EnumNullable = SupportedArgumentTypesCommand.CustomEnum.Value3
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_nullable_of_enum_type_as_null_if_it_is_not_set()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--enum-nullable"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                EnumNullable = null
            });
        }

        [Fact]
        public async Task Argument_values_can_be_bound_to_array_of_enum_type_by_names()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--enum-array", "value1", "value3"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                EnumArray = new[] {SupportedArgumentTypesCommand.CustomEnum.Value1, SupportedArgumentTypesCommand.CustomEnum.Value3}
            });
        }

        [Fact]
        public async Task Argument_values_can_be_bound_to_array_of_enum_type_by_ids()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--enum-array", "1", "3"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                EnumArray = new[] {SupportedArgumentTypesCommand.CustomEnum.Value1, SupportedArgumentTypesCommand.CustomEnum.Value3}
            });
        }

        [Fact]
        public async Task Argument_values_can_be_bound_to_array_of_enum_type_by_either_names_or_ids()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--enum-array", "1", "value3"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                EnumArray = new[] {SupportedArgumentTypesCommand.CustomEnum.Value1, SupportedArgumentTypesCommand.CustomEnum.Value3}
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_a_custom_type_if_it_has_a_constructor_accepting_a_string()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--str-constructible", "foobar"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                StringConstructible = new SupportedArgumentTypesCommand.CustomStringConstructible("foobar")
            });
        }

        [Fact]
        public async Task Argument_values_can_be_bound_to_array_of_custom_type_if_it_has_a_constructor_accepting_a_string()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--str-constructible-array", "foo", "bar"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                StringConstructibleArray = new[]
                {
                    new SupportedArgumentTypesCommand.CustomStringConstructible("foo"),
                    new SupportedArgumentTypesCommand.CustomStringConstructible("bar")
                }
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_a_custom_type_if_it_has_a_static_Parse_method()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--str-parseable", "foobar"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                StringParseable = SupportedArgumentTypesCommand.CustomStringParseable.Parse("foobar")
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_a_custom_type_if_it_has_a_static_Parse_method_with_format_provider()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--str-parseable-format", "foobar"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                StringParseableWithFormatProvider =
                    SupportedArgumentTypesCommand.CustomStringParseableWithFormatProvider.Parse("foobar", CultureInfo.InvariantCulture)
            });
        }

        [Fact]
        public async Task Argument_value_can_be_bound_to_a_custom_type_if_a_converter_has_been_specified()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--convertible", "13"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Convertible =
                    (SupportedArgumentTypesCommand.CustomConvertible)
                    new SupportedArgumentTypesCommand.CustomConvertibleConverter().ConvertFrom("13")
            });
        }

        [Fact]
        public async Task Argument_values_can_be_bound_to_array_of_custom_type_if_a_converter_has_been_specified()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--convertible-array", "13", "42"
            });

            var commandInstance = _console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                ConvertibleArray = new[]
                {
                    (SupportedArgumentTypesCommand.CustomConvertible)
                    new SupportedArgumentTypesCommand.CustomConvertibleConverter().ConvertFrom("13"),

                    (SupportedArgumentTypesCommand.CustomConvertible)
                    new SupportedArgumentTypesCommand.CustomConvertibleConverter().ConvertFrom("42")
                }
            });
        }

        [Fact]
        public async Task Argument_value_can_only_be_bound_if_the_target_type_is_supported()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<UnsupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--custom"
            });

            var stdErr = _console.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdErr.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Argument_value_can_only_be_bound_if_the_provided_value_can_be_converted_to_the_target_type()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--int", "foo"
            });

            var stdErr = _console.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdErr.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Argument_value_can_only_be_bound_to_non_nullable_type_if_it_is_set()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--int"
            });

            var stdErr = _console.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdErr.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Argument_values_can_only_be_bound_to_a_type_that_implements_IEnumerable()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--int", "1", "2", "3"
            });

            var stdErr = _console.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdErr.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Argument_values_can_only_be_bound_to_a_type_that_implements_IEnumerable_and_can_be_converted_from_an_array()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<UnsupportedArgumentTypesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--custom-enumerable"
            });

            var stdErr = _console.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdErr.Should().NotBeNullOrWhiteSpace();
        }
    }
}