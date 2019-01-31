using LOLCode.Compiler.Syntax;
using NUnit.Framework;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Tests.Syntax
{
	public static class PrintStatementTests
	{
		[Test]
		public static void Create()
		{
			var (pragma, _, _) = SyntaxEmitGenerators.Create();
			var statement = new PrintStatement(pragma);

			Assert.That(statement.location, Is.SameAs(pragma), nameof(CodeObject.location));
			Assert.That(statement.stderr, Is.False, nameof(PrintStatement.stderr));
			Assert.That(statement.message, Is.Null, nameof(PrintStatement.message));
			Assert.That(statement.newline, Is.True, nameof(PrintStatement.newline));
		}

		[Test]
		public static void Emit()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();

			var statement = new PrintStatement(pragma)
			{
				message = new MockExpression(pragma)
			};
			statement.Emit(method, ilGenerator);
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(12), nameof(ILGenerator.ILOffset));
		}

		[Test]
		public static void Process()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			var errors = new CompilerErrorCollection();

			var statement = new PrintStatement(pragma)
			{
				message = new MockExpression(pragma)
			};
			statement.Process(method, errors, ilGenerator);

			Assert.That(ilGenerator.ILOffset, Is.EqualTo(2), nameof(ILGenerator.ILOffset));
			Assert.That(errors.Count, Is.EqualTo(0), nameof(CollectionBase.Count));
		}
	}
}
