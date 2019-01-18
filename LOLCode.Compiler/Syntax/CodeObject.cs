using System.CodeDom.Compiler;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
	internal abstract class CodeObject
	{
		public CodePragma location;

		public CodeObject(CodePragma loc) => this.location = loc;

		public abstract void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen);
	}
}
