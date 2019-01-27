using LOLCode.Compiler.Symbols;
using NUnit.Framework;

namespace LOLCode.Compiler.Tests.Symbols
{
	public static class ArgumentRefTests
	{
		[Test]
		public static void Create()
		{
			var argumentRef = new ArgumentRef("a", 1);
			Assert.That(argumentRef.Name, Is.EqualTo("a"), nameof(SymbolRef.Name));
			Assert.That(argumentRef.Number, Is.EqualTo(1), nameof(ArgumentRef.Number));
			Assert.That(argumentRef.Type, Is.EqualTo(typeof(object)), nameof(VariableRef.Type));
		}
	}
}
