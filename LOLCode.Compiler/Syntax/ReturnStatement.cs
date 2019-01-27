using LOLCode.Compiler.Emitter;
using System.CodeDom.Compiler;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
	// TODO: Make it sealed
	internal class ReturnStatement 
		: Statement
	{
		// TODO: Make it readonly
		public Expression expression;

		public ReturnStatement(CodePragma loc) 
			: base(loc) { }

		public override void Emit(LOLMethod lm, ILGenerator gen)
		{
			this.expression.Emit(lm, lm.info.ReturnType, gen);
			gen.Emit(OpCodes.Ret);
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen) => 
			this.expression.Process(lm, errors, gen);
	}
}
