using System;
using System.Collections.Generic;
using System.Globalization;
using CliFx.Domain;
using CliFx.Exceptions;
using FluentAssertions;
using Xunit;

namespace CliFx.Tests
{
    public partial class ArgumentBindingSpecs
    {
        [Fact]
        public void Property_of_type_object_is_bound_directly_from_the_argument_value()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.Object), "value")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                Object = "value"
            });
        }

        [Fact]
        public void Property_of_type_object_array_is_bound_directly_from_the_argument_values()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.ObjectArray), "foo", "bar")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                ObjectArray = new object[] {"foo", "bar"}
            });
        }

        [Fact]
        public void Property_of_type_non_generic_IEnumerable_is_bound_directly_from_the_argument_values()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.Enumerable), "foo", "bar")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                Enumerable = new object[] {"foo", "bar"}
            });
        }

        [Fact]
        public void Property_of_type_string_is_bound_directly_from_the_argument_value()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.String), "value")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                String = "value"
            });
        }

        [Fact]
        public void Property_of_type_string_array_is_bound_directly_from_the_argument_values()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.StringArray), "foo", "bar")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                StringArray = new[] {"foo", "bar"}
            });
        }

        [Fact]
        public void Property_of_type_string_IEnumerable_is_bound_directly_from_the_argument_values()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.StringEnumerable), "foo", "bar")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                StringEnumerable = new[] {"foo", "bar"}
            });
        }

        [Fact]
        public void Property_of_type_string_IReadOnlyList_is_bound_directly_from_the_argument_values()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.StringReadOnlyList), "foo", "bar")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                StringReadOnlyList = new[] {"foo", "bar"}
            });
        }

        [Fact]
        public void Property_of_type_string_List_is_bound_directly_from_the_argument_values()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.StringList), "foo", "bar")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                StringList = new List<string> {"foo", "bar"}
            });
        }

        [Fact]
        public void Property_of_type_string_HashSet_is_bound_directly_from_the_argument_values()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.StringHashSet), "foo", "bar")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                StringHashSet = new HashSet<string>(new[] {"foo", "bar"})
            });
        }

        [Fact]
        public void Property_of_type_bool_is_bound_as_true_if_the_argument_value_is_true()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.Bool), "true")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                Bool = true
            });
        }

        [Fact]
        public void Property_of_type_bool_is_bound_as_false_if_the_argument_value_is_false()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.Bool), "false")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                Bool = false
            });
        }

        [Fact]
        public void Property_of_type_bool_is_bound_as_true_if_the_argument_value_is_not_set()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.Bool))
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                Bool = true
            });
        }

        [Fact]
        public void Property_of_type_char_is_bound_directly_from_the_argument_value_if_it_contains_only_one_character()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.Char), "a")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                Char = 'a'
            });
        }

        [Fact]
        public void Property_of_type_sbyte_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.Sbyte), "15")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                Sbyte = 15
            });
        }

        [Fact]
        public void Property_of_type_byte_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.Byte), "15")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                Byte = 15
            });
        }

        [Fact]
        public void Property_of_type_short_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.Short), "15")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                Short = 15
            });
        }

        [Fact]
        public void Property_of_type_ushort_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.Ushort), "15")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                Ushort = 15
            });
        }

        [Fact]
        public void Property_of_type_int_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.Int), "15")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                Int = 15
            });
        }

        [Fact]
        public void Property_of_type_nullable_int_is_bound_by_parsing_the_argument_value_if_it_is_set()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.IntNullable), "15")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                IntNullable = 15
            });
        }

        [Fact]
        public void Property_of_type_nullable_int_is_bound_as_null_if_the_argument_value_is_not_set()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.IntNullable))
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                IntNullable = null
            });
        }

        [Fact]
        public void Property_of_type_int_array_is_bound_by_parsing_the_argument_values()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.IntArray), "3", "14")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                IntArray = new[] {3, 14}
            });
        }

        [Fact]
        public void Property_of_type_nullable_int_array_is_bound_by_parsing_the_argument_values()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.IntNullableArray), "3", "14")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                IntNullableArray = new int?[] {3, 14}
            });
        }

        [Fact]
        public void Property_of_type_uint_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.Uint), "15")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                Uint = 15
            });
        }

        [Fact]
        public void Property_of_type_long_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.Long), "15")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                Long = 15
            });
        }

        [Fact]
        public void Property_of_type_ulong_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.Ulong), "15")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                Ulong = 15
            });
        }

        [Fact]
        public void Property_of_type_float_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.Float), "123.45")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                Float = 123.45F
            });
        }

        [Fact]
        public void Property_of_type_double_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.Double), "123.45")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                Double = 123.45
            });
        }

        [Fact]
        public void Property_of_type_decimal_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.Decimal), "123.45")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                Decimal = 123.45M
            });
        }

        [Fact]
        public void Property_of_type_DateTime_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.DateTime), "28 Apr 1995")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                DateTime = new DateTime(1995, 04, 28)
            });
        }

        [Fact]
        public void Property_of_type_DateTimeOffset_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.DateTimeOffset), "28 Apr 1995")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                DateTimeOffset = new DateTime(1995, 04, 28)
            });
        }

        [Fact]
        public void Property_of_type_TimeSpan_is_bound_by_parsing_the_argument_value()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.TimeSpan), "00:14:59")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                TimeSpan = new TimeSpan(00, 14, 59)
            });
        }

        [Fact]
        public void Property_of_type_nullable_TimeSpan_is_bound_by_parsing_the_argument_value_if_it_is_set()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.TimeSpanNullable), "00:14:59")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                TimeSpanNullable = new TimeSpan(00, 14, 59)
            });
        }

        [Fact]
        public void Property_of_type_nullable_TimeSpan_is_bound_as_null_if_the_argument_value_is_not_set()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.TimeSpanNullable))
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                TimeSpanNullable = null
            });
        }

        [Fact]
        public void Property_of_an_enum_type_is_bound_by_parsing_the_argument_value_as_name()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.CustomEnum), "value2")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                CustomEnum = CustomEnum.Value2
            });
        }

        [Fact]
        public void Property_of_an_enum_type_is_bound_by_parsing_the_argument_value_as_id()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.CustomEnum), "2")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                CustomEnum = CustomEnum.Value2
            });
        }

        [Fact]
        public void Property_of_a_nullable_enum_type_is_bound_by_parsing_the_argument_value_as_name_if_it_is_set()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.CustomEnumNullable), "value3")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                CustomEnumNullable = CustomEnum.Value3
            });
        }

        [Fact]
        public void Property_of_a_nullable_enum_type_is_bound_by_parsing_the_argument_value_as_id_if_it_is_set()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.CustomEnumNullable), "3")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                CustomEnumNullable = CustomEnum.Value3
            });
        }

        [Fact]
        public void Property_of_a_nullable_enum_type_is_bound_as_null_if_the_argument_value_is_not_set()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.CustomEnumNullable))
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                CustomEnumNullable = null
            });
        }

        [Fact]
        public void Property_of_an_enum_array_type_is_bound_by_parsing_the_argument_values_as_names()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.CustomEnumArray), "value1", "value3")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                CustomEnumArray = new[] {CustomEnum.Value1, CustomEnum.Value3}
            });
        }

        [Fact]
        public void Property_of_an_enum_array_type_is_bound_by_parsing_the_argument_values_as_ids()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.CustomEnumArray), "1", "3")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                CustomEnumArray = new[] {CustomEnum.Value1, CustomEnum.Value3}
            });
        }

        [Fact]
        public void Property_of_an_enum_array_type_is_bound_by_parsing_the_argument_values_as_either_names_or_ids()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.CustomEnumArray), "value1", "3")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                CustomEnumArray = new[] {CustomEnum.Value1, CustomEnum.Value3}
            });
        }

        [Fact]
        public void Property_of_a_type_that_has_a_constructor_accepting_a_string_is_bound_by_invoking_the_constructor_with_the_argument_value()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.TestStringConstructable), "foobar")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                TestStringConstructable = new StringConstructable("foobar")
            });
        }

        [Fact]
        public void Property_of_an_array_of_type_that_has_a_constructor_accepting_a_string_is_bound_by_invoking_the_constructor_with_the_argument_values()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.TestStringConstructableArray), "foo", "bar")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                TestStringConstructableArray = new[] {new StringConstructable("foo"), new StringConstructable("bar") }
            });
        }

        [Fact]
        public void Property_of_a_type_that_has_a_static_Parse_method_accepting_a_string_is_bound_by_invoking_the_method()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.TestStringParseable), "foobar")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                TestStringParseable = StringParseable.Parse("foobar")
            });
        }

        [Fact]
        public void Property_of_a_type_that_has_a_static_Parse_method_accepting_a_string_and_format_provider_is_bound_by_invoking_the_method()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.TestStringParseableWithFormatProvider), "foobar")
                .Build();

            // Act
            var command = schema.InitializeEntryPoint(input);

            // Assert
            command.Should().BeEquivalentTo(new AllSupportedTypesCommand
            {
                TestStringParseableWithFormatProvider = StringParseableWithFormatProvider.Parse("foobar", CultureInfo.InvariantCulture)
            });
        }

        [Fact]
        public void Property_of_custom_type_that_implements_IEnumerable_can_only_be_bound_if_that_type_has_a_constructor_accepting_an_array()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(CustomEnumerableCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(CustomEnumerableCommand.Option), "foo", "bar")
                .Build();

            // Act & assert
            Assert.Throws<CliFxException>(() => schema.InitializeEntryPoint(input));
        }

        [Fact]
        public void Property_of_non_nullable_type_can_only_be_bound_if_the_argument_value_is_set()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.Int))
                .Build();

            // Act & assert
            Assert.Throws<CliFxException>(() => schema.InitializeEntryPoint(input));
        }

        [Fact]
        public void Property_must_have_a_type_supported_by_the_framework_in_order_to_be_bound()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(NonStringParseableCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(NonStringParseableCommand.Option), "foo", "bar")
                .Build();

            // Act & assert
            Assert.Throws<CliFxException>(() => schema.InitializeEntryPoint(input));
        }

        [Fact]
        public void Property_must_have_a_type_that_implements_IEnumerable_in_order_to_be_bound_from_multiple_argument_values()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(AllSupportedTypesCommand)});

            var input = new CommandLineInputBuilder()
                .AddOption(nameof(AllSupportedTypesCommand.Int), "1", "2", "3")
                .Build();

            // Act & assert
            Assert.Throws<CliFxException>(() => schema.InitializeEntryPoint(input));
        }
    }
}