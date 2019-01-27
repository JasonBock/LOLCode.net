using LOLCode.Compiler.Emitter;
using LOLCode.Compiler.Symbols;
using System.CodeDom.Compiler;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
	// TODO: Make this sealed 
	internal class VariableDeclarationStatement 
		: Statement
	{
		// TODO: Make these readonly
		public VariableRef var;
		public Expression expression = null;

		public VariableDeclarationStatement(CodePragma loc) 
			: base(loc) { }

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
	}
}
