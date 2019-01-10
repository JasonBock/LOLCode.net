using notdot.LOLCode.Parser.v1_2;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace notdot.LOLCode
{
	internal class CodePragma
	{
		public ISymbolDocumentWriter doc;
		public string filename;
		public int startLine;
		public int startColumn;
		public int endLine;
		public int endColumn;

		public CodePragma(ISymbolDocumentWriter doc, string filename, int line, int column)
		{
			this.doc = doc;
			this.filename = filename;
			this.startLine = this.endLine = line;
			this.startColumn = this.endColumn = column;
		}

		internal void MarkSequencePoint(ILGenerator gen) => gen.MarkSequencePoint(this.doc, this.startLine, this.startColumn, this.endLine, this.endColumn);
	}

	internal abstract class CodeObject
	{
		public CodePragma location;

		public CodeObject(CodePragma loc) => this.location = loc;

		public abstract void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen);
	}

	internal abstract class Statement : CodeObject
	{
		public abstract void Emit(LOLMethod lm, ILGenerator gen);

		public Statement(CodePragma loc) : base(loc) { }
	}

	internal abstract class BreakableStatement : Statement
	{
		public abstract string Name { get; }
		public abstract Label? BreakLabel { get; }
		public abstract Label? ContinueLabel { get; }

		public BreakableStatement(CodePragma loc) : base(loc) { }
	}

	internal abstract class Expression : Statement
	{
		public abstract Type EvaluationType { get; }

		public abstract void Emit(LOLMethod lm, Type t, ILGenerator gen);

		public override void Emit(LOLMethod lm, ILGenerator gen) => this.Emit(lm, typeof(object), gen);

		public Expression(CodePragma loc) : base(loc) { }

		public static void EmitCast(ILGenerator gen, Type from, Type to)
		{
			if (from == to)
			{
				return;
			}
			else if (to == typeof(object))
			{
				if (from.IsValueType)
				{
					gen.Emit(OpCodes.Box, from);
				}
			}
			else if (from == typeof(object))
			{
				if (to == typeof(int))
				{
					gen.EmitCall(OpCodes.Call, typeof(stdlol.Utils).GetMethod("ToInt", BindingFlags.Public | BindingFlags.Static), null);
				}
				else if (to == typeof(float))
				{
					gen.EmitCall(OpCodes.Call, typeof(stdlol.Utils).GetMethod("ToFloat", BindingFlags.Public | BindingFlags.Static), null);
				}
				else if (to == typeof(string))
				{
					gen.EmitCall(OpCodes.Call, typeof(stdlol.Utils).GetMethod("ToString", BindingFlags.Public | BindingFlags.Static), null);
				}
				else if (to == typeof(bool))
				{
					gen.EmitCall(OpCodes.Call, typeof(stdlol.Utils).GetMethod("ToBool", BindingFlags.Public | BindingFlags.Static), null);
				}
				else
				{
					throw new InvalidOperationException(string.Format("Unknown cast: From {0} to {1}", from.Name, to.Name));
				}
			}
			else
			{
				throw new InvalidOperationException(string.Format("Unknown cast: From {0} to {1}", from.Name, to.Name));
			}
		}
	}

	internal abstract class LValue : Expression
	{
		public abstract void EmitGet(LOLMethod lm, Type t, ILGenerator gen);
		public abstract void StartSet(LOLMethod lm, Type t, ILGenerator gen);
		public abstract void EndSet(LOLMethod lm, Type t, ILGenerator gen);

		public LValue(CodePragma loc) : base(loc) { }

		public override void Emit(LOLMethod lm, Type t, ILGenerator gen) => this.EmitGet(lm, t, gen);
	}

	internal class BlockStatement : Statement
	{
		public List<Statement> statements = new List<Statement>();

		public override void Emit(LOLMethod lm, ILGenerator gen)
		{
			foreach (var stat in this.statements)
			{
				stat.Emit(lm, gen);
			}
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen)
		{
			foreach (var stat in this.statements)
			{
				stat.Process(lm, errors, gen);
			}
		}

		public BlockStatement(CodePragma loc) : base(loc) { }
	}

	internal class VariableLValue : LValue
	{
		public VariableRef var;

		public override Type EvaluationType => this.var.Type;

		public override void EmitGet(LOLMethod lm, Type t, ILGenerator gen)
		{
			if (this.var is LocalRef)
			{
				if (t == typeof(Dictionary<object, object>))
				{
					gen.Emit(OpCodes.Ldloca, (this.var as LocalRef).Local);
					gen.EmitCall(OpCodes.Call, typeof(stdlol.Utils).GetMethod("ToDict"), null);
				}
				else
				{
					gen.Emit(OpCodes.Ldloc, (this.var as LocalRef).Local);
				}
			}
			else if (this.var is GlobalRef)
			{
				if (t == typeof(Dictionary<object, object>))
				{
					gen.Emit(OpCodes.Ldnull);
					gen.Emit(OpCodes.Ldflda, (this.var as GlobalRef).Field);
					gen.EmitCall(OpCodes.Call, typeof(stdlol.Utils).GetMethod("ToDict"), null);
				}
				else
				{
					gen.Emit(OpCodes.Ldnull);
					gen.Emit(OpCodes.Ldfld, (this.var as GlobalRef).Field);
				}
			}
			else if (this.var is ArgumentRef)
			{
				if (t == typeof(Dictionary<object, object>))
				{
					gen.Emit(OpCodes.Ldarga, (this.var as ArgumentRef).Number);
					gen.EmitCall(OpCodes.Call, typeof(stdlol.Utils).GetMethod("ToDict"), null);
				}
				else
				{
					gen.Emit(OpCodes.Ldarg, (this.var as ArgumentRef).Number);
				}
			}
			else
			{
				throw new InvalidOperationException("Unknown variable type");
			}

			Expression.EmitCast(gen, this.var.Type, t);
		}

		public override void StartSet(LOLMethod lm, Type t, ILGenerator gen)
		{
			//Nothing to do
		}

		public override void EndSet(LOLMethod lm, Type t, ILGenerator gen)
		{
			//LOLProgram.WrapObject(t, gen);
			Expression.EmitCast(gen, t, this.var.Type);

			//Store it
			if (this.var is LocalRef)
			{
				gen.Emit(OpCodes.Stloc, (this.var as LocalRef).Local);
			}
			else if (this.var is GlobalRef)
			{
				gen.Emit(OpCodes.Stfld, (this.var as GlobalRef).Field);
			}
			else if (this.var is ArgumentRef)
			{
				gen.Emit(OpCodes.Starg, (this.var as ArgumentRef).Number);
			}
			else
			{
				throw new InvalidOperationException("Unknown variable type");
			}
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen)
		{
			return;
		}

		public VariableLValue(CodePragma loc) : base(loc) { }
		public VariableLValue(CodePragma loc, VariableRef vr) : base(loc) => this.var = vr;
	}

	internal class AssignmentStatement : Statement
	{
		public LValue lval;
		public Expression rval;

		public override void Emit(LOLMethod lm, ILGenerator gen)
		{
			this.location.MarkSequencePoint(gen);

			this.lval.StartSet(lm, this.rval.EvaluationType, gen);
			this.rval.Emit(lm, this.rval.EvaluationType, gen);
			this.lval.EndSet(lm, this.rval.EvaluationType, gen);
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen)
		{
			this.lval.Process(lm, errors, gen);
			this.rval.Process(lm, errors, gen);
		}

		public AssignmentStatement(CodePragma loc) : base(loc) { }
	}

	internal class VariableDeclarationStatement : Statement
	{
		public VariableRef var;
		public Expression expression = null;

		public override void Emit(LOLMethod lm, ILGenerator gen)
		{
			if (this.var is LocalRef)
			{
				lm.DefineLocal(gen, this.var as LocalRef);
			}

			if (this.expression != null)
			{
				this.expression.Emit(lm, gen);

				if (this.var is LocalRef)
				{
					gen.Emit(OpCodes.Stloc, (this.var as LocalRef).Local);
				}
				else
				{
					gen.Emit(OpCodes.Ldnull);
					gen.Emit(OpCodes.Stfld, (this.var as GlobalRef).Field);
				}
			}
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen)
		{
			return;
		}

		public VariableDeclarationStatement(CodePragma loc) : base(loc) { }
	}

	internal enum LoopType
	{
		Infinite,
		While,
		Until
	}

	internal class LoopStatement : BreakableStatement
	{
		public string name = null;
		public Statement statements;
		public Statement operation;
		public LoopType type = LoopType.Infinite;
		public Expression condition;

		private Label m_breakLabel;
		private Label m_continueLabel;

		public override Label? BreakLabel => this.m_breakLabel;

		public override Label? ContinueLabel => this.m_continueLabel;

		public override string Name => this.name;

		public override void Emit(LOLMethod lm, ILGenerator gen)
		{
			//Create the loop variable if it's defined
			/*if (operation != null)
				 lm.DefineLocal(gen, ((operation as AssignmentStatement).lval as VariableLValue).var as LocalRef);*/

			lm.breakables.Add(this);
			gen.MarkLabel(this.m_continueLabel);

			//Evaluate the condition (if one exists)
			if (this.condition != null)
			{
				this.condition.Emit(lm, typeof(bool), gen);
				if (this.type == LoopType.While)
				{
					gen.Emit(OpCodes.Brfalse, this.m_breakLabel);
				}
				else if (this.type == LoopType.Until)
				{
					gen.Emit(OpCodes.Brtrue, this.m_breakLabel);
				}
				else
				{
					throw new InvalidOperationException("Unknown loop type");
				}
			}

			this.statements.Emit(lm, gen);

			//Emit the loop op (if one exists)
			if (this.operation != null)
			{
				this.operation.Emit(lm, gen);
			}

			gen.Emit(OpCodes.Br, this.m_continueLabel);

			gen.MarkLabel(this.m_breakLabel);

			lm.breakables.RemoveAt(lm.breakables.Count - 1);
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen)
		{
			if (this.operation != null)
			{
				var fr = ((this.operation as AssignmentStatement).rval as FunctionExpression).func;
				if (fr.Arity > 1 || (fr.IsVariadic && fr.Arity != 0))
				{
					errors.Add(new CompilerError(this.location.filename, this.location.startLine, this.location.startColumn, null, "Function used in loop must take 1 argument"));
				}
			}

			this.m_breakLabel = gen.DefineLabel();
			this.m_continueLabel = gen.DefineLabel();

			lm.breakables.Add(this);
			this.statements.Process(lm, errors, gen);
			lm.breakables.RemoveAt(lm.breakables.Count - 1);
		}

		public void StartOperation(CodePragma loc) => this.operation = new AssignmentStatement(loc);

		public void SetOperationFunction(FunctionRef fr) => (this.operation as AssignmentStatement).rval = new FunctionExpression(this.operation.location, fr);

		public void SetLoopVariable(CodePragma loc, VariableRef vr)
		{
			(this.operation as AssignmentStatement).lval = new VariableLValue(loc, vr);
			((this.operation as AssignmentStatement).rval as FunctionExpression).arguments.Add(new VariableLValue(loc, vr));
		}

		public VariableRef GetLoopVariable() => ((this.operation as AssignmentStatement).lval as VariableLValue).var;

		public LoopStatement(CodePragma loc) : base(loc) { }
	}

	internal class PrimitiveExpression : Expression
	{
		public object value;

		public override Type EvaluationType => this.value.GetType();

		public override void Emit(LOLMethod lm, Type t, ILGenerator gen)
		{
			if (this.value is int && t == typeof(string))
			{
				this.value = ((int)this.value).ToString();
			}

			if (this.value is float && t == typeof(string))
			{
				this.value = ((float)this.value).ToString();
			}

			if (this.value is string && t == typeof(int))
			{
				this.value = int.Parse((string)this.value);
			}

			if (this.value is string && t == typeof(float))
			{
				this.value = float.Parse((string)this.value);
			}

			if (this.value.GetType() != t && t != typeof(object))
			{
				throw new ArgumentException(string.Format("{0} encountered, {1} expected.", this.value.GetType().Name, t.Name));
			}

			if (this.value is int)
			{
				gen.Emit(OpCodes.Ldc_I4, (int)this.value);
				if (t == typeof(object))
				{
					gen.Emit(OpCodes.Box, typeof(int));
				}
			}
			else if (this.value is float)
			{
				gen.Emit(OpCodes.Ldc_R4, (float)this.value);
				if (t == typeof(object))
				{
					gen.Emit(OpCodes.Box, typeof(float));
				}
			}
			else if (this.value is string)
			{
				gen.Emit(OpCodes.Ldstr, (string)this.value);
				if (t == typeof(object))
				{
					gen.Emit(OpCodes.Castclass, typeof(object));
				}
			}
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen)
		{
			if (this.value.GetType() != typeof(int) && this.value.GetType() != typeof(string))
			{
				//We throw an exception here because this would indicate an issue with the compiler, not with the code being compiled.
				throw new InvalidOperationException("PrimitiveExpression values must be int or string.");
			}

			return;
		}

		public PrimitiveExpression(CodePragma loc) : base(loc) { }
		public PrimitiveExpression(CodePragma loc, object val) : base(loc) => this.value = val;
	}

	internal class ConditionalStatement : Statement
	{
		public Expression condition;
		public Statement trueStatements;
		public Statement falseStatements;
		public Label ifFalse;
		public Label statementEnd;

		private bool invert = false;

		public override void Emit(LOLMethod lm, ILGenerator gen)
		{
			this.location.MarkSequencePoint(gen);

			this.condition.Emit(lm, typeof(bool), gen);

			if (this.invert)
			{
				gen.Emit(OpCodes.Brtrue, this.ifFalse);
			}
			else
			{
				gen.Emit(OpCodes.Brfalse, this.ifFalse);
			}

			this.trueStatements.Emit(lm, gen);
			if (!(this.falseStatements is BlockStatement) || ((BlockStatement)this.falseStatements).statements.Count > 0)
			{
				gen.Emit(OpCodes.Br, this.statementEnd);
			}

			gen.MarkLabel(this.ifFalse);
			if (!(this.falseStatements is BlockStatement) || ((BlockStatement)this.falseStatements).statements.Count > 0)
			{
				this.falseStatements.Emit(lm, gen);
			}

			//End of conditional
			gen.MarkLabel(this.statementEnd);
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen)
		{
			this.ifFalse = gen.DefineLabel();
			this.statementEnd = gen.DefineLabel();

			//If there are false statements but no true statements, invert the comparison and the branches
			if (this.trueStatements is BlockStatement && ((BlockStatement)this.trueStatements).statements.Count == 0)
			{
				var temp = this.trueStatements;
				this.trueStatements = this.falseStatements;
				this.falseStatements = temp;
				this.invert = true;
			}

			this.condition.Process(lm, errors, gen);
			this.trueStatements.Process(lm, errors, gen);
			if (this.falseStatements != null)
			{
				this.falseStatements.Process(lm, errors, gen);
			}
		}

		public ConditionalStatement(CodePragma loc) : base(loc) { }
	}

	internal class SwitchStatement : BreakableStatement
	{
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

		public List<Case> cases = new List<Case>();
		public Statement defaultCase = null;
		public Expression control;

		private Case[] sortedCases = null;
		private Label m_breakLabel;
		private Label defaultLabel;

		public override string Name => null;

		public override Label? BreakLabel => this.m_breakLabel;

		public override Label? ContinueLabel => null;

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
					ig.EmitCall(OpCodes.Call, typeof(string).GetMethod("Compare", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string), typeof(string) }, null), null);
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
					errors.Add(new CompilerError(this.location.filename, this.location.startLine, this.location.startColumn, null, string.Format("Duplicate OMG label: \"{0}\"", this.sortedCases[i].name)));
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

		public SwitchStatement(CodePragma loc) : base(loc) { }
	}

	internal class PrintStatement : Statement
	{
		public bool stderr = false;
		public Expression message;
		public bool newline = true;

		public override void Emit(LOLMethod lm, ILGenerator gen)
		{
			this.location.MarkSequencePoint(gen);

			//Get the appropriate stream
			if (this.stderr)
			{
				gen.EmitCall(OpCodes.Call, typeof(Console).GetProperty("Error", BindingFlags.Public | BindingFlags.Static).GetGetMethod(), null);
			}
			else
			{
				gen.EmitCall(OpCodes.Call, typeof(Console).GetProperty("Out", BindingFlags.Public | BindingFlags.Static).GetGetMethod(), null);
			}

			//Get the message
			this.message.Emit(lm, typeof(object), gen);

			//Indicate if it requires a newline or not
			if (this.newline)
			{
				gen.Emit(OpCodes.Ldc_I4_1);
			}
			else
			{
				gen.Emit(OpCodes.Ldc_I4_0);
			}

			gen.EmitCall(OpCodes.Call, typeof(stdlol.Utils).GetMethod("PrintObject"), null);
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen) => this.message.Process(lm, errors, gen);

		public PrintStatement(CodePragma loc) : base(loc) { }
	}

	internal enum IOAmount
	{
		Letter,
		Word,
		Line
	}

	internal class InputStatement : Statement
	{
		public IOAmount amount = IOAmount.Line;
		public LValue dest;

		public override void Emit(LOLMethod lm, ILGenerator gen)
		{
			this.location.MarkSequencePoint(gen);

			this.dest.StartSet(lm, typeof(string), gen);

			gen.EmitCall(OpCodes.Call, typeof(Console).GetProperty("In", BindingFlags.Public | BindingFlags.Static).GetGetMethod(), null);

			switch (this.amount)
			{
				case IOAmount.Letter:
					gen.EmitCall(OpCodes.Callvirt, typeof(TextReader).GetMethod("Read", new Type[0]), null);
					gen.EmitCall(OpCodes.Call, typeof(char).GetMethod("ToString", new Type[] { typeof(char) }), null);
					break;
				case IOAmount.Word:
					gen.EmitCall(OpCodes.Call, typeof(stdlol.Utils).GetMethod("ReadWord"), null);
					break;
				case IOAmount.Line:
					gen.EmitCall(OpCodes.Callvirt, typeof(TextReader).GetMethod("ReadLine", new Type[0]), null);
					break;
			}

			this.dest.EndSet(lm, typeof(string), gen);
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen) => this.dest.Process(lm, errors, gen);

		public InputStatement(CodePragma loc) : base(loc) { }
	}

	internal class BreakStatement : Statement
	{
		public string label = null;
		private int breakIdx = -1;

		public override void Emit(LOLMethod lm, ILGenerator gen)
		{
			this.location.MarkSequencePoint(gen);

			if (this.breakIdx < 0)
			{
				if (lm.info.ReturnType != typeof(void))
				{
					//TODO: When we optimise functions to possibly return other values, worry about this
					gen.Emit(OpCodes.Ldnull);
				}

				gen.Emit(OpCodes.Ret);
			}
			else
			{
				gen.Emit(OpCodes.Br, lm.breakables[this.breakIdx].BreakLabel.Value);
			}
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen)
		{
			this.breakIdx = lm.breakables.Count - 1;
			if (this.label == null)
			{
				while (this.breakIdx >= 0 && !lm.breakables[this.breakIdx].BreakLabel.HasValue)
				{
					this.breakIdx--;
				}
			}
			else
			{
				while (this.breakIdx >= 0 && (lm.breakables[this.breakIdx].Name != this.label || !lm.breakables[this.breakIdx].BreakLabel.HasValue))
				{
					this.breakIdx--;
				}
			}

			if (this.breakIdx < 0)
			{
				if (this.label != null)
				{
					errors.Add(new CompilerError(this.location.filename, this.location.startLine, this.location.startColumn, null, string.Format("Named ENUF \"{0}\" encountered, but nothing by that name exists to break out of!", this.label)));
				}
			}
		}

		public BreakStatement(CodePragma loc) : base(loc) { }
	}

	internal class ContinueStatement : Statement
	{
		public string label = null;
		private int breakIdx = -1;

		public override void Emit(LOLMethod lm, ILGenerator gen)
		{
			this.location.MarkSequencePoint(gen);

			gen.Emit(OpCodes.Br, lm.breakables[this.breakIdx].ContinueLabel.Value);
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen)
		{
			this.breakIdx = lm.breakables.Count - 1;
			if (this.label == null)
			{
				while (this.breakIdx >= 0 && !lm.breakables[this.breakIdx].ContinueLabel.HasValue)
				{
					this.breakIdx--;
				}
			}
			else
			{
				while (this.breakIdx >= 0 && (lm.breakables[this.breakIdx].Name != this.label || !lm.breakables[this.breakIdx].ContinueLabel.HasValue))
				{
					this.breakIdx--;
				}
			}

			if (this.breakIdx < 0)
			{
				if (this.label == null)
				{
					errors.Add(new CompilerError(this.location.filename, this.location.startLine, this.location.startColumn, null, "MOAR encountered, but nothing to continue!"));
				}
				else
				{
					errors.Add(new CompilerError(this.location.filename, this.location.startLine, this.location.startColumn, null, string.Format("Named MOAR \"{0}\" encountered, but nothing by that name exists to continue!", this.label)));
				}
			}
		}

		public ContinueStatement(CodePragma loc) : base(loc) { }
	}

	internal class FunctionExpression : Expression
	{
		public FunctionRef func;
		public List<Expression> arguments = new List<Expression>();

		public override Type EvaluationType => this.func.ReturnType;

		public override void Emit(LOLMethod lm, Type t, ILGenerator gen)
		{
			if (this.func.IsVariadic)
			{
				//First do standard (non variadic) arguments)
				for (var i = 0; i < this.func.Arity; i++)
				{
					this.arguments[i].Emit(lm, this.func.ArgumentTypes[i], gen);
				}

				//Now any variadic arguments go into an array
				var argType = this.func.ArgumentTypes[this.func.Arity].GetElementType();
				gen.Emit(OpCodes.Ldc_I4, this.arguments.Count - this.func.Arity);
				gen.Emit(OpCodes.Newarr, argType);

				for (var i = this.func.Arity; i < this.arguments.Count; i++)
				{
					gen.Emit(OpCodes.Dup);
					gen.Emit(OpCodes.Ldc_I4, i - this.func.Arity);
					this.arguments[i].Emit(lm, argType, gen);
					gen.Emit(OpCodes.Stelem, argType);
				}

				gen.EmitCall(OpCodes.Call, this.func.Method, null);
			}
			else
			{
				for (var i = 0; i < this.arguments.Count; i++)
				{
					this.arguments[i].Emit(lm, this.func.ArgumentTypes[i], gen);
				}

				gen.EmitCall(OpCodes.Call, this.func.Method, null);
			}

			//Finally, make sure the return type is correct
			Expression.EmitCast(gen, this.func.ReturnType, t);
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen)
		{
			if (this.arguments.Count != this.func.Arity && !this.func.IsVariadic)
			{
				errors.Add(new CompilerError(this.location.filename, this.location.startLine, this.location.startColumn, null, string.Format("Function \"{0}\" requires {1} arguments, passed {2}.", this.func.Name, this.func.Arity, this.arguments.Count)));
			}
			else if (this.arguments.Count < this.func.Arity && this.func.IsVariadic)
			{
				errors.Add(new CompilerError(this.location.filename, this.location.startLine, this.location.startColumn, null, string.Format("Function \"{0}\" requires at least {1} arguments, passed {2}.", this.func.Name, this.func.Arity, this.arguments.Count)));
			}

			foreach (var arg in this.arguments)
			{
				arg.Process(lm, errors, gen);
			}
		}

		public FunctionExpression(CodePragma loc) : base(loc) { }

		public FunctionExpression(CodePragma loc, FunctionRef fr) : base(loc) => this.func = fr;
	}

	internal class StringExpression : Expression
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

				gen.EmitCall(OpCodes.Call, typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(object[]) }), null);
			}
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen)
		{
			return;
		}

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
								e.SemErr(location.filename, location.startLine, location.startColumn, string.Format("Undefined variable: \"{0}\"", str.Substring(idx + 2, endIdx - idx - 2)));
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
								e.SemErr(location.filename, location.startLine, location.startColumn, string.Format("Unknown unicode normative name: \"{0}\".", str.Substring(idx + 2, endIdx - idx - 2)));
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
				e.SemErr(string.Format("Invalid escape sequence in string constant: {0}", ex.Message));
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

	internal class TypecastExpression : Expression
	{
		public Type destType;
		public Expression exp;

		public override Type EvaluationType => this.destType;

		public override void Emit(LOLMethod lm, Type t, ILGenerator gen)
		{
			this.exp.Emit(lm, this.destType, gen);
			if (this.destType != t)
			{
				Expression.EmitCast(gen, this.destType, t);
			}
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen) => this.exp.Process(lm, errors, gen);

		public TypecastExpression(CodePragma loc) : base(loc) { }

		public TypecastExpression(CodePragma loc, Type t, Expression exp) : base(loc)
		{
			this.destType = t;
			this.exp = exp;
		}
	}

	internal class ReturnStatement : Statement
	{
		public Expression expression;

		public override void Emit(LOLMethod lm, ILGenerator gen)
		{
			this.expression.Emit(lm, lm.info.ReturnType, gen);
			gen.Emit(OpCodes.Ret);
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen) => this.expression.Process(lm, errors, gen);

		public ReturnStatement(CodePragma loc) : base(loc) { }
	}
}
