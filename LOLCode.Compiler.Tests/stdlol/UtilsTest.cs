using LOLCode.Compiler;
using NUnit.Framework;
using stdlol;
using System;
using System.Collections.Generic;

namespace LOLCode.net.Tests.stdlol
{
	[TestFixture]
	public class UtilsTest
	{
		[Test]
		public void ToStringString()
		{
			var value = "CHEEZBURGER";

			Assert.AreEqual(value, Utils.ToString(value));
			Assert.AreEqual(value.ToString(), Utils.ToString(value as object));
			Assert.AreNotEqual("cheezburger", Utils.ToString(value));
			Assert.AreNotEqual("bucket", Utils.ToString(value));
		}

		[Test]
		public void ToStringNull()
		{
			object value = null;
			Assert.AreEqual(string.Empty, Utils.ToString(value));
		}

		[Test]
		public void ToStringDictionary()
		{
			var dict = new Dictionary<object, object>
			{
				{ 0, "KITTEH" }
			};
			Assert.AreEqual("KITTEH", Utils.ToString(dict));
		}

		[Test]
		public void ToStringDictionaryNoZeroIndex()
		{
			var dict = new Dictionary<object, object>
			{
				{ 1, "KITTEH" }
			};
			Assert.AreEqual(string.Empty, Utils.ToString(dict));
		}

		[Test]
		public void ToStringInvalidType()
		{
			var value = Guid.NewGuid();
			Assert.That(() => Utils.ToString(value),
				Throws.TypeOf<InvalidCastException>()
					.With.Property(nameof(Exception.Message)).EqualTo("Cannot cast type \"Guid\" to string"));
		}

		[Test]
		public void ToIntInt()
		{
			var value = 1337;
			var value2 = 0;

			Assert.AreEqual(1337, Utils.ToInt(value));
			Assert.AreEqual(1337, Utils.ToInt(value as object));
			Assert.AreEqual(0, Utils.ToInt(value2));
		}

		[Test]
		public void ToIntNull()
		{
			object value = null;
			Assert.AreEqual(0, Utils.ToInt(value));
		}

		[Test]
		public void ToIntDictionary()
		{
			var dictionary = new Dictionary<object, object>
			{
				{ 0, 1337 }
			};

			Assert.AreEqual(1337, Utils.ToInt(dictionary));
			Assert.AreEqual(1337, Utils.ToInt(dictionary as object));
		}

		[Test]
		public void ToIntDictionaryNoZeroIndex()
		{
			// NOTE: I'm unsure of this behavior...
			var dictionary = new Dictionary<object, object>
			{
				{ 1, 1337 }
			};

			Assert.AreEqual(0, Utils.ToInt(dictionary));
			Assert.AreEqual(0, Utils.ToInt(dictionary as object));
		}

		[Test]
		public void ToIntInvalidString()
		{
			var value = "KITTEH";
			Assert.That(() => Utils.ToInt(value),
				Throws.TypeOf<InvalidCastException>()
					.With.Property(nameof(Exception.Message)).EqualTo("Cannot cast type \"String\" to int"));
		}

		[Test]
		public void ToIntInvalidType()
		{
			var value = Guid.NewGuid();
			Assert.That(() => Utils.ToInt(value),
				Throws.TypeOf<InvalidCastException>()
					.With.Property(nameof(Exception.Message)).EqualTo("Cannot cast type \"Guid\" to int"));
		}
	}
}
