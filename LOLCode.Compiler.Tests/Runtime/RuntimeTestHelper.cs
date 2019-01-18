using LOLCode.Compiler;
using NUnit.Framework;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace LOLCode.net.Tests.Runtime
{
	internal class RuntimeTestHelper
	{
		internal static void TestExecuteSourcesNoInput(string source, string baseline, string testname, ExecuteMethod method) => TestExecuteSourcesNoInput(new string[] { source }, baseline, testname, method);

		internal static void TestExecuteSourcesNoInput(string[] sources, string baseline, string testname, ExecuteMethod method)
		{
			var provider = new LOLCodeCodeProvider();
			var assemblyName = string.Format("{0}.exe", testname);
			var cparam = GetDefaultCompilerParams(assemblyName, method);

			// Compile test
			var results = provider.CompileAssemblyFromSourceBatch(cparam, sources);

			// Collect errors (if any) 
			var errors = new StringBuilder();
			if (results.Errors.HasErrors)
			{
				for (var i = 0; i < results.Errors.Count; i++)
				{
					errors.AppendLine(results.Errors[i].ToString());
				}
			}

			// Ensure there are no errors before trying to execute
			Assert.AreEqual(0, results.Errors.Count, errors.ToString());

			if (method == ExecuteMethod.InMemory)
			{
				throw new NotImplementedException("In memory execution is not implimented yet");
			}
			else
			{
				// Run the executeable (collecting it's output) compare to baseline 
				Assert.AreEqual(baseline, RunExecuteable(assemblyName));
				File.Delete(assemblyName);
			}
		}

		private static string RunExecuteable(string path)
		{

			var p = new Process();
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.CreateNoWindow = true;
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.FileName = path;
			p.Start();
			p.WaitForExit();
			return p.StandardOutput.ReadToEnd();
		}


		private static CompilerParameters GetDefaultCompilerParams(string outfile, ExecuteMethod method)
		{
			var cparam = new CompilerParameters
			{
				GenerateExecutable = true,
				GenerateInMemory = (method == ExecuteMethod.InMemory) ? true : false,
				OutputAssembly = outfile,
				MainClass = "Program",
				IncludeDebugInformation = true
			};
			return cparam;
		}
	}

	internal enum ExecuteMethod
	{
		ExternalProcess,
		InMemory
	}
}
