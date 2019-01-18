namespace LOLCode.Compiler.Parser
{
	internal class Token
	{
		public int kind;    // token kind
		public int pos;     // token position in the source text (starting at 0)
		public int col;     // token column (starting at 0)
		public int line;    // token line (starting at 1)
		public string val;  // token value
		public Token next;  // ML 2005-03-11 Tokens are kept in linked list
	}
}
