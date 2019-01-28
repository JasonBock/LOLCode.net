using LOLCode.Compiler.Syntax;
using NUnit.Framework;
using System.CodeDom.Compiler;

namespace LOLCode.Compiler.Tests.Syntax
{
	public static class BlockStatementTests
	{
		[Test]
		public static void Create()
		{
			var (pragma, _, _) = SyntaxEmitGenerators.Create();
			var statement = new BlockStatement(pragma);

			Assert.That(statement.location, Is.SameAs(pragma), nameof(statement.location));
			Assert.That(statement.statements.Count, Is.EqualTo(0), nameof(statement.statements));
		}

		[Test]
		public static void Emit()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();

			var statement = new BlockStatement(pragma);
			statement.statements.Add(new MockStatement(pragma));
			statement.Emit(method, ilGenerator);

			Assert.That(ilGenerator.ILOffset, Is.EqualTo(1));
		}

		[Test]
		public static void Process()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			var errors = new CompilerErrorCollection();

			var statement = new BlockStatement(pragma);
			statement.statements.Add(new MockStatement(pragma));
			statement.Process(method, errors, ilGenerator);

			Assert.That(ilGenerator.ILOffset, Is.EqualTo(2));
		}
	}
}