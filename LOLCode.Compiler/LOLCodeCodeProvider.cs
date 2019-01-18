using LOLCode.Compiler;
using LOLCode.Compiler.Emitter;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;

namespace LOLCode.Compiler
{
	public class LOLCodeCodeProvider : CodeDomProvider, ICodeCompiler
	{
		[Obsolete]
		public override ICodeCompiler CreateCompiler() => this;

		[Obsolete]
		public override ICodeGenerator CreateGenerator() => throw new Exception("The method or operation is not implemented.");

		[Obsolete]
		public override ICodeParser CreateParser() => throw new Exception("The method or operation is not implemented.");

		public CompilerResults CompileAssemblyFromDom(CompilerParameters options, System.CodeDom.CodeCompileUnit compilationUnit) => throw new Exception("The method or operation is not implemented.");

		public CompilerResults CompileAssemblyFromDomBatch(CompilerParameters options, System.CodeDom.CodeCompileUnit[] compilationUnits) => throw new Exception("The method or operation is not implemented.");

		public CompilerResults CompileAssemblyFromFile(CompilerParameters options, string fileName) => this.CompileAssemblyFromFileBatch(options, new string[] { fileName });

		public CompilerResults CompileAssemblyFromFileBatch(CompilerParameters options, string[] fileNames)
		{
			var streams = new Stream[fileNames.Length];
			for (var i = 0; i < streams.Length; i++)
			{
				streams[i] = File.OpenRead(fileNames[i]);
			}

			return this.CompileAssemblyFromStreamBatch(options, fileNames, streams);
		}

		public CompilerResults CompileAssemblyFromSource(CompilerParameters options, string source) => this.CompileAssemblyFromSourceBatch(options, new string[] { source });

		public CompilerResults CompileAssemblyFromSourceBatch(CompilerParameters options, string[] sources)
		{
			var streams = new Stream[sources.Length];
			var filenames = new string[sources.Length];
			for (var i = 0; i < streams.Length; i++)
			{
				streams[i] = new MemoryStream(Encoding.UTF8.GetBytes(sources[i]));
				filenames[i] = "unknown";
			}

			return this.CompileAssemblyFromStreamBatch(options, filenames, streams);
		}

		private CompilerResults CompileAssemblyFromStreamBatch(CompilerParameters options, string[] filenames, Stream[] streams)
		{
			var name = new AssemblyName
			{
				Name = Path.GetFileName(options.OutputAssembly)
			};

			var ab = Thread.GetDomain().DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);

			if (options.IncludeDebugInformation)
			{
				var daCtor = typeof(DebuggableAttribute).GetConstructor(new Type[] { typeof(DebuggableAttribute.DebuggingModes) });
				var daBuilder = new CustomAttributeBuilder(daCtor, new object[] { DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations });
				ab.SetCustomAttribute(daBuilder);
			}

			var mb = ab.DefineDynamicModule(Path.GetFileName(options.OutputAssembly), options.IncludeDebugInformation);

			var ret = new CompilerResults(options.TempFiles);

			var prog = new LOLProgram(options);
			for (var i = 0; i < streams.Length; i++)
			{
				if (!streams[i].CanSeek)
				{
					throw new ArgumentException("Streams passed to CompileAssemblyFromStream[Batch] must be seekable");
				}

				var pass1 = Parser.Pass1.Parser.GetParser(prog, filenames[i], streams[i], ret);
				pass1.Parse();
				if (ret.Errors.HasErrors)
				{
					return ret;
				}

				streams[i].Seek(0, SeekOrigin.Begin);

				var p = Parser.v1_2.Parser.GetParser(mb, prog, filenames[i], streams[i], ret);
				p.Parse();
				if (ret.Errors.HasErrors)
				{
					return ret;
				}
			}

			var entryMethod = prog.Emit(ret.Errors, mb);
			if (ret.Errors.HasErrors)
			{
				return ret;
			}

			ab.SetEntryPoint(entryMethod);

			if (!options.GenerateInMemory)
			{
				ab.Save(options.OutputAssembly);
			}

			ret.CompiledAssembly = ab;

			return ret;
		}
	}
}
