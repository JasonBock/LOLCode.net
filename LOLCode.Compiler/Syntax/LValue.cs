using System;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
	internal abstract class LValue : Expression
	{
		public abstract void EmitGet(LOLMethod lm, Type t, ILGenerator gen);
		public abstract void StartSet(LOLMethod lm, Type t, ILGenerator gen);
		public abstract void EndSet(LOLMethod lm, Type t, ILGenerator gen);

		public LValue(CodePragma loc) : base(loc) { }

		public override void Emit(LOLMethod lm, Type t, ILGenerator gen) => this.EmitGet(lm, t, gen);
	}
}
