using LOLCode.Compiler.Syntax;
using NUnit.Framework;
using System.CodeDom.Compiler;

namespace LOLCode.Compiler.Tests.Syntax
{
	public static class AssignmentStatementTests
	{
		[Test]
		public static void Create()
		{
			var (pragma, _, _) = SyntaxEmitGenerators.Create();
			var statement = new AssignmentStatement(pragma);

			Assert.That(statement.location, Is.SameAs(pragma), nameof(statement.location));
			Assert.That(statement.lval, Is.Null, nameof(statement.lval));
			Assert.That(statement.rval, Is.Null, nameof(statement.rval));
		}

		[Test]
		public static void Emit()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();

			var statement = new AssignmentStatement(pragma)
			{
				lval = new MockLValue(pragma),
				rval = new MockExpression(pragma),
				location = pragma
			};
			statement.Emit(method, ilGenerator);
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(2));
		}

		[Test]
		public static void Process()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			var errors = new CompilerErrorCollection();

			var statement = new AssignmentStatement(pragma)
			{
				lval = new MockLValue(pragma),
				rval = new MockExpression(pragma),
			};
			statement.Process(method, errors, ilGenerator);

			Assert.That(ilGenerator.ILOffset, Is.EqualTo(3));
		}
	}
}