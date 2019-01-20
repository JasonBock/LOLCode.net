using System;
using System.Reflection;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Symbols
{
	// TODO: should be sealed
	internal class UserFunctionRef 
		: FunctionRef
	{
		// TODO: Shoudl be readonly
		public MethodBuilder Builder;

		public override MethodInfo Method => this.Builder;

		public UserFunctionRef(string name, int arity, bool variadic)
		{
			this.Name = name;
			this.Arity = arity;
			this.IsVariadic = variadic;
			this.Builder = null;
			this.ReturnType = typeof(object);

			this.ArgumentTypes = new Type[arity];
			for (var i = 0; i < arity; i++)
			{
				this.ArgumentTypes[i] = typeof(object);
			}
		}
	}
}
