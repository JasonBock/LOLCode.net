using LOLCode.Compiler;
using System;
using System.CodeDom.Compiler;
using System.IO;

namespace LOLCode
{
	class Program
	{
		static int Main(string[] args)
		{
			var arguments = new LolCompilerArguments();

			if (!Parser.ParseArgumentsWithUsage(args, arguments))
			{
				return 2;
			}

			// Ensure some goodness in the arguments
			if (!LolCompilerArguments.PostValidateArguments(arguments))
			{
				// Errors are output by PostValidateArguments to STDERR
				return 3;
			}

			// Warn the user if there is more than one source file, as they will be ignored (for now) 
			// TODO: Should be removed eventually
			if (arguments.sources.Length > 1)
			{
				Console.Error.WriteLine("lolc warning: More than one source file specifed. Only '{0}' will be compiled.", arguments.sources[0]);
			}

			// Good to go
			var outfileFile = string.IsNullOrEmpty(arguments.output) ? Path.ChangeExtension(arguments.sources[0], ".exe") : arguments.output;
			var compiler = new LOLCodeCodeProvider();
			var cparam = new CompilerParameters
			{
				GenerateExecutable = true,
				GenerateInMemory = false,
				OutputAssembly = outfileFile,
				MainClass = "Program",
				IncludeDebugInformation = arguments.debug
			};
			cparam.ReferencedAssemblies.AddRange(arguments.references);
			var results = compiler.CompileAssemblyFromFile(cparam, arguments.sources[0]);

			for (var i = 0; i < results.Errors.Count; i++)
			{
				Console.Error.WriteLine(results.Errors[i].ToString());
			}

			if (results.Errors.HasErrors)
			{
				Console.Out.WriteLine("Failed to compile.");
				return 1;
			}
			else
			{
				Console.Out.WriteLine("Successfully compiled.");
				return 0;
			}
		}
	}
}
