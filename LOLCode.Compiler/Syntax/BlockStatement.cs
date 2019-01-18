using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
	internal class BlockStatement 
		: Statement
	{
		public List<Statement> statements = new List<Statement>();

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

		public BlockStatement(CodePragma loc) : base(loc) { }
	}
}
