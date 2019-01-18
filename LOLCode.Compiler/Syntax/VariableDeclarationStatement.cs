using LOLCode.Compiler.Symbols;
using System.CodeDom.Compiler;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
	internal class VariableDeclarationStatement 
		: Statement
	{
		public VariableRef var;
		public Expression expression = null;

		public override void Emit(LOLMethod lm, ILGenerator gen)
		{
			if (this.var is LocalRef)
			{
				lm.DefineLocal(gen, this.var as LocalRef);
			}

			if (this.expression != null)
			{
				this.expression.Emit(lm, gen);

				if (this.var is LocalRef)
				{
					gen.Emit(OpCodes.Stloc, (this.var as LocalRef).Local);
				}
				else
				{
					gen.Emit(OpCodes.Ldnull);
					gen.Emit(OpCodes.Stfld, (this.var as GlobalRef).Field);
				}
			}
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen) { }

		public VariableDeclarationStatement(CodePragma loc) : base(loc) { }
	}
}
