using LOLCode.Compiler.Emitter;
using LOLCode.Compiler.Syntax;
using System.CodeDom.Compiler;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Tests.Syntax
{
	internal sealed class MockStatement
		: Statement
	{
		public MockStatement(CodePragma loc)
			: base(loc) { }

		public override void Emit(LOLMethod lm, ILGenerator gen) =>
			gen.Emit(OpCodes.Break);

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen)
		{
			gen.Emit(OpCodes.Break);
			gen.Emit(OpCodes.Break);
		}
	}
}
