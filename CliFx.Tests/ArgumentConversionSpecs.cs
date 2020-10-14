using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CliFx.Tests.Commands;
using CliFx.Tests.Commands.Converters;
using CliFx.Tests.Internal;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class ArgumentConversionSpecs
    {
        private readonly ITestOutputHelper _output;

        public ArgumentConversionSpecs(ITestOutputHelper output) => _output = output;

        [Fact]
        public async Task Property_of_type_object_is_bound_directly_from_the_argument_value()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--obj", "value"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Object = "value"
            });
        }

        [Fact]
        public async Task Property_of_type_object_array_is_bound_directly_from_the_argument_values()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--obj-array", "foo", "bar"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                ObjectArray = new object[] {"foo", "bar"}
            });
        }

        [Fact]
        public async Task Property_of_type_string_is_bound_directly_from_the_argument_value()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--str", "value"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                String = "value"
            });
        }

        [Fact]
        public async Task Property_of_type_string_array_is_bound_directly_from_the_argument_values()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--str-array", "foo", "bar"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                StringArray = new[] {"foo", "bar"}
            });
        }

        [Fact]
        public async Task Property_of_type_string_IEnumerable_is_bound_directly_from_the_argument_values()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--str-enumerable", "foo", "bar"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                StringEnumerable = new[] {"foo", "bar"}
            });
        }

        [Fact]
        public async Task Property_of_type_string_IReadOnlyList_is_bound_directly_from_the_argument_values()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--str-read-only-list", "foo", "bar"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                StringReadOnlyList = new[] {"foo", "bar"}
            });
        }

        [Fact]
        public async Task Property_of_type_string_List_is_bound_directly_from_the_argument_values()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--str-list", "foo", "bar"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                StringList = new List<string> {"foo", "bar"}
            });
        }

        [Fact]
        public async Task Property_of_type_string_HashSet_is_bound_directly_from_the_argument_values()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--str-set", "foo", "bar"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                StringHashSet = new HashSet<string> {"foo", "bar"}
            });
        }

        [Fact]
        public async Task Property_of_type_bool_is_bound_as_true_if_the_argument_value_is_true()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--bool", "true"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Bool = true
            });
        }

        [Fact]
        public async Task Property_of_type_bool_is_bound_as_false_if_the_argument_value_is_false()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--bool", "false"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Bool = false
            });
        }

        [Fact]
        public async Task Property_of_type_bool_is_bound_as_true_if_the_argument_value_is_not_set()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--bool"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Bool = true
            });
        }

        [Fact]
        public async Task Property_of_type_char_is_bound_directly_from_the_argument_value_if_it_contains_only_one_character()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--char", "a"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Char = 'a'
            });
        }

        [Fact]
        public async Task Property_of_type_sbyte_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--sbyte", "15"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Sbyte = 15
            });
        }

        [Fact]
        public async Task Property_of_type_byte_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--byte", "15"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Byte = 15
            });
        }

        [Fact]
        public async Task Property_of_type_short_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--short", "15"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Short = 15
            });
        }

        [Fact]
        public async Task Property_of_type_ushort_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--ushort", "15"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Ushort = 15
            });
        }

        [Fact]
        public async Task Property_of_type_int_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--int", "15"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Int = 15
            });
        }

        [Fact]
        public async Task Property_of_type_nullable_int_is_bound_by_parsing_the_argument_value_if_it_is_set()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--int-nullable", "15"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                IntNullable = 15
            });
        }

        [Fact]
        public async Task Property_of_type_nullable_int_is_bound_as_null_if_the_argument_value_is_not_set()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--int-nullable"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                IntNullable = null
            });
        }

        [Fact]
        public async Task Property_of_type_int_array_is_bound_by_parsing_the_argument_values()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--int-array", "3", "15"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                IntArray = new[] {3, 15}
            });
        }

        [Fact]
        public async Task Property_of_type_nullable_int_array_is_bound_by_parsing_the_argument_values()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--int-nullable-array", "3", "15"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                IntNullableArray = new int?[] {3, 15}
            });
        }

        [Fact]
        public async Task Property_of_type_uint_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--uint", "15"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Uint = 15
            });
        }

        [Fact]
        public async Task Property_of_type_long_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--long", "15"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Long = 15
            });
        }

        [Fact]
        public async Task Property_of_type_ulong_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--ulong", "15"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Ulong = 15
            });
        }

        [Fact]
        public async Task Property_of_type_float_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--float", "3.14"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Float = 3.14f
            });
        }

        [Fact]
        public async Task Property_of_type_double_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--double", "3.14"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Double = 3.14
            });
        }

        [Fact]
        public async Task Property_of_type_decimal_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--decimal", "3.14"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Decimal = 3.14m
            });
        }

        [Fact]
        public async Task Property_of_type_DateTime_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--datetime", "28 Apr 1995"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                DateTime = new DateTime(1995, 04, 28)
            });
        }

        [Fact]
        public async Task Property_of_type_DateTimeOffset_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--datetime-offset", "28 Apr 1995"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                DateTimeOffset = new DateTime(1995, 04, 28)
            });
        }

        [Fact]
        public async Task Property_of_type_TimeSpan_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--timespan", "00:14:59"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                TimeSpan = new TimeSpan(00, 14, 59)
            });
        }

        [Fact]
        public async Task Property_of_type_nullable_TimeSpan_is_bound_by_parsing_the_argument_value_if_it_is_set()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--timespan-nullable", "00:14:59"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                TimeSpanNullable = new TimeSpan(00, 14, 59)
            });
        }

        [Fact]
        public async Task Property_of_type_nullable_TimeSpan_is_bound_as_null_if_the_argument_value_is_not_set()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--timespan-nullable"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                TimeSpanNullable = null
            });
        }

        [Fact]
        public async Task Property_of_an_enum_type_is_bound_by_parsing_the_argument_value_as_name()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--enum", "value2"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Enum = SupportedArgumentTypesCommand.CustomEnum.Value2
            });
        }

        [Fact]
        public async Task Property_of_an_enum_type_is_bound_by_parsing_the_argument_value_as_id()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--enum", "2"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Enum = SupportedArgumentTypesCommand.CustomEnum.Value2
            });
        }

        [Fact]
        public async Task Property_of_a_nullable_enum_type_is_bound_by_parsing_the_argument_value_as_name_if_it_is_set()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--enum-nullable", "value3"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                EnumNullable = SupportedArgumentTypesCommand.CustomEnum.Value3
            });
        }

        [Fact]
        public async Task Property_of_a_nullable_enum_type_is_bound_by_parsing_the_argument_value_as_id_if_it_is_set()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--enum-nullable", "3"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                EnumNullable = SupportedArgumentTypesCommand.CustomEnum.Value3
            });
        }

        [Fact]
        public async Task Property_of_a_nullable_enum_type_is_bound_as_null_if_the_argument_value_is_not_set()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--enum-nullable"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                EnumNullable = null
            });
        }

        [Fact]
        public async Task Property_of_an_enum_array_type_is_bound_by_parsing_the_argument_values_as_names()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--enum-array", "value1", "value3"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                EnumArray = new[] {SupportedArgumentTypesCommand.CustomEnum.Value1, SupportedArgumentTypesCommand.CustomEnum.Value3}
            });
        }

        [Fact]
        public async Task Property_of_an_enum_array_type_is_bound_by_parsing_the_argument_values_as_ids()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--enum-array", "1", "3"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                EnumArray = new[] {SupportedArgumentTypesCommand.CustomEnum.Value1, SupportedArgumentTypesCommand.CustomEnum.Value3}
            });
        }

        [Fact]
        public async Task Property_of_an_enum_array_type_is_bound_by_parsing_the_argument_values_as_either_names_or_ids()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--enum-array", "1", "value3"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                EnumArray = new[] {SupportedArgumentTypesCommand.CustomEnum.Value1, SupportedArgumentTypesCommand.CustomEnum.Value3}
            });
        }

        [Fact]
        public async Task Property_of_a_type_that_has_a_constructor_accepting_a_string_is_bound_by_invoking_the_constructor_with_the_argument_value()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--str-constructible", "foobar"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                StringConstructible = new SupportedArgumentTypesCommand.CustomStringConstructible("foobar")
            });
        }

        [Fact]
        public async Task Property_of_an_array_of_type_that_has_a_constructor_accepting_a_string_is_bound_by_invoking_the_constructor_with_the_argument_values()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--str-constructible-array", "foo", "bar"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

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
        public async Task Property_of_a_type_that_has_a_static_Parse_method_accepting_a_string_is_bound_by_invoking_the_method()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--str-parseable", "foobar"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                StringParseable = SupportedArgumentTypesCommand.CustomStringParseable.Parse("foobar")
            });
        }

        [Fact]
        public async Task Property_of_a_type_that_has_a_static_Parse_method_accepting_a_string_and_format_provider_is_bound_by_invoking_the_method()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--str-parseable-format", "foobar"
            });

            var commandInstance = stdOut.GetString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                StringParseableWithFormatProvider =
                    SupportedArgumentTypesCommand.CustomStringParseableWithFormatProvider.Parse("foobar", CultureInfo.InvariantCulture)
            });
        }

        [Fact]
        public async Task Property_of_custom_type_must_be_string_initializable_in_order_to_be_bound()
        {
            // Arrange
            var (console, _, stdErr) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<UnsupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--str-non-initializable", "foobar"
            });

            // Assert
            exitCode.Should().NotBe(0);
            stdErr.GetString().Should().NotBeNullOrWhiteSpace();

            _output.WriteLine(stdErr.GetString());
        }

        [Fact]
        public async Task Property_of_custom_type_that_implements_IEnumerable_can_only_be_bound_if_that_type_has_a_constructor_accepting_an_array()
        {
            // Arrange
            var (console, _, stdErr) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<UnsupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--str-enumerable-non-initializable", "foobar"
            });

            // Assert
            exitCode.Should().NotBe(0);
            stdErr.GetString().Should().NotBeNullOrWhiteSpace();

            _output.WriteLine(stdErr.GetString());
        }

        [Fact]
        public async Task Property_of_non_nullable_type_can_only_be_bound_if_the_argument_value_is_set()
        {
            // Arrange
            var (console, _, stdErr) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--int"
            });

            // Assert
            exitCode.Should().NotBe(0);
            stdErr.GetString().Should().NotBeNullOrWhiteSpace();

            _output.WriteLine(stdErr.GetString());
        }

        [Fact]
        public async Task Property_must_have_a_type_that_implements_IEnumerable_in_order_to_be_bound_from_multiple_argument_values()
        {
            // Arrange
            var (console, _, stdErr) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--int", "1", "2", "3"
            });

            // Assert
            exitCode.Should().NotBe(0);
            stdErr.GetString().Should().NotBeNullOrWhiteSpace();

            _output.WriteLine(stdErr.GetString());
        }

        [Fact]
        public async Task Property_of_custom_type_is_bound_when_the_valid_converter_type_is_specified()
        {
            // Arrange
            const string foo = "foo";

            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<CommandWithParameterOfCustomType>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--prop", foo
            });

            // Assert
            exitCode.Should().Be(0);

            var commandInstance = stdOut.GetString().DeserializeJson<CommandWithParameterOfCustomType>();

            commandInstance.Should().BeEquivalentTo(new CommandWithParameterOfCustomType()
            {
                MyProperty = (CustomType) new CustomTypeConverter().ConvertFrom(foo)
            });
        }

        [Fact]
        public async Task Enumerable_of_the_custom_type_is_bound_when_the_valid_converter_type_is_specified()
        {
            // Arrange
            string foo = "foo";
            string bar = "bar";

            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<CommandWithEnumerableOfParametersOfCustomType>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--prop", foo, bar
            });

            // Assert
            exitCode.Should().Be(0);

            var commandInstance = stdOut.GetString().DeserializeJson<CommandWithEnumerableOfParametersOfCustomType>();

            commandInstance.Should().BeEquivalentTo(new CommandWithEnumerableOfParametersOfCustomType()
            {
                MyProperties = new List<CustomType>
                {
                    (CustomType) new CustomTypeConverter().ConvertFrom(foo),
                    (CustomType) new CustomTypeConverter().ConvertFrom(bar)
                }
            });
        }
    }
}