using LOLCode.Compiler.Emitter;
using System.CodeDom.Compiler;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
	internal class ConditionalStatement 
		: Statement
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
}
