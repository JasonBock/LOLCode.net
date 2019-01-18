namespace LOLCode.Compiler.Symbols
{
	internal class ArgumentRef 
		: VariableRef
	{
		public short Number;

		public ArgumentRef(string name, short num)
		{
			this.Name = name;
			this.Number = num;
			this.Type = typeof(object);
		}
	}
}
