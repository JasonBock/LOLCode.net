using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace LOLCode.Compiler.Tests
{
	public static class UtilsTests
	{
		private sealed class ToStringTest
		{
			public override string ToString() => "42";
		}

		[Test]
		public static void ReadWordWithNoContent()
		{
			using (var reader = new StringReader(string.Empty))
			{
				Assert.That(Utils.ReadWord(reader), Is.EqualTo(string.Empty));
			}
		}

		[Test]
		public static void ReadWordWithOnlyWhitespace()
		{
			using (var reader = new StringReader(" "))
			{
				Assert.That(Utils.ReadWord(reader), Is.EqualTo(string.Empty));
			}
		}

		[Test]
		public static void ReadWordWithContent()
		{
			using (var reader = new StringReader("abc"))
			{
				Assert.That(Utils.ReadWord(reader), Is.EqualTo("979899"));
			}
		}

		[Test]
		public static void ToStringWithNull() =>
			Assert.That(() => Utils.ToString(null), Throws.TypeOf<InvalidCastException>());

		[Test]
		public static void ToStringWithString()
		{
			var value = "CHEEZBURGER";
			var expectedValue = "CHEEZBURGER";
			Assert.That(Utils.ToString(value), Is.EqualTo(expectedValue));
		}

		[Test]
		public static void ToStringWithObject()
		{
			var value = new ToStringTest() as object;
			var expectedValue = "42";
			Assert.That(Utils.ToString(value), Is.EqualTo(expectedValue));
		}

		[Test]
		public static void ToIntWithInt()
		{
			var value = 1337;
			var expectedValue = 1337;
			Assert.That(Utils.ToInt(value), Is.EqualTo(expectedValue));
		}

		[Test]
		public static void ToIntWithFloat()
		{
			var value = 1337.123f;
			var expectedValue = 1337;
			Assert.That(Utils.ToInt(value), Is.EqualTo(expectedValue));
		}

		[Test]
		public static void ToIntWithTrueBool()
		{
			var value = true;
			var expectedValue = 1;
			Assert.That(Utils.ToInt(value), Is.EqualTo(expectedValue));
		}

		[Test]
		public static void ToIntWithFalseBool()
		{
			var value = false;
			var expectedValue = 0;
			Assert.That(Utils.ToInt(value), Is.EqualTo(expectedValue));
		}

		[Test]
		public static void ToIntWithParseableString()
		{
			var value = "123";
			var expectedValue = 123;
			Assert.That(Utils.ToInt(value), Is.EqualTo(expectedValue));
		}

		[Test]
		public static void ToIntWithUnparseableString() =>
			Assert.That(() => Utils.ToInt("quux"),
				Throws.TypeOf<InvalidCastException>().And.Message.EqualTo("Cannot cast non-numeric YARN to NUMBR"));

		[Test]
		public static void ToIntWithUnsupportedType() =>
			Assert.That(() => Utils.ToInt(Guid.NewGuid()),
				Throws.TypeOf<InvalidCastException>().And.Message.EqualTo($"Cannot cast type \"{typeof(Guid).Name}\" to NUMBR"));

		[Test]
		public static void ToBoolWithNull() =>
			Assert.That(Utils.ToBool(null), Is.EqualTo(false));

		[Test]
		public static void ToBoolWithFalseInt() =>
			Assert.That(Utils.ToBool(0), Is.EqualTo(false));

		[Test]
		public static void ToBoolWithFalseFloat() =>
			Assert.That(Utils.ToBool(0f), Is.EqualTo(false));

		[Test]
		public static void ToBoolWithFalseString() =>
			Assert.That(Utils.ToBool(string.Empty), Is.EqualTo(false));

		[Test]
		public static void ToBoolWithFalseBool() =>
			Assert.That(Utils.ToBool(false), Is.EqualTo(false));

		[Test]
		public static void ToBoolWithTrue() =>
			Assert.That(Utils.ToBool(Guid.NewGuid()), Is.EqualTo(true));

		[Test]
		public static void ToFloatWithInt() =>
			Assert.That(Utils.ToFloat(1), Is.EqualTo(1f));

		[Test]
		public static void ToFloatWithFloat() =>
			Assert.That(Utils.ToFloat(1.1f), Is.EqualTo(1.1f));

		[Test]
		public static void ToFloatWithTrueBool() =>
			Assert.That(Utils.ToFloat(true), Is.EqualTo(1f));

		[Test]
		public static void ToFloatWithFalseBool() =>
			Assert.That(Utils.ToFloat(false), Is.EqualTo(0f));

		[Test]
		public static void ToFloatWithParseableString()
		{
			var value = "1.1";
			var expectedValue = 1.1f;
			Assert.That(Utils.ToFloat(value), Is.EqualTo(expectedValue));
		}

		[Test]
		public static void ToFloatWithUnparseableString() =>
			Assert.That(() => Utils.ToFloat("quux"),
				Throws.TypeOf<InvalidCastException>().And.Message.EqualTo("Cannot cast non-numeric YARN to NUMBAR"));

		[Test]
		public static void ToFloatWithUnsupportedType() =>
			Assert.That(() => Utils.ToFloat(Guid.NewGuid()),
				Throws.TypeOf<InvalidCastException>().And.Message.EqualTo($"Cannot cast type \"{typeof(Guid).Name}\" to NUMBAR"));

		[Test]
		public static void PrintObjectWithNonDictionaryAndNoNewLine()
		{
			using (var writer = new StringWriter())
			{
				Utils.PrintObject(writer, "a", false);
				var builder = writer.GetStringBuilder();
				Assert.That(builder.ToString(), Is.EqualTo("a"));
			}
		}

		[Test]
		public static void PrintObjectWithNonDictionaryAndNewLine()
		{
			using (var writer = new StringWriter())
			{
				Utils.PrintObject(writer, "a", true);
				var builder = writer.GetStringBuilder();
				Assert.That(builder.ToString(), Is.EqualTo($"a{Environment.NewLine}"));
			}
		}

		[Test]
		public static void PrintObjectWithDictionaryAndNoNewLine()
		{
			using (var writer = new StringWriter())
			{
				Utils.PrintObject(writer, new Dictionary<object, object> { { 1, "a" }, { 2, "b" } }, false);
				var builder = writer.GetStringBuilder();
				Assert.That(builder.ToString(), Is.EqualTo("ab"));
			}
		}

		[Test]
		public static void PrintObjectWithDictionaryAndNewLine()
		{
			using (var writer = new StringWriter())
			{
				Utils.PrintObject(writer, new Dictionary<object, object> { { 1, "a" }, { 2, "b" } }, true);
				var builder = writer.GetStringBuilder();
				Assert.That(builder.ToString(), Is.EqualTo($"a{Environment.NewLine}b{Environment.NewLine}"));
			}
		}
	}
}