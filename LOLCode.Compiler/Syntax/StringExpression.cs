using LOLCode.Compiler.Emitter;
using LOLCode.Compiler.Parser.v1_2;
using LOLCode.Compiler.Symbols;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace LOLCode.Compiler.Syntax
{
	internal class StringExpression 
		: Expression
	{
		private readonly LValue[] vars;
		private readonly string str;

		public override Type EvaluationType => typeof(string);

		public override void Emit(LOLMethod lm, Type t, ILGenerator gen)
		{
			if (this.vars.Length == 0)
			{
				//Just output the string
				gen.Emit(OpCodes.Ldstr, this.str);
			}
			else
			{
				//Output a call to string.Format
				gen.Emit(OpCodes.Ldstr, this.str);
				gen.Emit(OpCodes.Ldc_I4, this.vars.Length);
				gen.Emit(OpCodes.Newarr, typeof(object));

				for (var i = 0; i < this.vars.Length; i++)
				{
					gen.Emit(OpCodes.Dup);
					gen.Emit(OpCodes.Ldc_I4, i);
					this.vars[i].EmitGet(lm, this.vars[i].EvaluationType, gen);
					gen.Emit(OpCodes.Stelem, this.vars[i].EvaluationType);
				}

				gen.EmitCall(OpCodes.Call, typeof(string).GetMethod(nameof(string.Format), new Type[] { typeof(string), typeof(object[]) }), null);
			}
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen) { }

		public static string UnescapeString(string str, Scope s, Errors e, CodePragma location, List<VariableRef> refs)
		{
			var lastIdx = 1;
			int idx;
			var ret = new StringBuilder();

			try
			{
				while ((idx = str.IndexOf(':', lastIdx)) != -1)
				{
					//Append the string between the last escape and this one
					ret.Append(str, lastIdx, idx - lastIdx);

					//Decipher the escape
					int endIdx, refnum;
					VariableRef vr;
					switch (str[idx + 1])
					{
						case ')':
							ret.Append('\n');
							lastIdx = idx + 2;
							break;
						case '>':
							ret.Append('\t');
							lastIdx = idx + 2;
							break;
						case 'o':
							ret.Append('\a');
							lastIdx = idx + 2;
							break;
						case '"':
							ret.Append('"');
							lastIdx = idx + 2;
							break;
						case ':':
							ret.Append(':');
							lastIdx = idx + 2;
							break;
						case '(':
							endIdx = str.IndexOf(')', idx + 2);
							ret.Append(char.ConvertFromUtf32(int.Parse(str.Substring(idx + 2, endIdx - idx - 2), System.Globalization.NumberStyles.AllowHexSpecifier)));
							lastIdx = endIdx + 1;
							break;
						case '{':
							endIdx = str.IndexOf('}', idx + 2);
							vr = s[str.Substring(idx + 2, endIdx - idx - 2)] as VariableRef;
							if (vr == null)
							{
								e.SemErr(location.filename, location.startLine, location.startColumn, $"Undefined variable: \"{str.Substring(idx + 2, endIdx - idx - 2)}\"");
							}

							refnum = refs.IndexOf(vr);
							if (refnum == -1)
							{
								refnum = refs.Count;
								refs.Add(vr);
							}
							ret.Append("{" + refnum.ToString() + "}");
							lastIdx = endIdx + 1;
							break;
						case '[':
							endIdx = str.IndexOf(']', idx + 2);
							var uc = UnicodeNameLookup.GetUnicodeCharacter(str.Substring(idx + 2, endIdx - idx - 2));
							if (uc == null)
							{
								e.SemErr(location.filename, location.startLine, location.startColumn, $"Unknown unicode normative name: \"{str.Substring(idx + 2, endIdx - idx - 2)}\".");
							}
							else
							{
								ret.Append(uc);
							}
							lastIdx = endIdx + 1;
							break;
					}
				}

				//Append the end of the string
				ret.Append(str, lastIdx, str.Length - lastIdx - 1);
			}
			catch (Exception ex)
			{
				e.SemErr($"Invalid escape sequence in string constant: {ex.Message}");
			}

			return ret.ToString();
		}

		public StringExpression(CodePragma loc, string str, Scope s, Errors e) : base(loc)
		{
			var refs = new List<VariableRef>();
			this.str = UnescapeString(str, s, e, this.location, refs);

			this.vars = new LValue[refs.Count];
			for (var i = 0; i < this.vars.Length; i++)
			{
				this.vars[i] = new VariableLValue(this.location, refs[i]);
			}
		}
	}
}