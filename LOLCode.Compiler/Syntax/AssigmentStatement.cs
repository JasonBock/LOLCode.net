﻿using LOLCode.Compiler.Emitter;
using System.CodeDom.Compiler;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
	// TODO: Should be sealed
	internal class AssignmentStatement 
		: Statement
	{
		// TODO: These should be readonly properties
		public LValue lval;
		public Expression rval;

		public AssignmentStatement(CodePragma loc) 
			: base(loc) { }

		public override void Emit(LOLMethod lm, ILGenerator gen)
		{
			this.location.MarkSequencePoint(gen);

			this.rval.Emit(lm, this.rval.EvaluationType, gen);
			this.lval.EndSet(lm, this.rval.EvaluationType, gen);
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen)
		{
			this.lval.Process(lm, errors, gen);
			this.rval.Process(lm, errors, gen);
		}
	}
}
