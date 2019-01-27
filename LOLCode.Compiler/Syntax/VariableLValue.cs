using LOLCode.Compiler.Emitter;
using LOLCode.Compiler.Symbols;
using System;
using System.CodeDom.Compiler;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
	// TODO: Make this sealed
	internal class VariableLValue 
		: LValue
	{
		// TODO: Make this readonly
		public VariableRef var;

		public VariableLValue(CodePragma loc) 
			: base(loc) { }

		public VariableLValue(CodePragma loc, VariableRef vr) 
			: base(loc) => this.var = vr;

		public override void EmitGet(LOLMethod lm, Type t, ILGenerator gen)
		{
			if (this.var is LocalRef)
			{
				gen.Emit(OpCodes.Ldloc, (this.var as LocalRef).Local);
			}
			else if (this.var is GlobalRef)
			{
				gen.Emit(OpCodes.Ldnull);
				gen.Emit(OpCodes.Ldfld, (this.var as GlobalRef).Field);
			}
			else if (this.var is ArgumentRef)
			{
				gen.Emit(OpCodes.Ldarg, (this.var as ArgumentRef).Number);
			}
			else
			{
				throw new InvalidOperationException("Unknown variable type");
			}

			Expression.EmitCast(gen, this.var.Type, t);
		}

		public override void StartSet(LOLMethod lm, Type t, ILGenerator gen) { }

		public override void EndSet(LOLMethod lm, Type t, ILGenerator gen)
		{
			Expression.EmitCast(gen, t, this.var.Type);

			if (this.var is LocalRef)
			{
				gen.Emit(OpCodes.Stloc, (this.var as LocalRef).Local);
			}
			else if (this.var is GlobalRef)
			{
				gen.Emit(OpCodes.Stfld, (this.var as GlobalRef).Field);
			}
			else if (this.var is ArgumentRef)
			{
				gen.Emit(OpCodes.Starg, (this.var as ArgumentRef).Number);
			}
			else
			{
				throw new InvalidOperationException("Unknown variable type");
			}
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen) { }

		public override Type EvaluationType => this.var.Type;
	}
}
