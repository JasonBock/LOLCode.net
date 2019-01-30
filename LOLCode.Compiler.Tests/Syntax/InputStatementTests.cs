using LOLCode.Compiler.Syntax;
using NUnit.Framework;
using System;
using System.CodeDom.Compiler;

namespace LOLCode.Compiler.Tests.Syntax
{
	public static class InputStatementTests
	{
		[Test]
		public static void Create()
		{
			var (pragma, _, _) = SyntaxEmitGenerators.Create();
			var statement = new InputStatement(pragma);

			Assert.That(statement.location, Is.SameAs(pragma), nameof(statement.location));
			Assert.That(statement.amount, Is.EqualTo(IOAmount.Line), nameof(statement.amount));
			Assert.That(statement.dest, Is.Null, nameof(statement.dest));
		}

		[Test]
		public static void Emit()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();

			var statement = new InputStatement(pragma)
			{
				dest = new MockLValue(pragma),
			};
			statement.Emit(method, ilGenerator);
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(11), nameof(ilGenerator.ILOffset));
		}

		[Test]
		public static void Process()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			var errors = new CompilerErrorCollection();

			var statement = new InputStatement(pragma)
			{
				dest = new MockLValue(pragma),
			};

			statement.Process(method, errors, ilGenerator);

			Assert.That(ilGenerator.ILOffset, Is.EqualTo(1), nameof(ilGenerator.ILOffset));
			Assert.That(errors.Count, Is.EqualTo(0), nameof(errors.Count));
		}
	}
}
