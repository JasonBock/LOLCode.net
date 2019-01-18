using LOLCode.Compiler.Emitter;
using LOLCode.Compiler.Symbols;
using LOLCode.Compiler.Syntax;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;
using LCS = LOLCode.Compiler.Syntax;

namespace LOLCode.Compiler.Parser.v1_2
{
	internal partial class Parser
	{
		const int _EOF = 0;
		const int _ident = 1;
		const int _intCon = 2;
		const int _realCon = 3;
		const int _stringCon = 4;
		const int _eos = 5;
		const int _can = 6;
		const int _in = 7;
		const int _im = 8;
		const int _outta = 9;
		const int _mkay = 10;
		const int _r = 11;
		const int _is = 12;
		const int _how = 13;
		const int maxT = 63;
		const int _comment = 64;
		const int _blockcomment = 65;
		const int _continuation = 66;

		const bool T = true;
		const bool x = false;
		const int minErrDist = 2;

		public Scanner scanner;
		public Errors errors;

		public Token t;    // last recognized token
		public Token la;   // lookahead token
		int errDist = minErrDist;

		public Parser(Scanner scanner) => this.scanner = scanner;

		void SynErr(int n)
		{
			if (this.errDist >= minErrDist)
			{
				this.errors.SynErr(this.filename, this.la.line, this.la.col, n);
			}

			this.errDist = 0;
		}

		public void SemErr(string msg)
		{
			if (this.errDist >= minErrDist)
			{
				this.errors.SemErr(this.filename, this.t.line, this.t.col, msg);
			}

			this.errDist = 0;
		}

		void Get()
		{
			for (; ; )
			{
				this.t = this.la;
				this.la = this.scanner.Scan();
				if (this.la.kind <= maxT) { ++this.errDist; break; }
				if (this.la.kind == 64)
				{
				}
				if (this.la.kind == 65)
				{
				}
				if (this.la.kind == 66)
				{
				}

				this.la = this.t;
			}
		}

		void Expect(int n)
		{
			if (this.la.kind == n) { this.Get(); } else { this.SynErr(n); }
		}

		bool StartOf(int s) => this.set[s, this.la.kind];

		void ExpectWeak(int n, int follow)
		{
			if (this.la.kind == n)
			{
				this.Get();
			}
			else
			{
				this.SynErr(n);
				while (!this.StartOf(follow))
				{
					this.Get();
				}
			}
		}

		bool WeakSeparator(int n, int syFol, int repFol)
		{
			var s = new bool[maxT + 1];
			if (this.la.kind == n) { this.Get(); return true; }
			else if (this.StartOf(repFol))
			{
				return false;
			}
			else
			{
				for (var i = 0; i <= maxT; i++)
				{
					s[i] = this.set[syFol, i] || this.set[repFol, i] || this.set[0, i];
				}
				this.SynErr(n);
				while (!s[this.la.kind])
				{
					this.Get();
				}

				return this.StartOf(syFol);
			}
		}

		void LOLCode()
		{
			this.Expect(14);
			if (this.la.kind == 15)
			{
				this.Get();
				this.Expect(16);
				this.program.version = LOLCodeVersion.v1_2;
			}
			while (this.la.kind == 5)
			{
				this.Get();
			}
			this.Statements(out this.program.methods["Main"].statements);
			this.Expect(17);
			while (this.la.kind == 5)
			{
				this.Get();
			}
		}

		void Statements(out Statement stat)
		{
			var bs = new BlockStatement(this.GetPragma(this.t)); stat = bs;
			while ((this.StartOf(1) || this.la.kind == _can || this.la.kind == _how) && this.scanner.Peek().kind != _outta)
			{
				if (this.la.kind == 6)
				{
					this.CanHasStatement();
				}
				else if (this.la.kind == 13)
				{
					this.FunctionDeclaration();
				}
				else if (this.StartOf(1))
				{
					this.Statement(out var s);
					bs.statements.Add(s);
				}
				else
				{
					this.SynErr(64);
				}

				while (this.la.kind == 5)
				{
					this.Get();
				}
			}
		}

		void CanHasStatement()
		{
			var sb = new StringBuilder();
			this.Expect(6);
			this.Expect(19);
			this.Expect(1);
			sb.Append(this.t.val);
			while (this.la.kind == 22)
			{
				this.Get();
				this.Expect(1);
				sb.Append('.'); sb.Append(this.t.val);
			}
			this.Expect(23);
			if (!this.program.ImportLibrary(sb.ToString()))
			{
				this.Error($"Library \"{sb.ToString()}\" not found.");
			}
		}

		void FunctionDeclaration()
		{
			if (this.currentMethod != null)
			{
				this.Error("Cannot define a function inside another function.");
			}

			this.Expect(13);
			this.Expect(57);
			this.Expect(18);
			this.Expect(1);
			this.currentMethod = new LOLMethod(this.GetFunction(this.t.val), this.program); short arg = 0;
			if (this.la.kind == 30)
			{
				this.Get();
				this.Expect(1);
				this.currentMethod.SetArgumentName(arg++, this.t.val);
				while (this.la.kind == 48)
				{
					this.Get();
					this.Expect(30);
					this.Expect(1);
					this.currentMethod.SetArgumentName(arg++, this.t.val);
				}
			}
			while (this.la.kind == 5)
			{
				this.Get();
			}
			this.Statements(out this.currentMethod.statements);
			this.Expect(58);
			this.Expect(59);
			this.Expect(60);
			this.Expect(61);
			this.program.methods.Add(this.currentMethod.info.Name, this.currentMethod); this.currentMethod = null;
		}

		void Statement(out Statement stat)
		{
			stat = null;
			if (this.la.kind == 18)
			{
				this.IHasAStatement(out stat);
			}
			else if (this.TokenAfterLValue().kind == _r)
			{
				this.AssignmentStatement(out stat);
			}
			else if (this.TokenAfterLValue().kind == _is)
			{
				this.TypecastStatement(out stat);
			}
			else if (this.la.kind == 24)
			{
				this.GimmehStatement(out stat);
			}
			else if (this.la.kind == 8)
			{
				this.LoopStatement(out stat);
			}
			else if (this.la.kind == 28)
			{
				this.BreakStatement(out stat);
			}
			else if (this.la.kind == 29)
			{
				this.ContinueStatement(out stat);
			}
			else if (this.la.kind == 33)
			{
				this.OrlyStatement(out stat);
			}
			else if (this.la.kind == 40)
			{
				this.SwitchStatement(out stat);
			}
			else if (this.la.kind == 43 || this.la.kind == 44)
			{
				this.PrintStatement(out stat);
			}
			else if (this.StartOf(2))
			{
				this.ExpressionStatement(out stat);
			}
			else if (this.la.kind == 62)
			{
				this.ReturnStatement(out stat);
			}
			else
			{
				this.SynErr(65);
			}
		}

		void IHasAStatement(out Statement stat)
		{
			var vds = new VariableDeclarationStatement(this.GetPragma(this.la)); stat = vds;
			this.Expect(18);
			this.Expect(19);
			this.Expect(20);
			this.Expect(1);
			vds.var = this.DeclareVariable(this.t.val); this.SetEndPragma(stat);
			if (this.la.kind == 21)
			{
				this.Get();
				this.Expression(out vds.expression);
			}
		}

		void AssignmentStatement(out Statement stat)
		{
			var ass = new AssignmentStatement(this.GetPragma(this.la)); stat = ass;
			this.LValue(out ass.lval);
			this.Expect(11);
			this.Expression(out ass.rval);
			this.SetEndPragma(stat);
		}

		void TypecastStatement(out Statement stat)
		{
			var ass = new AssignmentStatement(this.GetPragma(this.la));
			stat = ass;
			this.LValue(out ass.lval);
			this.Expect(12);
			this.Expect(46);
			this.Expect(20);
			this.Typename(out var t);
			this.SetEndPragma(ass);
			ass.rval = new TypecastExpression(ass.location, t, ass.lval);
		}

		void GimmehStatement(out Statement stat)
		{
			var ins = new InputStatement(this.GetPragma(this.la)); stat = ins;
			this.Expect(24);
			if (this.la.kind == 25 || this.la.kind == 26 || this.la.kind == 27)
			{
				if (this.la.kind == 25)
				{
					this.Get();
				}
				else if (this.la.kind == 26)
				{
					this.Get();
					ins.amount = IOAmount.Word;
				}
				else
				{
					this.Get();
					ins.amount = IOAmount.Letter;
				}
			}
			this.LValue(out ins.dest);
			this.SetEndPragma(stat);
		}

		void LoopStatement(out Statement stat)
		{
			var ls = new LoopStatement(this.GetPragma(this.la)); stat = ls;
			this.Expect(8);
			this.Expect(7);
			this.Expect(30);
			this.Expect(1);
			ls.name = this.t.val; this.BeginScope();
			if (this.la.kind == 1)
			{
				ls.StartOperation(this.GetPragma(this.la));
				this.Get();
				ls.SetOperationFunction(this.GetFunction(this.t.val));
				if (this.la.kind == 30)
				{
					this.Get();
				}
				this.Expect(1);
				ls.SetLoopVariable(this.GetPragma(this.t), this.GetVariable(this.t.val)); this.SetEndPragma(ls.operation); this.SetEndPragma(((ls.operation as AssignmentStatement).rval as FunctionExpression).arguments[0]); this.SetEndPragma((ls.operation as AssignmentStatement).rval); this.SetEndPragma((ls.operation as AssignmentStatement).lval);
			}
			if (this.la.kind == 31 || this.la.kind == 32)
			{
				if (this.la.kind == 31)
				{
					this.Get();
					ls.type = LoopType.Until;
				}
				else
				{
					this.Get();
					ls.type = LoopType.While;
				}
				this.Expression(out ls.condition);
			}
			this.SetEndPragma(stat);
			this.Expect(5);
			while (this.la.kind == 5)
			{
				this.Get();
			}
			this.Statements(out ls.statements);
			this.Expect(8);
			this.Expect(9);
			this.Expect(30);
			this.Expect(1);
			if (this.t.val != ls.name)
			{
				this.Error("Loop terminator label does not match loop label");
			}

			this.EndScope();
		}

		void BreakStatement(out Statement stat)
		{
			var bs = new BreakStatement(this.GetPragma(this.la)); stat = bs;
			this.Expect(28);
			if (this.la.kind == 1)
			{
				this.Get();
				bs.label = this.t.val;
			}
			this.Expect(5);
			this.SetEndPragma(stat);
		}

		void ContinueStatement(out Statement stat)
		{
			var cs = new ContinueStatement(this.GetPragma(this.la)); stat = cs;
			this.Expect(29);
			if (this.la.kind == 1)
			{
				this.Get();
				cs.label = this.t.val;
			}
			this.Expect(5);
			this.SetEndPragma(stat);
		}

		void OrlyStatement(out Statement stat)
		{
			var cs = new ConditionalStatement(this.GetPragma(this.la));
			stat = cs;
			var cur = cs;
			cs.condition = new VariableLValue(this.GetPragma(this.la), this.GetVariable("IT"));
			this.Expect(33);
			this.Expect(34);
			this.Expect(23);
			this.SetEndPragma(cs);
			while (this.la.kind == 5)
			{
				this.Get();
			}
			if (this.la.kind == 35)
			{
				this.Get();
				this.Expect(34);
				while (this.la.kind == 5)
				{
					this.Get();
				}
			}
			this.BeginScope();
			this.Statements(out cs.trueStatements);
			this.EndScope();
			while (this.la.kind == 36)
			{
				this.Get();
				cur.falseStatements = new ConditionalStatement(this.GetPragma(this.la));
				this.Expression(out var e);
				(cur.falseStatements as ConditionalStatement).condition = e; this.SetEndPragma(cur.falseStatements); cur = (ConditionalStatement)cur.falseStatements; this.BeginScope();
				while (this.la.kind == 5)
				{
					this.Get();
				}
				this.Statements(out cur.trueStatements);
				this.EndScope();
			}
			if (this.la.kind == 37)
			{
				this.Get();
				this.Expect(38);
				while (this.la.kind == 5)
				{
					this.Get();
				}
				this.BeginScope();
				this.Statements(out cur.falseStatements);
				this.EndScope();
			}
			this.Expect(39);
		}

		void SwitchStatement(out Statement stat)
		{
			var ss = new SwitchStatement(this.GetPragma(this.la))
			{
				control = new VariableLValue(this.GetPragma(this.la), this.GetVariable("IT"))
			};
			stat = ss;
			this.Expect(40);
			this.Expect(23);
			this.SetEndPragma(ss);
			while (this.la.kind == 5)
			{
				this.Get();
			}
			while (this.la.kind == 41)
			{
				this.Get();
				this.SwitchLabel(out var label);
				while (this.la.kind == 5)
				{
					this.Get();
				}
				this.Statements(out var block);
				this.AddCase(ss, label, block);
			}
			if (this.la.kind == 42)
			{
				this.Get();
				while (this.la.kind == 5)
				{
					this.Get();
				}
				this.Statements(out ss.defaultCase);
			}
			this.Expect(39);
		}

		void PrintStatement(out Statement stat)
		{
			var ps = new PrintStatement(this.GetPragma(this.la))
			{
				message = new FunctionExpression(this.GetPragma(this.la), this.GetFunction("SMOOSH"))
			};
			stat = ps;
			if (this.la.kind == 43)
			{
				this.Get();
			}
			else if (this.la.kind == 44)
			{
				this.Get();
				ps.stderr = true;
			}
			else
			{
				this.SynErr(66);
			}

			while (this.StartOf(2))
			{
				this.Expression(out var e);
				(ps.message as FunctionExpression).arguments.Add(e);
			}
			if (this.la.kind == 45)
			{
				this.Get();
				ps.newline = false;
			}
			this.Expect(5);
			this.SetEndPragma(stat);
		}

		void ExpressionStatement(out Statement stat)
		{
			var ass = new AssignmentStatement(this.GetPragma(this.la))
			{
				lval = new VariableLValue(this.GetPragma(this.la), this.GetVariable("IT"))
			};
			stat = ass;
			this.Expression(out var exp);
			ass.rval = exp;
			this.SetEndPragma(ass);
		}

		void ReturnStatement(out Statement stat)
		{
			var rs = new ReturnStatement(this.GetPragma(this.la)); stat = rs;
			this.Expect(62);
			this.Expect(30);
			this.Expression(out rs.expression);
		}

		void Expression(out Expression exp)
		{
			exp = null;
			if (this.IsFunction(this.la.val))
			{
				this.FunctionExpression(out exp);
			}
			else if (this.la.kind == 49)
			{
				this.TypecastExpression(out exp);
			}
			else if (this.StartOf(3))
			{
				this.Unary(out exp);
			}
			else
			{
				this.SynErr(67);
			}
		}

		void LValue(out LValue lv)
		{
			lv = null;
			this.Expect(1);
			lv = new VariableLValue(this.GetPragma(this.t), this.GetVariable(this.t.val)); this.SetEndPragma(lv);
		}

		void SwitchLabel(out object obj)
		{
			obj = null;
			if (this.StartOf(4))
			{
				this.Const(out obj);
			}
			else if (this.la.kind == 4)
			{
				this.Get();
				var refs = new List<VariableRef>();
				obj = LCS.StringExpression.UnescapeString(this.t.val, this.GetScope(), this.errors, this.GetPragma(this.t), refs);
				if (refs.Count > 0)
				{
					this.Error("String constants in OMG labels cannot contain variable substitutions.");
				}
			}
			else
			{
				this.SynErr(68);
			}
		}

		void Const(out object val)
		{
			val = null;
			if (this.la.kind == 2)
			{
				this.Get();
				val = int.Parse(this.t.val);
			}
			else if (this.la.kind == 3)
			{
				this.Get();
				val = float.Parse(this.t.val);
			}
			else if (this.la.kind == 54)
			{
				this.Get();
				val = null;
			}
			else if (this.la.kind == 55)
			{
				this.Get();
				val = true;
			}
			else if (this.la.kind == 56)
			{
				this.Get();
				val = false;
			}
			else
			{
				this.SynErr(69);
			}
		}

		void Typename(out Type t)
		{
			t = typeof(void);
			if (this.la.kind == 50)
			{
				this.Get();
				t = typeof(bool);
			}
			else if (this.la.kind == 51)
			{
				this.Get();
				t = typeof(int);
			}
			else if (this.la.kind == 52)
			{
				this.Get();
				t = typeof(float);
			}
			else if (this.la.kind == 53)
			{
				this.Get();
				t = typeof(string);
			}
			else
			{
				this.SynErr(70);
			}
		}

		void FunctionExpression(out Expression exp)
		{
			var fe = new FunctionExpression(this.GetPragma(this.la)); exp = fe;
			this.Expect(1);
			fe.func = this.GetFunction(this.t.val); var argsLeft = fe.func.Arity;
			if (this.la.kind == 47)
			{
				this.Get();
			}
			if ((argsLeft > 0 || (fe.func.IsVariadic && this.la.kind != _mkay)) && this.la.kind != _eos)
			{
				this.Expression(out var e2);
				fe.arguments.Add(e2); argsLeft--;
				while ((argsLeft > 0 || (fe.func.IsVariadic && this.la.kind != _mkay)) && this.la.kind != _eos)
				{
					if (this.la.kind == 48)
					{
						this.Get();
					}
					this.Expression(out e2);
					fe.arguments.Add(e2); argsLeft--;
				}
			}
			if (fe.func.IsVariadic && this.la.kind != _eos)
			{
				this.Expect(10);
			}
		}

		void TypecastExpression(out Expression exp)
		{
			var te = new TypecastExpression(this.GetPragma(this.la));
			exp = te;
			this.Expect(49);
			this.Expression(out te.exp);
			if (this.la.kind == 20)
			{
				this.Get();
			}
			this.Typename(out te.destType);
			this.SetEndPragma(te);
		}

		void Unary(out Expression exp)
		{
			exp = null;
			if (this.StartOf(4))
			{
				this.Const(out var val);
				exp = new PrimitiveExpression(this.GetPragma(this.t), val);
				this.SetEndPragma(exp);
			}
			else if (this.la.kind == 4)
			{
				this.StringExpression(out exp);
			}
			else if (this.la.kind == 1)
			{
				this.LValue(out var lv);
				exp = lv;
				this.SetEndPragma(exp);
			}
			else
			{
				this.SynErr(71);
			}
		}

		void StringExpression(out Expression exp)
		{
			this.Expect(4);
			exp = new StringExpression(this.GetPragma(this.t), this.t.val, this.GetScope(), this.errors); this.SetEndPragma(exp);
		}



		public void Parse()
		{
			this.la = new Token
			{
				val = ""
			};
			this.Get();
			this.LOLCode();

			this.Expect(0);
		}

		bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,x,x,x, T,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, T,x,x,x, T,T,x,x, x,T,x,x, x,x,x,x, T,x,x,T, T,x,x,x, x,T,x,x, x,x,T,T, T,x,x,x, x,x,T,x, x},
		{x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,T,T, T,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,x,x,x, x,x,x,x, x},
		{x,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,x,x,x, x,x,x,x, x}

	};
	} // end Parser


	public class Errors
	{
		private readonly CompilerErrorCollection cec;

		public Errors(CompilerErrorCollection c) => this.cec = c;

		public void SynErr(string file, int line, int col, int n)
		{
			string s;
			switch (n)
			{
				case 0: s = "EOF expected"; break;
				case 1: s = "ident expected"; break;
				case 2: s = "intCon expected"; break;
				case 3: s = "realCon expected"; break;
				case 4: s = "stringCon expected"; break;
				case 5: s = "eos expected"; break;
				case 6: s = "can expected"; break;
				case 7: s = "in expected"; break;
				case 8: s = "im expected"; break;
				case 9: s = "outta expected"; break;
				case 10: s = "mkay expected"; break;
				case 11: s = "r expected"; break;
				case 12: s = "is expected"; break;
				case 13: s = "how expected"; break;
				case 14: s = "\"HAI\" expected"; break;
				case 15: s = "\"TO\" expected"; break;
				case 16: s = "\"1.2\" expected"; break;
				case 17: s = "\"KTHXBYE\" expected"; break;
				case 18: s = "\"I\" expected"; break;
				case 19: s = "\"HAS\" expected"; break;
				case 20: s = "\"A\" expected"; break;
				case 21: s = "\"ITZ\" expected"; break;
				case 22: s = "\".\" expected"; break;
				case 23: s = "\"?\" expected"; break;
				case 24: s = "\"GIMMEH\" expected"; break;
				case 25: s = "\"LINE\" expected"; break;
				case 26: s = "\"WORD\" expected"; break;
				case 27: s = "\"LETTAR\" expected"; break;
				case 28: s = "\"GTFO\" expected"; break;
				case 29: s = "\"MOAR\" expected"; break;
				case 30: s = "\"YR\" expected"; break;
				case 31: s = "\"TIL\" expected"; break;
				case 32: s = "\"WILE\" expected"; break;
				case 33: s = "\"O\" expected"; break;
				case 34: s = "\"RLY\" expected"; break;
				case 35: s = "\"YA\" expected"; break;
				case 36: s = "\"MEBBE\" expected"; break;
				case 37: s = "\"NO\" expected"; break;
				case 38: s = "\"WAI\" expected"; break;
				case 39: s = "\"OIC\" expected"; break;
				case 40: s = "\"WTF\" expected"; break;
				case 41: s = "\"OMG\" expected"; break;
				case 42: s = "\"OMGWTF\" expected"; break;
				case 43: s = "\"VISIBLE\" expected"; break;
				case 44: s = "\"INVISIBLE\" expected"; break;
				case 45: s = "\"!\" expected"; break;
				case 46: s = "\"NOW\" expected"; break;
				case 47: s = "\"OF\" expected"; break;
				case 48: s = "\"AN\" expected"; break;
				case 49: s = "\"MAEK\" expected"; break;
				case 50: s = "\"TROOF\" expected"; break;
				case 51: s = "\"NUMBR\" expected"; break;
				case 52: s = "\"NUMBAR\" expected"; break;
				case 53: s = "\"YARN\" expected"; break;
				case 54: s = "\"NOOB\" expected"; break;
				case 55: s = "\"WIN\" expected"; break;
				case 56: s = "\"FAIL\" expected"; break;
				case 57: s = "\"DUZ\" expected"; break;
				case 58: s = "\"IF\" expected"; break;
				case 59: s = "\"U\" expected"; break;
				case 60: s = "\"SAY\" expected"; break;
				case 61: s = "\"SO\" expected"; break;
				case 62: s = "\"FOUND\" expected"; break;
				case 63: s = "??? expected"; break;
				case 64: s = "invalid Statements"; break;
				case 65: s = "invalid Statement"; break;
				case 66: s = "invalid PrintStatement"; break;
				case 67: s = "invalid Expression"; break;
				case 68: s = "invalid SwitchLabel"; break;
				case 69: s = "invalid Const"; break;
				case 70: s = "invalid Typename"; break;
				case 71: s = "invalid Unary"; break;

				default: s = "error " + n; break;
			}
			this.cec.Add(new CompilerError(file, line, col, n.ToString(), s));
		}

		public void SemErr(string file, int line, int col, string s) => this.cec.Add(new CompilerError(file, line, col, "", s));

		public void SemErr(string s) => this.cec.Add(new CompilerError(null, 0, 0, "", s));

		public void Warning(string file, int line, int col, string s)
		{
			var ce = new CompilerError(file, line, col, "", s)
			{
				IsWarning = true
			};
			this.cec.Add(ce);
		}

		public void Warning(string s)
		{
			var ce = new CompilerError(null, 0, 0, "", s)
			{
				IsWarning = true
			};
			this.cec.Add(ce);
		}
	} // Errors


	public class FatalError : Exception
	{
		public FatalError(string m) : base(m) { }
	}

}