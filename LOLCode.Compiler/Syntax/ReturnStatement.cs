using System.CodeDom.Compiler;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
	internal class ReturnStatement 
		: Statement
	{
		public Expression expression;

		public override void Emit(LOLMethod lm, ILGenerator gen)
		{
			this.expression.Emit(lm, lm.info.ReturnType, gen);
			gen.Emit(OpCodes.Ret);
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen) => 
			this.expression.Process(lm, errors, gen);

		public ReturnStatement(CodePragma loc) : base(loc) { }
	}
}
