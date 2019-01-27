using System;
using System.Reflection;

namespace LOLCode.Compiler.Symbols
{
	// TODO: Make this sealed
	internal class ImportFunctionRef
		: FunctionRef
	{
		// TODO: Can probably get rid of the field and make the property read-only.
		private readonly MethodInfo m_Method;

		public override MethodInfo Method => this.m_Method;

		public ImportFunctionRef(MethodInfo m, string name)
		{
			this.Name = name;

			var pi = m.GetParameters();
			// TODO: I think using GetCustomAttributeData should be quicker.
			if(pi.Length > 0 && pi[pi.Length - 1].GetCustomAttributes(typeof(ParamArrayAttribute), true).Length > 0)
			{
				this.Arity = pi.Length - 1;
				this.IsVariadic = true;
			}
			else
			{
				this.Arity = pi.Length;
				this.IsVariadic = false;
			}

			this.m_Method = m;
			this.ReturnType = m.ReturnType;

			this.ArgumentTypes = new Type[pi.Length];
			for (var i = 0; i < pi.Length; i++)
			{
				this.ArgumentTypes[i] = pi[i].ParameterType;
			}
		}
	}
}