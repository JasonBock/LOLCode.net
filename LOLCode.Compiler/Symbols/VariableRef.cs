using System;

namespace LOLCode.Compiler.Symbols
{
	internal abstract class VariableRef 
		: SymbolRef
	{
		// TODO: Should be read-only.
		public Type Type;
	}
}
