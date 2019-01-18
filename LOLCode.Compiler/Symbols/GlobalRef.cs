using System.Reflection.Emit;

namespace LOLCode.Compiler.Symbols
{
	internal class GlobalRef 
		: VariableRef
	{
		public FieldBuilder Field;

		public GlobalRef(string name)
		{
			this.Name = name;
			this.Field = null;
			this.Type = typeof(object);
		}
	}
}
