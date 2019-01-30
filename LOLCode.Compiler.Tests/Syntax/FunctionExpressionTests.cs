using LOLCode.Compiler.Symbols;
using LOLCode.Compiler.Syntax;
using NUnit.Framework;
using System;
using System.CodeDom.Compiler;

namespace LOLCode.Compiler.Tests.Syntax
{
	public static class FunctionExpressionTests
	{
		public static int NoParameters() => 0;

		[Test]
		public static void Create()
		{
			var (pragma, _, _) = SyntaxEmitGenerators.Create();
			var expression = new FunctionExpression(pragma);

			Assert.That(expression.location, Is.SameAs(pragma), nameof(expression.location));
			Assert.That(expression.func, Is.Null, nameof(expression.func));
			Assert.That(expression.arguments.Count, Is.EqualTo(0), nameof(expression.arguments));
		}

		[Test]
		public static void CreateWithFunctionRef()
		{
			var (pragma, method, _) = SyntaxEmitGenerators.Create();
			const string methodName = nameof(FunctionExpressionTests.NoParameters);
			var methodInfo = typeof(FunctionExpressionTests).GetMethod(methodName);
			var importFunctionRef = new ImportFunctionRef(methodInfo, methodName);
			var expression = new FunctionExpression(pragma, importFunctionRef);

			Assert.That(expression.location, Is.SameAs(pragma), nameof(expression.location));
			Assert.That(expression.func, Is.SameAs(importFunctionRef), nameof(expression.func));
			Assert.That(expression.arguments.Count, Is.EqualTo(0), nameof(expression.arguments));
		}

		[Test]
		public static void Emit()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();

			const string methodName = nameof(FunctionExpressionTests.NoParameters);
			var methodInfo = typeof(FunctionExpressionTests).GetMethod(methodName);
			var importFunctionRef = new ImportFunctionRef(methodInfo, methodName);
			var expression = new FunctionExpression(pragma, importFunctionRef);

			expression.Emit(method, typeof(object), ilGenerator);
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(10), nameof(ilGenerator.ILOffset));
		}

		[Test]
		public static void Process()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			var errors = new CompilerErrorCollection();

			const string methodName = nameof(FunctionExpressionTests.NoParameters);
			var methodInfo = typeof(FunctionExpressionTests).GetMethod(methodName);
			var importFunctionRef = new ImportFunctionRef(methodInfo, methodName);
			var expression = new FunctionExpression(pragma, importFunctionRef);
			expression.Process(method, errors, ilGenerator);
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(0), nameof(ilGenerator.ILOffset));
			Assert.That(errors.Count, Is.EqualTo(0), nameof(errors.Count));
		}
	}
}
