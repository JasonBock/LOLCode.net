using System;
using System.Reflection;

namespace LOLCode.Compiler.Symbols
{
	internal abstract class FunctionRef 
		: SymbolRef
	{
		// TODO: All of these should be readonly.
		public int Arity;
		public bool IsVariadic;
		public Type ReturnType;
		public Type[] ArgumentTypes;
		public abstract MethodInfo Method { get; }
	}
}
