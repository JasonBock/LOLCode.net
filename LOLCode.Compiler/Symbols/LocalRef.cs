using System.Reflection.Emit;

namespace LOLCode.Compiler.Symbols
{
	// TODO: Should be sealed.
	internal class LocalRef
		: VariableRef
	{
		// TODO: Should be readonly property
		public LocalBuilder Local;

		public LocalRef(string name)
		{
			this.Name = name;
			this.Local = null;
			this.Type = typeof(object);
		}
	}
}
