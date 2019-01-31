using LOLCode.Compiler.Parser.v1_2;
using LOLCode.Compiler.Symbols;
using LOLCode.Compiler.Syntax;
using NUnit.Framework;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Tests.Syntax
{
	public static class StringExpressionTests
	{
		[Test]
		public static void Create()
		{
			var (pragma, _, _) = SyntaxEmitGenerators.Create();
			var errors = new CompilerErrorCollection();
			var expression = new StringExpression(pragma, "a", new Scope(), new Errors(errors));

			Assert.That(expression.location, Is.SameAs(pragma), nameof(CodeObject.location));
		}

		[Test]
		public static void Emit()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			var errors = new CompilerErrorCollection();
			var expression = new StringExpression(pragma, "a", new Scope(), new Errors(errors));
			expression.Emit(method, typeof(string), ilGenerator);

			Assert.That(ilGenerator.ILOffset, Is.EqualTo(5), nameof(ILGenerator.ILOffset));
			Assert.That(errors.Count, Is.EqualTo(1), nameof(CollectionBase.Count));
		}

		[Test]
		public static void Process()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			var errors = new CompilerErrorCollection();
			var expression = new StringExpression(pragma, "a", new Scope(), new Errors(errors));
			expression.Process(method, errors, ilGenerator);

			Assert.That(ilGenerator.ILOffset, Is.EqualTo(0), nameof(ILGenerator.ILOffset));
			Assert.That(errors.Count, Is.EqualTo(1), nameof(CollectionBase.Count));
		}

		[Test]
		public static void UnescapeString()
		{
			var (pragma, _, _) = SyntaxEmitGenerators.Create();
			var errors = new CompilerErrorCollection();
			var result = StringExpression.UnescapeString("abc", new Scope(), new Errors(errors), pragma,
				new List<VariableRef>());

			Assert.That(result, Is.EqualTo("b"));
		}
	}
}
