using LOLCode.Compiler.Emitter;
using System;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
	internal abstract class LValue 
		: Expression
	{
		// TODO: Make this protected.
		public LValue(CodePragma loc)
			: base(loc) { }

		public abstract void EmitGet(LOLMethod lm, Type t, ILGenerator gen);
		public abstract void StartSet(LOLMethod lm, Type t, ILGenerator gen);
		public abstract void EndSet(LOLMethod lm, Type t, ILGenerator gen);
		public override void Emit(LOLMethod lm, Type t, ILGenerator gen) => this.EmitGet(lm, t, gen);
	}
}
