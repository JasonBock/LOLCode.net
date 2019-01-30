using LOLCode.Compiler.Symbols;
using LOLCode.Compiler.Syntax;
using NUnit.Framework;
using System;
using System.CodeDom.Compiler;

namespace LOLCode.Compiler.Tests.Syntax
{
	public static class LoopStatementTests
	{
		[Test]
		public static void Create()
		{
			var (pragma, _, _) = SyntaxEmitGenerators.Create();
			var statement = new LoopStatement(pragma);

			Assert.That(statement.location, Is.SameAs(pragma), nameof(statement.location));
			Assert.That(statement.name, Is.Null, nameof(statement.name));
			Assert.That(statement.statements, Is.Null, nameof(statement.statements));
			Assert.That(statement.operation, Is.Null, nameof(statement.operation));
			Assert.That(statement.type, Is.EqualTo(LoopType.Infinite), nameof(statement.type));
			Assert.That(statement.condition, Is.Null, nameof(statement.condition));
		}

		[Test]
		public static void Emit()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();

			var errors = new CompilerErrorCollection();

			var statement = new LoopStatement(pragma)
			{
				statements = new MockStatement(pragma)
			};
			statement.Process(method, errors, ilGenerator);
			statement.Emit(method, ilGenerator);
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(8), nameof(ilGenerator.ILOffset));
		}

		[Test]
		public static void Process()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			var errors = new CompilerErrorCollection();

			var statement = new LoopStatement(pragma)
			{
				statements = new MockStatement(pragma)
			};
			statement.Process(method, errors, ilGenerator);

			Assert.That(ilGenerator.ILOffset, Is.EqualTo(2), nameof(ilGenerator.ILOffset));
			Assert.That(errors.Count, Is.EqualTo(0), nameof(errors.Count));
		}

		[Test]
		public static void StartOperation()
		{
			var (pragma, _, _) = SyntaxEmitGenerators.Create();
			var statement = new LoopStatement(pragma);
			statement.StartOperation(pragma);
			Assert.That(statement.operation, Is.Not.Null, nameof(statement.operation));
		}

		[Test]
		public static void SetOperationFunction()
		{
			var (pragma, _, _) = SyntaxEmitGenerators.Create();
			var statement = new LoopStatement(pragma);
			statement.StartOperation(pragma);
			statement.SetOperationFunction(new UserFunctionRef("a", 0, false));
			Assert.That((statement.operation as AssignmentStatement).rval, Is.Not.Null, nameof(statement.operation));
		}

		[Test]
		public static void SetLoopVariable()
		{
			var (pragma, _, _) = SyntaxEmitGenerators.Create();
			var statement = new LoopStatement(pragma);
			statement.StartOperation(pragma);
			statement.SetOperationFunction(new UserFunctionRef("a", 0, false));
			statement.SetLoopVariable(pragma, new LocalRef("a"));
			Assert.That((statement.operation as AssignmentStatement).lval, Is.Not.Null, nameof(statement.operation));
			Assert.That(((statement.operation as AssignmentStatement).rval as FunctionExpression).arguments.Count, 
				Is.EqualTo(1), nameof(AssignmentStatement.rval));
		}

		[Test]
		public static void GetLoopVariable()
		{
			var (pragma, _, _) = SyntaxEmitGenerators.Create();
			var statement = new LoopStatement(pragma);
			Assert.That(statement.BreakLabel, Is.Not.Null, nameof(LoopStatement.BreakLabel));
		}

		[Test]
		public static void BreakLabel()
		{
			var (pragma, _, _) = SyntaxEmitGenerators.Create();
			var statement = new LoopStatement(pragma);
			Assert.That(statement.BreakLabel, Is.Not.Null, nameof(LoopStatement.BreakLabel));
		}

		[Test]
		public static void ContinueLabel()
		{
			var (pragma, _, _) = SyntaxEmitGenerators.Create();
			var statement = new LoopStatement(pragma);
			Assert.That(statement.ContinueLabel, Is.Not.Null, nameof(LoopStatement.ContinueLabel));
		}

		[Test]
		public static void Name()
		{
			var (pragma, _, _) = SyntaxEmitGenerators.Create();
			var statement = new LoopStatement(pragma)
			{
				name = "b"
			};
			Assert.That(statement.Name, Is.EqualTo("b"), nameof(LoopStatement.Name));
		}
	}
}
