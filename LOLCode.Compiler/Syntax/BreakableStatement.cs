using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
	internal abstract class BreakableStatement 
		: Statement
	{
		public abstract string Name { get; }
		public abstract Label? BreakLabel { get; }
		public abstract Label? ContinueLabel { get; }

		public BreakableStatement(CodePragma loc) : base(loc) { }
	}
}
