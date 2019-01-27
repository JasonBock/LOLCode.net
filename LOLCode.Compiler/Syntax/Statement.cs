using LOLCode.Compiler.Emitter;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
	internal abstract class Statement 
		: CodeObject
	{
		// TODO: Make this protected
		public Statement(CodePragma loc) 
			: base(loc) { }

		public abstract void Emit(LOLMethod lm, ILGenerator gen);
	}
}
