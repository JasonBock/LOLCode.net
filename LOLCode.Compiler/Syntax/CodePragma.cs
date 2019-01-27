using System.Diagnostics.SymbolStore;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
	// TODO: make this sealed
	internal class CodePragma
	{
		// TODO: make all of these readonly
		public ISymbolDocumentWriter doc;
		public string filename;
		public int startLine;
		public int startColumn;
		public int endLine;
		public int endColumn;

		public CodePragma(ISymbolDocumentWriter doc, string filename, int line, int column)
		{
			this.doc = doc;
			this.filename = filename;
			this.startLine = this.endLine = line;
			this.startColumn = this.endColumn = column;
		}

		internal void MarkSequencePoint(ILGenerator gen) => gen.MarkSequencePoint(this.doc, this.startLine, this.startColumn, this.endLine, this.endColumn);
	}
}
