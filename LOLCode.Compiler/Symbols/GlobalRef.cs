using System.Reflection.Emit;

namespace LOLCode.Compiler.Symbols
{
	// TODO: Should be sealed
	internal class GlobalRef 
		: VariableRef
	{
		// Should be readonly
		public FieldBuilder Field;

		public GlobalRef(string name)
		{
			this.Name = name;
			this.Field = null;
			this.Type = typeof(object);
		}
	}
}
