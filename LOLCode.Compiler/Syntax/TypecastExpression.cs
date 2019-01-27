using LOLCode.Compiler.Emitter;
using System;
using System.CodeDom.Compiler;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
	// TODO: Make this sealed
	internal class TypecastExpression 
		: Expression
	{
		// TODO: Make these readonly
		public Type destType;
		public Expression exp;

		public TypecastExpression(CodePragma loc) 
			: base(loc) { }

		public TypecastExpression(CodePragma loc, Type t, Expression exp)
			: base(loc) => (this.destType, this.exp) = (t, exp);

		public override void Emit(LOLMethod lm, Type t, ILGenerator gen)
		{
			this.exp.Emit(lm, this.destType, gen);
			if (this.destType != t)
			{
				Expression.EmitCast(gen, this.destType, t);
			}
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen) => this.exp.Process(lm, errors, gen);

		public override Type EvaluationType => this.destType;
	}
}
