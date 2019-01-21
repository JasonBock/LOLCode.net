using NUnit.Framework;
using System;

namespace LOLCode.Compiler.Tests
{
	public static class CoreTests
	{
		[Test]
		public static void CallSUMWithTwoStringInts() =>
			Assert.That(Core.SUM("1", "2"), Is.EqualTo(3));

		[Test]
		public static void CallSUMWithTwoStringFloats() =>
			Assert.That(Core.SUM("1.1", "2.2"), Is.EqualTo(1.1f + 2.2f));

		[Test]
		public static void CallSUMWithTwoInts() =>
			Assert.That(Core.SUM(1, 2), Is.EqualTo(3));

		[Test]
		public static void CallSUMWithTwoFloats() =>
			Assert.That(Core.SUM(1.1f, 2.2f), Is.EqualTo(1.1f + 2.2f));

		[Test]
		public static void CallSUMWithIntAndFloat() =>
			Assert.That(Core.SUM(1, 2.2f), Is.EqualTo(1 + 2.2f));

		[Test]
		public static void CallSUMWithFloatAndInt() =>
			Assert.That(Core.SUM(2.2f, 1), Is.EqualTo(2.2f + 1));

		[Test]
		public static void CallSUMWithUnsupportedTypes() =>
			Assert.That(() => Core.SUM(Guid.NewGuid(), Guid.NewGuid()),
				Throws.TypeOf<InvalidOperationException>().And.Message.EqualTo($"Cannot add types \"{typeof(Guid)}\" and \"{typeof(Guid)}\""));

		[Test]
		public static void CallDIFFWithTwoStringInts() =>
			Assert.That(Core.DIFF("2", "1"), Is.EqualTo(1));

		[Test]
		public static void CallDIFFWithTwoStringFloats() =>
			Assert.That(Core.DIFF("2.2", "1.1"), Is.EqualTo(2.2f - 1.1f));

		[Test]
		public static void CallDIFFWithTwoInts() =>
			Assert.That(Core.DIFF(2, 1), Is.EqualTo(1));

		[Test]
		public static void CallDIFFWithTwoFloats() =>
			Assert.That(Core.DIFF(2.2f, 1.1f), Is.EqualTo(2.2f - 1.1f));

		[Test]
		public static void CallDIFFWithIntAndFloat() =>
			Assert.That(Core.DIFF(2, 1.1f), Is.EqualTo(2 - 1.1f));

		[Test]
		public static void CallDIFFWithFloatAndInt() =>
			Assert.That(Core.DIFF(2.2f, 1), Is.EqualTo(2.2f - 1));

		[Test]
		public static void CallDIFFWithUnsupportedTypes() =>
			Assert.That(() => Core.DIFF(Guid.NewGuid(), Guid.NewGuid()),
				Throws.TypeOf<InvalidOperationException>().And.Message.EqualTo($"Cannot add types \"{typeof(Guid)}\" and \"{typeof(Guid)}\""));

		[Test]
		public static void CallPRODUKTWithTwoStringInts() =>
			Assert.That(Core.PRODUKT("3", "2"), Is.EqualTo(6));

		[Test]
		public static void CallPRODUKTWithTwoStringFloats() =>
			Assert.That(Core.PRODUKT("3.3", "2.2"), Is.EqualTo(3.3f * 2.2f));

		[Test]
		public static void CallPRODUKTWithTwoInts() =>
			Assert.That(Core.PRODUKT(3, 2), Is.EqualTo(6));

		[Test]
		public static void CallPRODUKTWithTwoFloats() =>
			Assert.That(Core.PRODUKT(3.3f, 2.2f), Is.EqualTo(3.3f * 2.2f));

		[Test]
		public static void CallPRODUKTWithIntAndFloat() =>
			Assert.That(Core.PRODUKT(3, 2.2f), Is.EqualTo(3 * 2.2f));

		[Test]
		public static void CallPRODUKTWithFloatAndInt() =>
			Assert.That(Core.PRODUKT(2.2f, 3), Is.EqualTo(2.2f * 3));

		[Test]
		public static void CallPRODUKTWithUnsupportedTypes() =>
			Assert.That(() => Core.PRODUKT(Guid.NewGuid(), Guid.NewGuid()),
				Throws.TypeOf<InvalidOperationException>().And.Message.EqualTo($"Cannot add types \"{typeof(Guid)}\" and \"{typeof(Guid)}\""));

		[Test]
		public static void CallQUOSHUNTWithTwoStringInts() =>
			Assert.That(Core.QUOSHUNT("6", "2"), Is.EqualTo(3));

		[Test]
		public static void CallQUOSHUNTWithTwoStringFloats() =>
			Assert.That(Core.QUOSHUNT("6.6", "2.2"), Is.EqualTo(6.6f / 2.2f));

		[Test]
		public static void CallQUOSHUNTWithTwoInts() =>
			Assert.That(Core.QUOSHUNT(6, 2), Is.EqualTo(3));

		[Test]
		public static void CallQUOSHUNTWithTwoFloats() =>
			Assert.That(Core.QUOSHUNT(6.6f, 2.2f), Is.EqualTo(6.6f / 2.2f));

		[Test]
		public static void CallQUOSHUNTWithIntAndFloat() =>
			Assert.That(Core.QUOSHUNT(6, 2.2f), Is.EqualTo(6 / 2.2f));

		[Test]
		public static void CallQUOSHUNTWithFloatAndInt() =>
			Assert.That(Core.QUOSHUNT(6.6f, 2), Is.EqualTo(6.6f / 2));

		[Test]
		public static void CallQUOSHUNTWithUnsupportedTypes() =>
			Assert.That(() => Core.QUOSHUNT(Guid.NewGuid(), Guid.NewGuid()),
				Throws.TypeOf<InvalidOperationException>().And.Message.EqualTo($"Cannot add types \"{typeof(Guid)}\" and \"{typeof(Guid)}\""));

		[Test]
		public static void CallMODWithTwoStringInts() =>
			Assert.That(Core.MOD("6", "2"), Is.EqualTo(0));

		[Test]
		public static void CallMODWithTwoStringFloats() =>
			Assert.That(Core.MOD("6.6", "2.2"), Is.EqualTo(6.6f % 2.2f));

		[Test]
		public static void CallMODWithTwoInts() =>
			Assert.That(Core.MOD(6, 2), Is.EqualTo(0));

		[Test]
		public static void CallMODWithTwoFloats() =>
			Assert.That(Core.MOD(6.6f, 2.2f), Is.EqualTo(6.6f % 2.2f));

		[Test]
		public static void CallMODWithIntAndFloat() =>
			Assert.That(Core.MOD(6, 2.2f), Is.EqualTo(6 % 2.2f));

		[Test]
		public static void CallMODWithFloatAndInt() =>
			Assert.That(Core.MOD(6.6f, 2), Is.EqualTo(6.6f % 2));

		[Test]
		public static void CallMODWithUnsupportedTypes() =>
			Assert.That(() => Core.MOD(Guid.NewGuid(), Guid.NewGuid()),
				Throws.TypeOf<InvalidOperationException>().And.Message.EqualTo($"Cannot add types \"{typeof(Guid)}\" and \"{typeof(Guid)}\""));

		[Test]
		public static void CallBIGGRWithTwoStringFirstIsBigger() =>
			Assert.That(Core.BIGGR("ac", "ab"), Is.EqualTo("ac"));

		[Test]
		public static void CallBIGGRWithTwoStringSecondIsBigger() =>
			Assert.That(Core.BIGGR("ab", "ac"), Is.EqualTo("ac"));

		[Test]
		public static void CallBIGGRWithTwoIntsFirstIsBigger() =>
			Assert.That(Core.BIGGR(6, 2), Is.EqualTo(6));

		[Test]
		public static void CallBIGGRWithTwoIntsSecondIsBigger() =>
			Assert.That(Core.BIGGR(2, 6), Is.EqualTo(6));

		[Test]
		public static void CallBIGGRWithTwoFloatsFirstIsBigger() =>
			Assert.That(Core.BIGGR(6.6f, 2.2f), Is.EqualTo(6.6f));

		[Test]
		public static void CallBIGGRWithTwoFloatsSecondIsBigger() =>
			Assert.That(Core.BIGGR(2.2f, 6.6f), Is.EqualTo(6.6f));

		[Test]
		public static void CallBIGGRWithIntAndFloatFirstIsBigger() =>
			Assert.That(Core.BIGGR(6, 2.2f), Is.EqualTo(6));

		[Test]
		public static void CallBIGGRWithIntAndFloatSecondIsBigger() =>
			Assert.That(Core.BIGGR(2, 6.6f), Is.EqualTo(6.6f));

		[Test]
		public static void CallBIGGRWithFloatAndIntFirstIsBigger() =>
			Assert.That(Core.BIGGR(6.6f, 2), Is.EqualTo(6.6f));

		[Test]
		public static void CallBIGGRWithFloatAndIntSecondIsBigger() =>
			Assert.That(Core.BIGGR(2.2f, 6), Is.EqualTo(6));

		[Test]
		public static void CallBIGGRWithUnsupportedTypes() =>
			Assert.That(() => Core.BIGGR(Guid.NewGuid(), Guid.NewGuid()),
				Throws.TypeOf<InvalidOperationException>().And.Message.EqualTo($"Cannot add types \"{typeof(Guid)}\" and \"{typeof(Guid)}\""));

		[Test]
		public static void CallSMALLRWithTwoStringFirstIsSmaller() =>
			Assert.That(Core.SMALLR("ab", "ac"), Is.EqualTo("ab"));

		[Test]
		public static void CallSMALLRWithTwoStringSecondIsSmaller() =>
			Assert.That(Core.SMALLR("ac", "ab"), Is.EqualTo("ab"));

		[Test]
		public static void CallSMALLRWithTwoIntsFirstIsSmaller() =>
			Assert.That(Core.SMALLR(2, 6), Is.EqualTo(2));

		[Test]
		public static void CallSMALLRWithTwoIntsSecondIsSmaller() =>
			Assert.That(Core.SMALLR(6, 2), Is.EqualTo(2));

		[Test]
		public static void CallSMALLRWithTwoFloatsFirstIsSmaller() =>
			Assert.That(Core.SMALLR(2.2f, 6.6f), Is.EqualTo(2.2f));

		[Test]
		public static void CallSMALLRWithTwoFloatsSecondIsSmaller() =>
			Assert.That(Core.SMALLR(6.6f, 2.2f), Is.EqualTo(2.2f));

		[Test]
		public static void CallSMALLRWithIntAndFloatFirstIsSmaller() =>
			Assert.That(Core.SMALLR(2, 6.6f), Is.EqualTo(2));

		[Test]
		public static void CallSMALLRWithIntAndFloatSecondIsSmaller() =>
			Assert.That(Core.SMALLR(6, 2.2f), Is.EqualTo(2.2f));

		[Test]
		public static void CallSMALLRWithFloatAndIntFirstIsSmaller() =>
			Assert.That(Core.SMALLR(2.2f, 6), Is.EqualTo(2.2f));

		[Test]
		public static void CallSMALLRWithFloatAndIntSecondIsSmaller() =>
			Assert.That(Core.SMALLR(6.6f, 2), Is.EqualTo(2));

		[Test]
		public static void CallSMALLRWithUnsupportedTypes() =>
			Assert.That(() => Core.SMALLR(Guid.NewGuid(), Guid.NewGuid()),
				Throws.TypeOf<InvalidOperationException>().And.Message.EqualTo($"Cannot add types \"{typeof(Guid)}\" and \"{typeof(Guid)}\""));

		[Test]
		public static void CallBOTHWithFalseAndFalse() =>
			Assert.That(Core.BOTH(false, false), Is.False);

		[Test]
		public static void CallBOTHWithFalseAndTrue() =>
			Assert.That(Core.BOTH(false, true), Is.False);

		[Test]
		public static void CallBOTHWithTrueAndFalse() =>
			Assert.That(Core.BOTH(true, false), Is.False);

		[Test]
		public static void CallBOTHWithTrueAndTrue() =>
			Assert.That(Core.BOTH(true, true), Is.True);

		[Test]
		public static void CallWONWithFalseAndFalse() =>
			Assert.That(Core.WON(false, false), Is.False);

		[Test]
		public static void CallWONWithFalseAndTrue() =>
			Assert.That(Core.WON(false, true), Is.True);

		[Test]
		public static void CallWONWithTrueAndFalse() =>
			Assert.That(Core.WON(true, false), Is.True);

		[Test]
		public static void CallWONWithTrueAndTrue() =>
			Assert.That(Core.WON(true, true), Is.False);

		[Test]
		public static void CallNOTWithFalse() =>
			Assert.That(Core.NOT(false), Is.True);

		[Test]
		public static void CallNOTWithTrue() =>
			Assert.That(Core.NOT(true), Is.False);

		[Test]
		public static void CallALLWithNoValues() =>
			Assert.That(Core.ALL(), Is.True);

		[Test]
		public static void CallALLWithOneFalse() =>
			Assert.That(Core.ALL(true, true, false), Is.False);

		[Test]
		public static void CallALLWithAllTrue() =>
			Assert.That(Core.ALL(true, true, true), Is.True);

		[Test]
		public static void CallANYWithNoValues() =>
			Assert.That(Core.ANY(), Is.False);

		[Test]
		public static void CallANYWithOneTrue() =>
			Assert.That(Core.ANY(false, false, true), Is.True);

		[Test]
		public static void CallANYWithAllFalse() =>
			Assert.That(Core.ANY(false, false, false), Is.False);

		[Test]
		public static void CallSAEMWithIntAndFloatEqual() =>
			Assert.That(Core.SAEM(2, 2.0f), Is.True);

		[Test]
		public static void CallSAEMWithIntAndFloatNotEqual() =>
			Assert.That(Core.SAEM(2, 1.0f), Is.False);

		[Test]
		public static void CallSAEMWithFloatAndIntEqual() =>
			Assert.That(Core.SAEM(2.0f, 2), Is.True);

		[Test]
		public static void CallSAEMWithFloatAndIntNotEqual() =>
			Assert.That(Core.SAEM(1.0f, 2), Is.False);

		[Test]
		public static void CallSAEMWithObjectsEqual() =>
			Assert.That(Core.SAEM("a", "a"), Is.True);

		[Test]
		public static void CallSAEMWithObjectsNotEqual() =>
			Assert.That(Core.SAEM("a", "b"), Is.False);

		[Test]
		public static void CallDIFFRINTWithIntAndFloatEqual() =>
			Assert.That(Core.DIFFRINT(2, 2.0f), Is.False);

		[Test]
		public static void CallDIFFRINTWithIntAndFloatNotEqual() =>
			Assert.That(Core.DIFFRINT(2, 1.0f), Is.True);

		[Test]
		public static void CallDIFFRINTWithFloatAndIntEqual() =>
			Assert.That(Core.DIFFRINT(2.0f, 2), Is.False);

		[Test]
		public static void CallDIFFRINTWithFloatAndIntNotEqual() =>
			Assert.That(Core.DIFFRINT(1.0f, 2), Is.True);

		[Test]
		public static void CallDIFFRINTWithObjectsEqual() =>
			Assert.That(Core.DIFFRINT("a", "a"), Is.False);

		[Test]
		public static void CallDIFFRINTWithObjectsNotEqual() =>
			Assert.That(Core.DIFFRINT("a", "b"), Is.True);

		[Test]
		public static void CallSMOOSHWithNoValues() =>
			Assert.That(Core.SMOOSH(), Is.EqualTo(string.Empty));

		[Test]
		public static void CallSMOOSHWithValues() =>
			Assert.That(Core.SMOOSH("a", "b", "c"), Is.EqualTo("abc"));

		[Test]
		public static void CallUPPINWithStringInt() =>
			Assert.That(Core.UPPIN("1"), Is.EqualTo(2));

		[Test]
		public static void CallUPPINWithInt() =>
			Assert.That(Core.UPPIN(1), Is.EqualTo(2));

		[Test]
		public static void CallUPPINWithFloat() =>
			Assert.That(Core.UPPIN(1.0f), Is.EqualTo(1.0f + 1));

		[Test]
		public static void CallUPPINWithUnsupportedType() =>
			Assert.That(() => Core.UPPIN(Guid.NewGuid()),
				Throws.TypeOf<ArgumentException>().And.Message.EqualTo($"Cannot call UPPIN on value of type {typeof(Guid).Name}"));

		[Test]
		public static void CallNERFINWithStringInt() =>
			Assert.That(Core.NERFIN("2"), Is.EqualTo(1));

		[Test]
		public static void CallNERFINWithInt() =>
			Assert.That(Core.NERFIN(2), Is.EqualTo(1));

		[Test]
		public static void CallNERFINWithFloat() =>
			Assert.That(Core.NERFIN(2.0f), Is.EqualTo(2.0f - 1));

		[Test]
		public static void CallNERFINWithUnsupportedType() =>
			Assert.That(() => Core.NERFIN(Guid.NewGuid()),
				Throws.TypeOf<ArgumentException>().And.Message.EqualTo($"Cannot call NERFIN on value of type {typeof(Guid).Name}"));
	}
}