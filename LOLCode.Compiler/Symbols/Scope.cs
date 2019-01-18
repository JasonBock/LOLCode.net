using System.Collections.Generic;

namespace LOLCode.Compiler.Symbols
{
	internal class Scope 
		: IEnumerable<SymbolRef>
	{
		private readonly Scope parentScope = null;
		private readonly Dictionary<string, SymbolRef> dict = new Dictionary<string, SymbolRef>();

		public Scope(Scope parent) => this.parentScope = parent;

		public Scope() : this(null) { }

		public SymbolRef this[string name]
		{
			get
			{
				if (!this.dict.TryGetValue(name, out var ret))
				{
					if (this.parentScope == null)
					{
						return null;
					}
					else
					{
						return this.parentScope[name];
					}
				}
				else
				{
					return ret;
				}
			}
		}

		public void AddSymbol(SymbolRef s) => this.dict.Add(s.Name, s);

		public bool RemoveSymbol(SymbolRef s) => this.dict.Remove(s.Name);

		public IEnumerator<SymbolRef> GetEnumerator() => this.dict.Values.GetEnumerator();

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();
	}
}