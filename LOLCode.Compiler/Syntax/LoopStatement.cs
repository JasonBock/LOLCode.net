using System;
using System.CodeDom.Compiler;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
	internal class LoopStatement 
		: BreakableStatement
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
}
