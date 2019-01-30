using LOLCode.Compiler.Syntax;
using NUnit.Framework;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Tests.Syntax
{
	public static class CodePragmaTests
	{
		[Test]
		public static void Create()
		{
			var assemblyName = new AssemblyName("a");
			var fileName = $"{assemblyName.Name}.dll";
			var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
				assemblyName, AssemblyBuilderAccess.RunAndSave);
			var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name, fileName, true);
			var document = moduleBuilder.DefineDocument(fileName, Guid.Empty, Guid.Empty, Guid.Empty);
			var pragma = new CodePragma(document, fileName, 1, 2);

			Assert.That(pragma.doc, Is.SameAs(document), nameof(pragma.doc));
			Assert.That(pragma.endColumn, Is.EqualTo(2), nameof(pragma.endColumn));
			Assert.That(pragma.endLine, Is.EqualTo(1), nameof(pragma.endLine));
			Assert.That(pragma.filename, Is.EqualTo(fileName), nameof(pragma.filename));
			Assert.That(pragma.startColumn, Is.EqualTo(2), nameof(pragma.startColumn));
			Assert.That(pragma.startLine, Is.EqualTo(1), nameof(pragma.startLine));
		}

		[Test]
		public static void MarkSequencePoint()
		{
			var (pragma, _, ilGenerator) = SyntaxEmitGenerators.Create();
			pragma.MarkSequencePoint(ilGenerator);

			Assert.That(ilGenerator.ILOffset, Is.EqualTo(0), nameof(ilGenerator.ILOffset));
		}
	}
}
