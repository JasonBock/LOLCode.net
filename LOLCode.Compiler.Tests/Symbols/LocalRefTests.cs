using LOLCode.Compiler.Symbols;
using NUnit.Framework;

namespace LOLCode.Compiler.Tests.Symbols
{
	public static class LocalRefTests
	{
		[Test]
		public static void Create()
		{
			var localRef = new LocalRef("a");
			Assert.That(localRef.Name, Is.EqualTo("a"), nameof(LocalRef.Name));
			Assert.That(localRef.Local, Is.Null, nameof(LocalRef.Local));
			Assert.That(localRef.Type, Is.EqualTo(typeof(object)), nameof(LocalRef.Type));
		}
	}
}
