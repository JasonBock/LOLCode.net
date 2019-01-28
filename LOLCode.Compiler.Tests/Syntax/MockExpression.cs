using LOLCode.Compiler.Emitter;
using LOLCode.Compiler.Syntax;
using System;
using System.CodeDom.Compiler;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Tests.Syntax
{
	internal sealed class MockExpression
		: Expression
	{
		public MockExpression(CodePragma loc)
			: base(loc) { }

		public override Type EvaluationType => typeof(int);

		public override void Emit(LOLMethod lm, Type t, ILGenerator gen) =>
			gen.Emit(OpCodes.Break);

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen)
		{
			gen.Emit(OpCodes.Break);
			gen.Emit(OpCodes.Break);
		}
	}
}
