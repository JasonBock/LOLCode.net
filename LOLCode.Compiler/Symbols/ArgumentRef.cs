namespace LOLCode.Compiler.Symbols
{
	// TODO: Should be sealed
	internal class ArgumentRef
		: VariableRef
	{
		// TODO: Should be readonly
		public short Number;

		public ArgumentRef(string name, short num)
		{
			this.Name = name;
			this.Number = num;
			this.Type = typeof(object);
		}
	}
}
