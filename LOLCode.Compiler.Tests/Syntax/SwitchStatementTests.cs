using LOLCode.Compiler.Syntax;
using NUnit.Framework;
using System;
using System.CodeDom.Compiler;

namespace LOLCode.Compiler.Tests.Syntax
{
	public static class SwitchStatementTests
	{
		[Test]
		public static void Create()
		{
			var (pragma, _, _) = SyntaxEmitGenerators.Create();
			var statement = new SwitchStatement(pragma);

			Assert.That(statement.location, Is.SameAs(pragma), nameof(statement.location));
			Assert.That(statement.cases.Count, Is.EqualTo(0), nameof(statement.cases));
			Assert.That(statement.defaultCase, Is.Null, nameof(statement.defaultCase));
			Assert.That(statement.control, Is.Null, nameof(statement.control));
		}

		[Test]
		public static void Emit()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			var errors = new CompilerErrorCollection();

			var statement = new SwitchStatement(pragma)
			{
				control = new MockExpression(pragma),
				defaultCase = new MockStatement(pragma) 
			};

			statement.cases.Add(new SwitchStatement.Case("a", new MockStatement(pragma)));

			statement.Process(method, errors, ilGenerator);
			statement.Emit(method, ilGenerator);
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(37), nameof(ilGenerator.ILOffset));
		}

		[Test]
		public static void Process()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			var errors = new CompilerErrorCollection();

			var statement = new SwitchStatement(pragma)
			{
				control = new MockExpression(pragma),
				defaultCase = new MockStatement(pragma)
			};

			statement.cases.Add(new SwitchStatement.Case("a", new MockStatement(pragma)));

			statement.Process(method, errors, ilGenerator);
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(6), nameof(ilGenerator.ILOffset));
			Assert.That(errors.Count, Is.EqualTo(0), nameof(errors.Count));
		}
	}
}
