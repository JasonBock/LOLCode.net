using LOLCode.Compiler.Emitter;
using LOLCode.Compiler.Symbols;
using LOLCode.Compiler.Syntax;
using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Tests.Syntax
{
	internal static class SyntaxEmitGenerators
	{
		internal static (CodePragma, LOLMethod, ILGenerator) Create()
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

			return (pragma, method, ilGenerator);
		}
	}
}
