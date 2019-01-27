using LOLCode.Compiler.Emitter;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
	// TODO: Make this sealed
	internal class BlockStatement 
		: Statement
	{
		// TODO: Make this readonly (and immutable)
		public List<Statement> statements = new List<Statement>();
		public BlockStatement(CodePragma loc) 
			: base(loc) { }

		public override void Emit(LOLMethod lm, ILGenerator gen)
		{
			foreach (var stat in this.statements)
			{
				stat.Emit(lm, gen);
			}
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen)
		{
			foreach (var stat in this.statements)
			{
				stat.Process(lm, errors, gen);
			}
		}
	}
}
