using LOLCode.Compiler.Emitter;
using System.CodeDom.Compiler;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
	// TODO: make it sealed
	internal class ContinueStatement 
		: Statement
	{
		// TODO: These should be readonly
		public string label = null;
		private int breakIdx = -1;

		public ContinueStatement(CodePragma loc) 
			: base(loc) { }

		public override void Emit(LOLMethod lm, ILGenerator gen)
		{
			this.location.MarkSequencePoint(gen);

			// TODO: This needs to be addressed. If you call this after the ctor,
			// you'll get an index exception. What's the right thing to do?
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
					errors.Add(new CompilerError(this.location.filename, this.location.startLine, this.location.startColumn, null, 
						"MOAR encountered, but nothing to continue!"));
				}
				else
				{
					errors.Add(new CompilerError(this.location.filename, this.location.startLine, this.location.startColumn, null, 
						$"Named MOAR \"{this.label}\" encountered, but nothing by that name exists to continue!"));
				}
			}
		}
	}
}
