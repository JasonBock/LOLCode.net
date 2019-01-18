using System;
using System.CodeDom.Compiler;

namespace LOLCode.Compiler.Parser.Pass1
{
	internal partial class Parser
	{
		const int _EOF = 0;
		const int _ident = 1;
		const int _intCon = 2;
		const int _realCon = 3;
		const int _stringCon = 4;
		const int _eos = 5;
		const int maxT = 78;
		const int _comment = 79;
		const int _blockcomment = 80;
		const int _continuation = 81;

		const bool T = true;
		const bool x = false;
		const int minErrDist = 2;

		public Scanner scanner;
		public Errors errors;

		public Token t;    // last recognized token
		public Token la;   // lookahead token
		int errDist = minErrDist;



		public Parser(Scanner scanner) => this.scanner = scanner;//errors = new Errors();

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
				if (this.la.kind == 79)
				{
				}
				if (this.la.kind == 80)
				{
				}
				if (this.la.kind == 81)
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
			this.Expect(6);
			if (this.la.kind == 7)
			{
				this.Get();
				if (this.la.kind == 8)
				{
					this.Get();
					this.version = LOLCodeVersion.v1_0;
				}
				else if (this.la.kind == 9)
				{
					this.Get();
					this.version = LOLCodeVersion.IRCSPECZ;
				}
				else if (this.la.kind == 10)
				{
					this.Get();
					this.version = LOLCodeVersion.v1_1;
				}
				else if (this.la.kind == 11)
				{
					this.Get();
					this.version = LOLCodeVersion.v1_2;
				}
				else
				{
					this.SynErr(79);
				}
			}
			while (this.la.kind == 5)
			{
				this.Get();
			}
			this.Statements();
			this.Expect(12);
			while (this.la.kind == 5)
			{
				this.Get();
			}
		}

		void Statements()
		{
			while (this.StartOf(1))
			{
				if (this.la.kind == 13)
				{
					this.FunctionDeclaration();
				}
				else if (this.la.kind == 15)
				{
					this.VarDecl();
				}
				else
				{
					this.OtherStatement();
				}
				this.Expect(5);
				while (this.la.kind == 5)
				{
					this.Get();
				}
			}
		}

		void FunctionDeclaration()
		{
			var arity = 0; string name;
			this.Expect(13);
			this.Expect(14);
			this.Expect(15);
			this.Expect(1);
			name = this.t.val;
			if (this.la.kind == 16)
			{
				this.Get();
				this.Expect(1);
				arity++;
				while (this.la.kind == 17)
				{
					this.Get();
					this.Expect(16);
					this.Expect(1);
					arity++;
				}
			}
			while (this.la.kind == 5)
			{
				this.Get();
			}
			while (this.StartOf(2))
			{
				if (this.la.kind == 15)
				{
					this.VarDecl();
				}
				else
				{
					this.OtherStatement();
				}
				this.Expect(5);
				while (this.la.kind == 5)
				{
					this.Get();
				}
			}
			this.Expect(18);
			this.Expect(19);
			this.Expect(20);
			this.Expect(21);
			this.globals.AddSymbol(new UserFunctionRef(name, arity, false));
		}

		void VarDecl()
		{
			this.Expect(15);
			this.Expect(22);
			this.Expect(23);
			this.Expect(1);
			if (this.la.kind == 24)
			{
				this.Get();
				this.OtherStatement();
			}
		}

		void OtherStatement()
		{
			if (this.la.kind == 1)
			{
				this.Get();
			}
			else if (this.la.kind == 2)
			{
				this.Get();
			}
			else if (this.la.kind == 3)
			{
				this.Get();
			}
			else if (this.la.kind == 4)
			{
				this.Get();
			}
			else if (this.StartOf(3))
			{
				this.Keyword();
			}
			else
			{
				this.SynErr(80);
			}

			while (this.StartOf(4))
			{
				if (this.la.kind == 1)
				{
					this.Get();
				}
				else if (this.la.kind == 2)
				{
					this.Get();
				}
				else if (this.la.kind == 3)
				{
					this.Get();
				}
				else if (this.la.kind == 4)
				{
					this.Get();
				}
				else
				{
					this.Keyword();
				}
			}
		}

		void Keyword()
		{
			switch (this.la.kind)
			{
				case 25:
					{
						this.Get();
						break;
					}
				case 22:
					{
						this.Get();
						break;
					}
				case 26:
					{
						this.Get();
						break;
					}
				case 27:
					{
						this.Get();
						break;
					}
				case 28:
					{
						this.Get();
						break;
					}
				case 29:
					{
						this.Get();
						break;
					}
				case 30:
					{
						this.Get();
						break;
					}
				case 31:
					{
						this.Get();
						break;
					}
				case 32:
					{
						this.Get();
						break;
					}
				case 33:
					{
						this.Get();
						break;
					}
				case 16:
					{
						this.Get();
						break;
					}
				case 34:
					{
						this.Get();
						break;
					}
				case 35:
					{
						this.Get();
						break;
					}
				case 23:
					{
						this.Get();
						break;
					}
				case 24:
					{
						this.Get();
						break;
					}
				case 17:
					{
						this.Get();
						break;
					}
				case 36:
					{
						this.Get();
						break;
					}
				case 37:
					{
						this.Get();
						break;
					}
				case 38:
					{
						this.Get();
						break;
					}
				case 39:
					{
						this.Get();
						break;
					}
				case 40:
					{
						this.Get();
						break;
					}
				case 41:
					{
						this.Get();
						break;
					}
				case 42:
					{
						this.Get();
						break;
					}
				case 43:
					{
						this.Get();
						break;
					}
				case 44:
					{
						this.Get();
						break;
					}
				case 45:
					{
						this.Get();
						break;
					}
				case 46:
					{
						this.Get();
						break;
					}
				case 47:
					{
						this.Get();
						break;
					}
				case 48:
					{
						this.Get();
						break;
					}
				case 49:
					{
						this.Get();
						break;
					}
				case 50:
					{
						this.Get();
						break;
					}
				case 51:
					{
						this.Get();
						break;
					}
				case 52:
					{
						this.Get();
						break;
					}
				case 53:
					{
						this.Get();
						break;
					}
				case 54:
					{
						this.Get();
						break;
					}
				case 55:
					{
						this.Get();
						break;
					}
				case 56:
					{
						this.Get();
						break;
					}
				case 57:
					{
						this.Get();
						break;
					}
				case 58:
					{
						this.Get();
						break;
					}
				case 59:
					{
						this.Get();
						break;
					}
				case 60:
					{
						this.Get();
						break;
					}
				case 61:
					{
						this.Get();
						break;
					}
				case 62:
					{
						this.Get();
						break;
					}
				case 63:
					{
						this.Get();
						break;
					}
				case 64:
					{
						this.Get();
						break;
					}
				case 65:
					{
						this.Get();
						break;
					}
				case 66:
					{
						this.Get();
						break;
					}
				case 67:
					{
						this.Get();
						break;
					}
				case 68:
					{
						this.Get();
						break;
					}
				case 69:
					{
						this.Get();
						break;
					}
				case 70:
					{
						this.Get();
						break;
					}
				case 71:
					{
						this.Get();
						break;
					}
				case 72:
					{
						this.Get();
						break;
					}
				case 73:
					{
						this.Get();
						break;
					}
				case 74:
					{
						this.Get();
						break;
					}
				case 75:
					{
						this.Get();
						break;
					}
				case 76:
					{
						this.Get();
						break;
					}
				case 77:
					{
						this.Get();
						break;
					}
				default: this.SynErr(81); break;
			}
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
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,T, T,x,x,x, x,x,x,x, x,T,x,T, T,T,x,x, x,x,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x,x},
		{x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,T, T,T,x,x, x,x,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x,x},
		{x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x,x}

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
				case 6: s = "\"HAI\" expected"; break;
				case 7: s = "\"TO\" expected"; break;
				case 8: s = "\"1.0\" expected"; break;
				case 9: s = "\"IRCSPECZ\" expected"; break;
				case 10: s = "\"1.1\" expected"; break;
				case 11: s = "\"1.2\" expected"; break;
				case 12: s = "\"KTHXBYE\" expected"; break;
				case 13: s = "\"HOW\" expected"; break;
				case 14: s = "\"DUZ\" expected"; break;
				case 15: s = "\"I\" expected"; break;
				case 16: s = "\"YR\" expected"; break;
				case 17: s = "\"AN\" expected"; break;
				case 18: s = "\"IF\" expected"; break;
				case 19: s = "\"U\" expected"; break;
				case 20: s = "\"SAY\" expected"; break;
				case 21: s = "\"SO\" expected"; break;
				case 22: s = "\"HAS\" expected"; break;
				case 23: s = "\"A\" expected"; break;
				case 24: s = "\"ITZ\" expected"; break;
				case 25: s = "\"CAN\" expected"; break;
				case 26: s = "\"?\" expected"; break;
				case 27: s = "\"GIMMEH\" expected"; break;
				case 28: s = "\"LINE\" expected"; break;
				case 29: s = "\"WORD\" expected"; break;
				case 30: s = "\"LETTAR\" expected"; break;
				case 31: s = "\"GTFO\" expected"; break;
				case 32: s = "\"ENUF\" expected"; break;
				case 33: s = "\"OV\" expected"; break;
				case 34: s = "\"UR\" expected"; break;
				case 35: s = "\"MOAR\" expected"; break;
				case 36: s = "\"IM\" expected"; break;
				case 37: s = "\"IN\" expected"; break;
				case 38: s = "\"KTHX\" expected"; break;
				case 39: s = "\"OUTTA\" expected"; break;
				case 40: s = "\"UPZ\" expected"; break;
				case 41: s = "\"NERFZ\" expected"; break;
				case 42: s = "\"TIEMZD\" expected"; break;
				case 43: s = "\"OVARZ\" expected"; break;
				case 44: s = "\"!!\" expected"; break;
				case 45: s = "\"IZ\" expected"; break;
				case 46: s = "\"YARLY\" expected"; break;
				case 47: s = "\"MEBBE\" expected"; break;
				case 48: s = "\"NOWAI\" expected"; break;
				case 49: s = "\"WTF\" expected"; break;
				case 50: s = "\"OMG\" expected"; break;
				case 51: s = "\"OMGWTF\" expected"; break;
				case 52: s = "\"BYES\" expected"; break;
				case 53: s = "\"DIAF\" expected"; break;
				case 54: s = "\"VISIBLE\" expected"; break;
				case 55: s = "\"INVISIBLE\" expected"; break;
				case 56: s = "\"!\" expected"; break;
				case 57: s = "\"LOL\" expected"; break;
				case 58: s = "\"R\" expected"; break;
				case 59: s = "\"AND\" expected"; break;
				case 60: s = "\"XOR\" expected"; break;
				case 61: s = "\"OR\" expected"; break;
				case 62: s = "\"NOT\" expected"; break;
				case 63: s = "\"BIGR\" expected"; break;
				case 64: s = "\"THAN\" expected"; break;
				case 65: s = "\"SMALR\" expected"; break;
				case 66: s = "\"LIEK\" expected"; break;
				case 67: s = "\"UP\" expected"; break;
				case 68: s = "\"NERF\" expected"; break;
				case 69: s = "\"TIEMZ\" expected"; break;
				case 70: s = "\"OVAR\" expected"; break;
				case 71: s = "\"NOOB\" expected"; break;
				case 72: s = "\"MAH\" expected"; break;
				case 73: s = "\"OF\" expected"; break;
				case 74: s = "\"MAEK\" expected"; break;
				case 75: s = "\"IS\" expected"; break;
				case 76: s = "\"NOW\" expected"; break;
				case 77: s = "\"FOUND\" expected"; break;
				case 78: s = "??? expected"; break;
				case 79: s = "invalid LOLCode"; break;
				case 80: s = "invalid OtherStatement"; break;
				case 81: s = "invalid Keyword"; break;

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