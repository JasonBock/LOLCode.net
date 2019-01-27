using LOLCode.Compiler.Symbols;
using NUnit.Framework;

namespace LOLCode.Compiler.Tests.Symbols
{
	public static class GlobalRefTests
	{
		[Test]
		public static void Create()
		{
			var globalRef = new GlobalRef("a");
			Assert.That(globalRef.Name, Is.EqualTo("a"), nameof(SymbolRef.Name));
			Assert.That(globalRef.Field, Is.Null, nameof(GlobalRef.Field));
			Assert.That(globalRef.Type, Is.EqualTo(typeof(object)), nameof(VariableRef.Type));
		}
	}
}
