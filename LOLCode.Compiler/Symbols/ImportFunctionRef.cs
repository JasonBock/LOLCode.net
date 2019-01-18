using System;
using System.Reflection;

namespace LOLCode.Compiler.Symbols
{
	internal class ImportFunctionRef : FunctionRef
	{
		private readonly MethodInfo m_Method;

		public override MethodInfo Method => this.m_Method;

		public ImportFunctionRef(MethodInfo m, string name)
		{
			this.Name = name;

			var pi = m.GetParameters();
			if (pi[pi.Length - 1].GetCustomAttributes(typeof(ParamArrayAttribute), true).Length > 0)
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
