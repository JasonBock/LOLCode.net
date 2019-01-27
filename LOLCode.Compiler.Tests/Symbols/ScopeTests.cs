using LOLCode.Compiler.Symbols;
using NUnit.Framework;

namespace LOLCode.Compiler.Tests.Symbols
{
	public static class ScopeTests
	{
		[Test]
		public static void GetSymbolWithValidKey()
		{
			var localRef = new LocalRef("a");
			var scope = new Scope();
			scope.AddSymbol(localRef);
			Assert.That(scope["a"], Is.SameAs(localRef));
		}

		[Test]
		public static void GetSymbolWithInvalidKeyAndNullParentScope()
		{
			var localRef = new LocalRef("a");
			var scope = new Scope();
			scope.AddSymbol(localRef);
			Assert.That(scope["b"], Is.Null);
		}

		[Test]
		public static void GetSymbolWithInvalidKeyAndParentScopeWithValidKey()
		{
			var localRef = new LocalRef("a");
			var parentScope = new Scope();
			parentScope.AddSymbol(localRef);
			var scope = new Scope(parentScope);
			Assert.That(scope["a"], Is.SameAs(localRef));
		}

		[Test]
		public static void GetSymbolWithInvalidKeyAndParentScopeWithInvalidKey()
		{
			var localRef = new LocalRef("a");
			var parentScope = new Scope();
			parentScope.AddSymbol(localRef);
			var scope = new Scope(parentScope);
			Assert.That(scope["b"], Is.Null);
		}

		[Test]
		public static void RemoveSymbol()
		{
			var localRef = new LocalRef("a");
			var scope = new Scope();
			scope.AddSymbol(localRef);
			scope.RemoveSymbol(localRef);
			Assert.That(scope["a"], Is.Null);
		}
	}
}
