using LOLCode.Compiler.Emitter;
using System.CodeDom.Compiler;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
	internal abstract class CodeObject
	{
		// TODO: This should be readonly
		public CodePragma location;

		// TODO: This should be protected.
		public CodeObject(CodePragma loc) => this.location = loc;

		public abstract void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen);
	}
}
