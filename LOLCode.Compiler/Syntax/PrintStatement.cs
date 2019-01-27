using LOLCode.Compiler.Emitter;
using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
	// TODO: Make this sealed
	internal class PrintStatement 
		: Statement
	{
		// TODO: make all of these readonly
		public bool stderr = false;
		public Expression message;
		public bool newline = true;

		public PrintStatement(CodePragma loc) 
			: base(loc) { }

		public override void Emit(LOLMethod lm, ILGenerator gen)
		{
			this.location.MarkSequencePoint(gen);

			//Get the appropriate stream
			if (this.stderr)
			{
				gen.EmitCall(OpCodes.Call, typeof(Console).GetProperty(nameof(Console.Error), BindingFlags.Public | BindingFlags.Static).GetGetMethod(), null);
			}
			else
			{
				gen.EmitCall(OpCodes.Call, typeof(Console).GetProperty(nameof(Console.Out), BindingFlags.Public | BindingFlags.Static).GetGetMethod(), null);
			}

			//Get the message
			this.message.Emit(lm, typeof(object), gen);

			//Indicate if it requires a newline or not
			if (this.newline)
			{
				gen.Emit(OpCodes.Ldc_I4_1);
			}
			else
			{
				gen.Emit(OpCodes.Ldc_I4_0);
			}

			gen.EmitCall(OpCodes.Call, typeof(Utils).GetMethod(nameof(Utils.PrintObject)), null);
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen) => this.message.Process(lm, errors, gen);
	}
}
