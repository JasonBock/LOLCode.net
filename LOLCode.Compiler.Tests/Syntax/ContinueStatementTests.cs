﻿using LOLCode.Compiler.Syntax;
using NUnit.Framework;
using System;
using System.CodeDom.Compiler;

namespace LOLCode.Compiler.Tests.Syntax
{
	public static class ContinueStatementTests
	{
		[Test]
		public static void Create()
		{
			var (pragma, _, _) = SyntaxEmitGenerators.Create();
			var statement = new ContinueStatement(pragma);

			Assert.That(statement.location, Is.SameAs(pragma), nameof(statement.location));
			Assert.That(statement.label, Is.Null, nameof(statement.label));
		}

		[Test, Ignore("Index issue, needs more investigation...")]
		public static void Emit()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			var statement = new ContinueStatement(pragma);
			statement.Emit(method, ilGenerator);
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(1), nameof(ilGenerator.ILOffset));
		}

		[Test]
		public static void Process()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			var errors = new CompilerErrorCollection();

			var statement = new ContinueStatement(pragma);
			statement.Process(method, errors, ilGenerator);
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(0), nameof(ilGenerator.ILOffset));
			Assert.That(errors.Count, Is.EqualTo(1), nameof(errors.Count));
		}
	}
}
