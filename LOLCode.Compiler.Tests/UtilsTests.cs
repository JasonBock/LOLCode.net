using NUnit.Framework;
using System;

namespace LOLCode.Compiler.Tests
{
	public static class UtilsTests
	{
		private sealed class ToStringTest
		{
			public override string ToString() => "42";
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
		public static void ToBoolWithTrueValue() =>
			Assert.That(Utils.ToBool(Guid.NewGuid()), Is.EqualTo(true));
	}
}