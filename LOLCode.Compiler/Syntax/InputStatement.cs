using LOLCode.Compiler.Emitter;
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
	// TODO: make it sealed
	internal class InputStatement 
		: Statement
	{
		// TODO: make these readonly
		public IOAmount amount = IOAmount.Line;
		public LValue dest;

		public InputStatement(CodePragma loc) 
			: base(loc) { }

		public override void Emit(LOLMethod lm, ILGenerator gen)
		{
			this.location.MarkSequencePoint(gen);

			gen.EmitCall(OpCodes.Call, typeof(Console).GetProperty(nameof(Console.In), BindingFlags.Public | BindingFlags.Static).GetGetMethod(), null);

			switch (this.amount)
			{
				case IOAmount.Letter:
					gen.EmitCall(OpCodes.Callvirt, typeof(TextReader).GetMethod(nameof(TextReader.Read), new Type[0]), null);
					gen.EmitCall(OpCodes.Call, typeof(char).GetMethod(nameof(char.ToString), new Type[] { typeof(char) }), null);
					break;
				case IOAmount.Word:
					gen.EmitCall(OpCodes.Call, typeof(Utils).GetMethod(nameof(Utils.ReadWord)), null);
					break;
				case IOAmount.Line:
					gen.EmitCall(OpCodes.Callvirt, typeof(TextReader).GetMethod(nameof(TextReader.ReadLine), new Type[0]), null);
					break;
			}

			this.dest.EndSet(lm, typeof(string), gen);
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen) => this.dest.Process(lm, errors, gen);
	}
}