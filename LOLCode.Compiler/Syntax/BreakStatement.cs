using System.CodeDom.Compiler;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
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
					errors.Add(new CompilerError(this.location.filename, this.location.startLine, this.location.startColumn, null, 
						$"Named ENUF \"{this.label}\" encountered, but nothing by that name exists to break out of!"));
				}
			}
		}

		public BreakStatement(CodePragma loc) : base(loc) { }
	}
}
