using LOLCode.Compiler.Syntax;
using NUnit.Framework;
using System.CodeDom.Compiler;

namespace LOLCode.Compiler.Tests.Syntax
{
	public static class ConditionalStatementTests
	{
		[Test]
		public static void Create()
		{
			var (pragma, _, _) = SyntaxEmitGenerators.Create();
			var statement = new ConditionalStatement(pragma);

			Assert.That(statement.location, Is.SameAs(pragma), nameof(statement.location));
			Assert.That(statement.condition, Is.Null, nameof(statement.condition));
			Assert.That(statement.trueStatements, Is.Null, nameof(statement.trueStatements));
			Assert.That(statement.falseStatements, Is.Null, nameof(statement.falseStatements));
		}

		[Test]
		public static void Emit()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();

			var statement = new ConditionalStatement(pragma)
			{
				condition = new MockExpression(pragma),
				trueStatements = new MockStatement(pragma),
				falseStatements = new MockStatement(pragma),
				ifFalse = ilGenerator.DefineLabel(),
				statementEnd = ilGenerator.DefineLabel(),
			};
			statement.Emit(method, ilGenerator);
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(13), nameof(ilGenerator.ILOffset));
		}

		[Test]
		public static void Process()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			var errors = new CompilerErrorCollection();

			var statement = new ConditionalStatement(pragma)
			{
				condition = new MockExpression(pragma),
				trueStatements = new MockStatement(pragma),
				falseStatements = new MockStatement(pragma),
			};
			statement.Process(method, errors, ilGenerator);
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(6), nameof(ilGenerator.ILOffset));
			Assert.That(errors.Count, Is.EqualTo(0), nameof(errors.Count));
		}
	}
}
