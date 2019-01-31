using LOLCode.Compiler.Symbols;
using LOLCode.Compiler.Syntax;
using NUnit.Framework;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Tests.Syntax
{
	public static class VariableLValueTests
	{
		[Test]
		public static void Create()
		{
			var (pragma, _, _) = SyntaxEmitGenerators.Create();
			var statement = new VariableLValue(pragma);

			Assert.That(statement.location, Is.SameAs(pragma), nameof(CodeObject.location));
			Assert.That(statement.var, Is.Null, nameof(VariableLValue.var));
		}

		[Test]
		public static void CreateWithVariableRef()
		{
			var (pragma, _, _) = SyntaxEmitGenerators.Create();
			var localRef = new LocalRef("a");
			var statement = new VariableLValue(pragma, localRef);

			Assert.That(statement.location, Is.SameAs(pragma), nameof(CodeObject.location));
			Assert.That(statement.var, Is.SameAs(localRef), nameof(VariableLValue.var));
		}

		[Test]
		public static void EmitGet()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			var localBuilder = ilGenerator.DeclareLocal(typeof(int));
			var localRef = new LocalRef("a") { Local = localBuilder };
			var statement = new VariableLValue(pragma, localRef);

			statement.EmitGet(method, typeof(object), ilGenerator);
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(1), nameof(ILGenerator.ILOffset));
		}

		[Test]
		public static void StartSet()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			var localBuilder = ilGenerator.DeclareLocal(typeof(int));
			var localRef = new LocalRef("a") { Local = localBuilder };
			var statement = new VariableLValue(pragma, localRef);

			statement.StartSet(method, typeof(object), ilGenerator);
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(0), nameof(ILGenerator.ILOffset));
		}

		[Test]
		public static void EndSet()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			var localBuilder = ilGenerator.DeclareLocal(typeof(int));
			var localRef = new LocalRef("a") { Local = localBuilder };
			var statement = new VariableLValue(pragma, localRef);

			statement.EndSet(method, typeof(object), ilGenerator);
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(1), nameof(ILGenerator.ILOffset));
		}

		[Test]
		public static void Process()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			var errors = new CompilerErrorCollection();

			var statement = new VariableLValue(pragma);
			statement.Process(method, errors, ilGenerator);
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(0), nameof(ILGenerator.ILOffset));
			Assert.That(errors.Count, Is.EqualTo(0), nameof(CollectionBase.Count));
		}
	}
}
