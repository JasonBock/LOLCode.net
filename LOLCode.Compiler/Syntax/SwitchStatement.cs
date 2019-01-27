using LOLCode.Compiler.Emitter;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
	// TODO: Make this sealed
	internal class SwitchStatement
		: BreakableStatement
	{
		// TODO: make these readonly and/or immutable
		public List<Case> cases = new List<Case>();
		public Statement defaultCase = null;
		public Expression control;
		private Case[] sortedCases = null;
		private Label m_breakLabel;
		private Label defaultLabel;

		public SwitchStatement(CodePragma loc) 
			: base(loc) { }

		public override void Emit(LOLMethod lm, ILGenerator gen)
		{
			lm.breakables.Add(this);
			this.location.MarkSequencePoint(gen);

			if (this.cases[0].name is int)
			{
				//Switch is integer
				this.control.Emit(lm, typeof(int), gen);
				this.EmitIntegerSwitch(lm, gen);
			}
			else if (this.cases[0].name is string)
			{
				//Switch is string
				this.control.Emit(lm, typeof(string), gen);
				this.EmitStringSwitch(lm, gen);
			}

			gen.Emit(OpCodes.Br, this.defaultLabel);

			//Output code for all the cases
			foreach (var c in this.cases)
			{
				gen.MarkLabel(c.label);
				c.statement.Emit(lm, gen);
			}

			//Default case
			gen.MarkLabel(this.defaultLabel);
			this.defaultCase.Emit(lm, gen);

			//End of statement
			gen.MarkLabel(this.m_breakLabel);

			lm.breakables.RemoveAt(lm.breakables.Count - 1);
		}

		private delegate void SwitchComparisonDelegate(ILGenerator gen, Case c);

		private void EmitIntegerSwitch(LOLMethod lm, ILGenerator gen)
		{
			if (this.sortedCases.Length * 2 >= (((int)this.sortedCases[this.sortedCases.Length - 1].name) - ((int)this.sortedCases[0].name)))
			{
				//Switch is compact, emit a jump table
				this.EmitIntegerJumpTable(lm, gen);
			}
			else
			{
				//Switch is not compact - emit a binary tree
				var loc = lm.GetTempLocal(gen, typeof(int));
				gen.Emit(OpCodes.Stloc, loc);
				this.EmitSwitchTree(lm, gen, 0, this.sortedCases.Length, loc, delegate (ILGenerator ig, Case c)
				{
					ig.Emit(OpCodes.Ldc_I4, (int)c.name);
					ig.Emit(OpCodes.Sub);
				});
				lm.ReleaseTempLocal(loc);
			}
		}

		private void EmitIntegerJumpTable(LOLMethod lm, ILGenerator gen)
		{
			var len = ((int)this.sortedCases[this.sortedCases.Length - 1].name) - ((int)this.sortedCases[0].name) + 1;
			var offset = (int)this.sortedCases[0].name;
			if (offset < len / 2)
			{
				len += offset;
				offset = 0;
			}

			var jumpTable = new Label[len];
			var casePtr = 0;

			if (offset > 0)
			{
				gen.Emit(OpCodes.Ldc_I4, offset);
				gen.Emit(OpCodes.Sub);
			}

			for (var i = 0; i < len; i++)
			{
				if (((int)this.sortedCases[casePtr].name) == i + offset)
				{
					jumpTable[i] = this.sortedCases[casePtr++].label = gen.DefineLabel();
				}
				else
				{
					jumpTable[i] = this.defaultLabel;
				}
			}

			gen.Emit(OpCodes.Switch, jumpTable);
		}

		private void EmitStringSwitch(LOLMethod lm, ILGenerator gen)
		{
			var loc = lm.GetTempLocal(gen, typeof(string));
			gen.Emit(OpCodes.Stloc, loc);
			this.EmitSwitchTree(lm, gen, 0, this.sortedCases.Length, loc, delegate (ILGenerator ig, Case c)
			{
				ig.Emit(OpCodes.Ldstr, (string)c.name);
				ig.EmitCall(OpCodes.Call, typeof(string).GetMethod(nameof(string.Compare),
					BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string), typeof(string) }, null), null);
			});
			lm.ReleaseTempLocal(loc);
		}

		private void EmitSwitchTree(LOLMethod lm, ILGenerator gen, int off, int len, LocalBuilder loc, SwitchComparisonDelegate compare)
		{
			Label branch;

			var idx = off + len / 2;
			var c = this.sortedCases[idx];
			c.label = gen.DefineLabel();

			//Load the variable and compare it with the current case
			gen.Emit(OpCodes.Ldloc, loc);
			compare(gen, c);

			if (len == 1)
			{
				//If we're in a range of one, we can simplify things
				gen.Emit(OpCodes.Ldc_I4_0);
				gen.Emit(OpCodes.Beq, c.label);
				gen.Emit(OpCodes.Br, this.defaultLabel);
			}
			else if (idx == off)
			{
				//The less-than case is default, so test that last
				gen.Emit(OpCodes.Dup);
				gen.Emit(OpCodes.Ldc_I4_0);
				branch = gen.DefineLabel();
				gen.Emit(OpCodes.Bgt, branch);
				gen.Emit(OpCodes.Brfalse, c.label);
				//Not greater and not zero - must be less
				gen.Emit(OpCodes.Br, this.defaultLabel);

				gen.MarkLabel(branch);
				gen.Emit(OpCodes.Pop);
				this.EmitSwitchTree(lm, gen, off + 1, len - 1, loc, compare);
			}
			else if (idx == off + len - 1)
			{
				//The greater-than case is default,  so test that last
				gen.Emit(OpCodes.Dup);
				gen.Emit(OpCodes.Ldc_I4_0);
				branch = gen.DefineLabel();
				gen.Emit(OpCodes.Blt, branch);
				gen.Emit(OpCodes.Brfalse, c.label);
				//Not less and not zero - must be greater
				gen.Emit(OpCodes.Br, this.defaultLabel);

				gen.MarkLabel(branch);
				gen.Emit(OpCodes.Pop);
				this.EmitSwitchTree(lm, gen, off, len - 1, loc, compare);
			}
			else
			{
				//Both branches are non-empty
				gen.Emit(OpCodes.Dup);
				gen.Emit(OpCodes.Ldc_I4_0);
				branch = gen.DefineLabel();
				gen.Emit(OpCodes.Blt, branch);
				gen.Emit(OpCodes.Brfalse, c.label);
				//Not less and not zero - must be greater
				this.EmitSwitchTree(lm, gen, idx + 1, len - (idx - off) - 1, loc, compare);

				gen.MarkLabel(branch);
				gen.Emit(OpCodes.Pop);
				this.EmitSwitchTree(lm, gen, off, idx - off, loc, compare);
			}
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen)
		{
			this.m_breakLabel = gen.DefineLabel();
			if (this.defaultCase != null)
			{
				this.defaultLabel = gen.DefineLabel();
			}

			Type t = null;
			foreach (var c in this.cases)
			{
				if (c.name.GetType() != t)
				{
					if (t != null)
					{
						errors.Add(new CompilerError(this.location.filename, this.location.startLine, this.location.startColumn, null, "A WTF statement cannot have OMGs with more than one type"));
						break;
					}
					else
					{
						t = c.name.GetType();
					}
				}
			}

			if (t != typeof(int) && t != typeof(string))
			{
				errors.Add(new CompilerError(this.location.filename, this.location.startLine, this.location.startColumn, null, "OMG labels must be NUMBARs or YARNs"));
			}

			//Sort the cases
			this.sortedCases = new Case[this.cases.Count];
			this.cases.CopyTo(this.sortedCases);
			Array.Sort<Case>(this.sortedCases);

			//Check for duplicates
			for (var i = 1; i < this.sortedCases.Length; i++)
			{
				if (this.sortedCases[i - 1].CompareTo(this.sortedCases[i]) == 0)
				{
					errors.Add(new CompilerError(this.location.filename, this.location.startLine, this.location.startColumn, null, $"Duplicate OMG label: \"{this.sortedCases[i].name}\""));
				}
			}

			//Process child statements
			lm.breakables.Add(this);
			this.control.Process(lm, errors, gen);
			foreach (var c in this.cases)
			{
				c.statement.Process(lm, errors, gen);
			}

			this.defaultCase.Process(lm, errors, gen);
			lm.breakables.RemoveAt(lm.breakables.Count - 1);
		}

		public override string Name => null;

		public override Label? BreakLabel => this.m_breakLabel;

		public override Label? ContinueLabel => null;

		// TODO: Make this sealed
		public class Case : IComparable<Case>
		{
			public object name;
			public Statement statement;
			public Label label;

			public Case(object name, Statement stat)
			{
				this.name = name;
				this.statement = stat;
			}

			public int CompareTo(Case other)
			{
				if (this.name is int)
				{
					return ((int)this.name).CompareTo((int)other.name);
				}
				else
				{
					return (this.name as string).CompareTo(other.name as string);
				}
			}
		}
	}
}
