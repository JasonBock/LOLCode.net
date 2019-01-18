using LOLCode.Compiler;
using LOLCode.Compiler.Symbols;
using System.CodeDom.Compiler;
using System.IO;

namespace LOLCode.Compiler.Parser.Pass1
{
	internal partial class Parser
	{
		public static Parser GetParser(LOLProgram prog, string filename, Stream s, CompilerResults results)
		{
			var p = new Parser(new Scanner(s))
			{
				filename = Path.GetFileName(filename),
				errors = new Errors(results.Errors),
				globals = prog.globals
			};

			return p;
		}

		private string filename;
		public LOLCodeVersion version = LOLCodeVersion.v1_2;
		public Scope globals;

		void Error(string s)
		{
			if (this.errDist >= minErrDist)
			{
				this.errors.SemErr(this.filename, this.t.line, this.t.col, s);
			}

			this.errDist = 0;
		}

		void Warning(string s)
		{
			if (this.errDist >= minErrDist)
			{
				this.errors.Warning(this.filename, this.t.line, this.t.col, s);
			}

			this.errDist = 0;
		}

	}

}
