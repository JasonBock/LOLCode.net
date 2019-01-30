using LOLCode.Compiler.Syntax;
using NUnit.Framework;
using System;
using System.CodeDom.Compiler;

namespace LOLCode.Compiler.Tests.Syntax
{
	public static class TypecastExpressionTests
	{
		[Test]
		public static void Create()
		{
			var (pragma, _, _) = SyntaxEmitGenerators.Create();
			var expression = new TypecastExpression(pragma);

			Assert.That(expression.location, Is.SameAs(pragma), nameof(expression.location));
			Assert.That(expression.destType, Is.Null, nameof(expression.destType));
			Assert.That(expression.exp, Is.Null, nameof(expression.exp));
		}

		[Test]
		public static void EmitWhenTypesAreTheSame()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();

			var expression = new TypecastExpression(pragma)
			{
				destType = typeof(int),
				exp = new MockExpression(pragma),
			};
			expression.Emit(method, typeof(int), ilGenerator);
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(1), nameof(ilGenerator.ILOffset));
		}

		[Test]
		public static void EmitWhenTypesAreDifferent()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();

			var expression = new TypecastExpression(pragma)
			{
				destType = typeof(Guid),
				exp = new MockExpression(pragma),
			};
			expression.Emit(method, typeof(object), ilGenerator);
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(6), nameof(ilGenerator.ILOffset));
		}

		[Test]
		public static void Process()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			var errors = new CompilerErrorCollection();

			var expression = new TypecastExpression(pragma)
			{
				destType = typeof(Guid),
				exp = new MockExpression(pragma),
			};
			expression.Process(method, errors, ilGenerator);
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(2), nameof(ilGenerator.ILOffset));
			Assert.That(errors.Count, Is.EqualTo(0), nameof(errors.Count));
		}
	}
}
