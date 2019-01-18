using LOLCode.Compiler.Emitter;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
	internal abstract class Statement 
		: CodeObject
	{
		public abstract void Emit(LOLMethod lm, ILGenerator gen);

		public Statement(CodePragma loc) : base(loc) { }
	}
}
