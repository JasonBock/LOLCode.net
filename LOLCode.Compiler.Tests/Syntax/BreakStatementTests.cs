using LOLCode.Compiler.Syntax;
using NUnit.Framework;
using System.CodeDom.Compiler;

namespace LOLCode.Compiler.Tests.Syntax
{
	public static class BreakStatementTests
	{
		[Test]
		public static void Create()
		{
			var (pragma, _, _) = SyntaxEmitGenerators.Create();
			var statement = new BreakStatement(pragma);

			Assert.That(statement.location, Is.SameAs(pragma), nameof(statement.location));
			Assert.That(statement.label, Is.Null, nameof(statement.label));
		}

		[Test]
		public static void Emit()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();

			var statement = new BreakStatement(pragma);
			statement.Emit(method, ilGenerator);

			Assert.That(ilGenerator.ILOffset, Is.EqualTo(2), nameof(ilGenerator.ILOffset));
		}

		[Test]
		public static void Process()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			var errors = new CompilerErrorCollection();

			var statement = new BreakStatement(pragma);
			statement.Process(method, errors, ilGenerator);

			Assert.That(ilGenerator.ILOffset, Is.EqualTo(0), nameof(ilGenerator.ILOffset));
			Assert.That(errors.Count, Is.EqualTo(0), nameof(errors.Count));
		}
	}
}
