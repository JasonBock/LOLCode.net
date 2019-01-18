using LOLCode.Compiler;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Reflection.Emit;

namespace notdot.LOLCode.Parser.v1_2
{
	internal partial class Parser
	{
		public static Parser GetParser(ModuleBuilder mb, LOLProgram prog, string filename, Stream s, CompilerResults cr)
		{
			var p = new Parser(new Scanner(s))
			{
				filename = Path.GetFileName(filename)
			};
			if (prog.compileropts.IncludeDebugInformation)
			{
				p.doc = mb.DefineDocument(p.filename, Guid.Empty, Guid.Empty, Guid.Empty);
			}
			else
			{
				//Not a debug build
				p.doc = null;
			}

			p.program = prog;
			p.errors = new Errors(cr.Errors);
			p.main = prog.methods["Main"];

			return p;
		}

		private string filename;
		private ISymbolDocumentWriter doc;
		private LOLProgram program;
		private LOLMethod main;
		private LOLMethod currentMethod = null;

		private bool IsArrayIndex() => this.scanner.Peek().kind == _in;

		private CodePragma GetPragma(Token tok) => new CodePragma(this.doc, this.filename, tok.line, tok.col);

		private void SetEndPragma(CodeObject co)
		{
			if (co == null)
			{
				//We encountered an error - we can ignore this, since it will result in a compiler error
				return;
			}

			co.location.endLine = this.t.line;
			co.location.endColumn = this.t.col + this.t.val.Length;
		}

		private void BeginScope()
		{
		}

		private void EndScope()
		{
		}

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

		private Scope GetScope()
		{
			if (this.currentMethod == null)
			{
				return this.main.locals;
			}
			else
			{
				return this.currentMethod.locals;
			}
		}

		private VariableRef DeclareVariable(string name)
		{
			VariableRef ret;
			if (this.currentMethod == null)
			{
				ret = new LocalRef(name);
				this.main.locals.AddSymbol(ret);
			}
			else
			{
				ret = new LocalRef(name);
				this.currentMethod.locals.AddSymbol(ret);
			}

			return ret;
		}

		private FunctionRef GetFunction(string name)
		{
			var ret = this.GetScope()[name];
			if (ret == null)
			{
				this.Error(string.Format("Unknown function: \"{0}\"", name));
			}

			if (!(ret is FunctionRef))
			{
				this.Error(string.Format("{0} is a variable, but is used like a function", name));
			}

			return ret as FunctionRef;
		}

		private bool IsFunction(string name) => this.GetScope()[name] is FunctionRef;

		private VariableRef GetVariable(string name)
		{
			var ret = this.GetScope()[name];
			if (ret == null)
			{
				this.Error(string.Format("Unknown variable: \"{0}\"", name));
			}

			if (!(ret is VariableRef))
			{
				this.Error(string.Format("{0} is a function, but is used like a variable", name));
			}

			return ret as VariableRef;
		}

		private void AddCase(SwitchStatement ss, object label, Statement block) => ss.cases.Add(new SwitchStatement.Case(label, block));

		private Token TokenAfterLValue()
		{
			this.scanner.ResetPeek();
			return this.scanner.Peek();
		}
	}
}
