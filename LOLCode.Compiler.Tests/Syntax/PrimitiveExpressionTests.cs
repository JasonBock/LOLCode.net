using LOLCode.Compiler.Syntax;
using NUnit.Framework;
using System;
using System.CodeDom.Compiler;

namespace LOLCode.Compiler.Tests.Syntax
{
	public static class PrimitiveExpressionTests
	{
		[Test]
		public static void Create()
		{
			var (pragma, _, _) = SyntaxEmitGenerators.Create();
			var expression = new PrimitiveExpression(pragma);

			Assert.That(expression.location, Is.SameAs(pragma), nameof(expression.location));
			Assert.That(expression.value, Is.Null, nameof(expression.value));
		}

		[Test]
		public static void CreateWithValue()
		{
			var value = new object();
			var (pragma, _, _) = SyntaxEmitGenerators.Create();
			var expression = new PrimitiveExpression(pragma, value);

			Assert.That(expression.location, Is.SameAs(pragma), nameof(expression.location));
			Assert.That(expression.value, Is.SameAs(value), nameof(expression.value));
		}

		[Test]
		public static void Emit()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();

			var value = new object();
			var expression = new PrimitiveExpression(pragma, value);
			expression.Emit(method, typeof(object), ilGenerator);
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(0), nameof(ilGenerator.ILOffset));
		}

		[Test]
		public static void Process()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			var errors = new CompilerErrorCollection();

			var value = 3;
			var expression = new PrimitiveExpression(pragma, value);
			expression.Process(method, errors, ilGenerator);
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(0), nameof(ilGenerator.ILOffset));
			Assert.That(errors.Count, Is.EqualTo(0), nameof(errors.Count));
		}
	}
}
