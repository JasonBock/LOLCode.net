using LOLCode.Compiler.Emitter;
using LOLCode.Compiler.Symbols;
using LOLCode.Compiler.Syntax;
using NUnit.Framework;
using System;
using System.CodeDom.Compiler;
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

		[Test]
		public static void Emit()
		{
			var assemblyName = new AssemblyName("a");
			var fileName = $"{assemblyName.Name}.dll";
			var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
				assemblyName, AssemblyBuilderAccess.RunAndSave);
			var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name, fileName, true);
			var document = moduleBuilder.DefineDocument(fileName, Guid.Empty, Guid.Empty, Guid.Empty);
			var typeBuilder = moduleBuilder.DefineType("b");
			var methodBuilder = typeBuilder.DefineMethod("c", MethodAttributes.Static | MethodAttributes.Public);
			var ilGenerator = methodBuilder.GetILGenerator();
			var localBuilder = ilGenerator.DeclareLocal(typeof(int));
			var pragma = new CodePragma(document, fileName, 1, 2);

			var options = new CompilerParameters
			{
				GenerateExecutable = true,
				GenerateInMemory = false,
				OutputAssembly = "a.exe",
				MainClass = "Program",
				IncludeDebugInformation = false
			};
			var method = new LOLMethod(new UserFunctionRef("UFR", 0, false), new LOLProgram(options));
			var localRef = new LocalRef("a") { Local = localBuilder };

			var lValue = new VariableLValue(pragma, localRef);
			var rValue = new VariableLValue(pragma, localRef);

			var statement = new AssignmentStatement(pragma)
			{
				lval = lValue,
				rval = rValue,
				location = pragma
			};
			statement.Emit(method, ilGenerator);
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(2));
		}

		[Test]
		public static void Process()
		{
			var assemblyName = new AssemblyName("a");
			var fileName = $"{assemblyName.Name}.dll";
			var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
				assemblyName, AssemblyBuilderAccess.RunAndSave);
			var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name, fileName, true);
			var document = moduleBuilder.DefineDocument(fileName, Guid.Empty, Guid.Empty, Guid.Empty);
			var typeBuilder = moduleBuilder.DefineType("b");
			var methodBuilder = typeBuilder.DefineMethod("c", MethodAttributes.Static | MethodAttributes.Public);
			var ilGenerator = methodBuilder.GetILGenerator();
			var localBuilder = ilGenerator.DeclareLocal(typeof(int));
			var pragma = new CodePragma(document, fileName, 1, 2);
			var options = new CompilerParameters
			{
				GenerateExecutable = true,
				GenerateInMemory = false,
				OutputAssembly = "a.exe",
				MainClass = "Program",
				IncludeDebugInformation = false
			};
			var method = new LOLMethod(new UserFunctionRef("UFR", 0, false), new LOLProgram(options));
			var localRef = new LocalRef("a") { Local = localBuilder };
			var lValue = new VariableLValue(pragma, localRef);
			var rValue = new VariableLValue(pragma, localRef);
			var errors = new CompilerErrorCollection();

			var statement = new AssignmentStatement(pragma)
			{
				lval = lValue,
				rval = rValue,
			};
			statement.Process(method, errors, ilGenerator);

			Assert.That(ilGenerator.ILOffset, Is.EqualTo(0));
		}
	}
}