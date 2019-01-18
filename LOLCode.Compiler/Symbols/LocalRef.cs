using System.Reflection.Emit;

namespace LOLCode.Compiler.Symbols
{
	internal class LocalRef 
		: VariableRef
	{
		public LocalBuilder Local;

		public LocalRef(string name)
		{
			this.Name = name;
			this.Local = null;
			this.Type = typeof(object);
		}
	}
}
