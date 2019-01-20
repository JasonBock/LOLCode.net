using LOLCode.Compiler.Syntax;
using NUnit.Framework;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Tests.Syntax
{
	public static class AssignmentStatementTests
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
			var pragma = new CodePragma(document, fileName, 0, 0);
			var statement = new AssignmentStatement(pragma);

			Assert.That(statement.location, Is.SameAs(pragma), nameof(statement.location));
			Assert.That(statement.lval, Is.Null, nameof(statement.lval));
			Assert.That(statement.rval, Is.Null, nameof(statement.rval));
		}
	}
}
