using System;
using System.Collections.Generic;
using System.IO;

namespace notdot.LOLCode.Parser.v1_2
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

	//-----------------------------------------------------------------------------------
	// Buffer
	//-----------------------------------------------------------------------------------
	internal class Buffer
	{
		public const int EOF = char.MaxValue + 1;
		const int MAX_BUFFER_LENGTH = 64 * 1024; // 64KB
		byte[] buf;         // input buffer
		int bufStart;       // position of first byte in buffer relative to input stream
		int bufLen;         // length of buffer
		int fileLen;        // length of input stream
		int pos;            // current position in buffer
		Stream stream;      // input stream (seekable)
		bool isUserStream;  // was the stream opened by the user?

		public Buffer(Stream s, bool isUserStream)
		{
			this.stream = s; this.isUserStream = isUserStream;
			this.fileLen = this.bufLen = (int)s.Length;
			if (this.stream.CanSeek && this.bufLen > MAX_BUFFER_LENGTH)
			{
				this.bufLen = MAX_BUFFER_LENGTH;
			}

			this.buf = new byte[this.bufLen];
			this.bufStart = int.MaxValue; // nothing in the buffer so far
			this.Pos = 0; // setup buffer to position 0 (start)
			if (this.bufLen == this.fileLen)
			{
				this.Close();
			}
		}

		protected Buffer(Buffer b)
		{ // called in UTF8Buffer constructor
			this.buf = b.buf;
			this.bufStart = b.bufStart;
			this.bufLen = b.bufLen;
			this.fileLen = b.fileLen;
			this.pos = b.pos;
			this.stream = b.stream;
			b.stream = null;
			this.isUserStream = b.isUserStream;
		}

		~Buffer() { this.Close(); }

		protected void Close()
		{
			if (!this.isUserStream && this.stream != null)
			{
				this.stream.Close();
				this.stream = null;
			}
		}

		public virtual int Read()
		{
			if (this.pos < this.bufLen)
			{
				return this.buf[this.pos++];
			}
			else if (this.Pos < this.fileLen)
			{
				this.Pos = this.Pos; // shift buffer start to Pos
				return this.buf[this.pos++];
			}
			else
			{
				return EOF;
			}
		}

		public int Peek()
		{
			var curPos = this.Pos;
			var ch = this.Read();
			this.Pos = curPos;
			return ch;
		}

		public string GetString(int beg, int end)
		{
			var len = end - beg;
			var buf = new char[len];
			var oldPos = this.Pos;
			this.Pos = beg;
			for (var i = 0; i < len; i++)
			{
				buf[i] = (char)this.Read();
			}

			this.Pos = oldPos;
			return new string(buf);
		}

		public int Pos
		{
			get => this.pos + this.bufStart;
			set
			{
				if (value < 0)
				{
					value = 0;
				}
				else if (value > this.fileLen)
				{
					value = this.fileLen;
				}

				if (value >= this.bufStart && value < this.bufStart + this.bufLen)
				{ // already in buffer
					this.pos = value - this.bufStart;
				}
				else if (this.stream != null)
				{ // must be swapped in
					this.stream.Seek(value, SeekOrigin.Begin);
					this.bufLen = this.stream.Read(this.buf, 0, this.buf.Length);
					this.bufStart = value; this.pos = 0;
				}
				else
				{
					this.pos = this.fileLen - this.bufStart; // make Pos return fileLen
				}
			}
		}
	}

	//-----------------------------------------------------------------------------------
	// UTF8Buffer
	//-----------------------------------------------------------------------------------
	internal class UTF8Buffer : Buffer
	{
		public UTF8Buffer(Buffer b) : base(b) { }

		public override int Read()
		{
			int ch;
			do
			{
				ch = base.Read();
				// until we find a uft8 start (0xxxxxxx or 11xxxxxx)
			} while ((ch >= 128) && ((ch & 0xC0) != 0xC0) && (ch != EOF));
			if (ch < 128 || ch == EOF)
			{
				// nothing to do, first 127 chars are the same in ascii and utf8
				// 0xxxxxxx or end of file character
			}
			else if ((ch & 0xF0) == 0xF0)
			{
				// 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
				var c1 = ch & 0x07; ch = base.Read();
				var c2 = ch & 0x3F; ch = base.Read();
				var c3 = ch & 0x3F; ch = base.Read();
				var c4 = ch & 0x3F;
				ch = (((((c1 << 6) | c2) << 6) | c3) << 6) | c4;
			}
			else if ((ch & 0xE0) == 0xE0)
			{
				// 1110xxxx 10xxxxxx 10xxxxxx
				var c1 = ch & 0x0F; ch = base.Read();
				var c2 = ch & 0x3F; ch = base.Read();
				var c3 = ch & 0x3F;
				ch = (((c1 << 6) | c2) << 6) | c3;
			}
			else if ((ch & 0xC0) == 0xC0)
			{
				// 110xxxxx 10xxxxxx
				var c1 = ch & 0x1F; ch = base.Read();
				var c2 = ch & 0x3F;
				ch = (c1 << 6) | c2;
			}
			return ch;
		}
	}

	//-----------------------------------------------------------------------------------
	// Scanner
	//-----------------------------------------------------------------------------------
	internal class Scanner
	{
		const char EOL = '\n';
		const int eofSym = 0; /* pdt */
		const int maxT = 63;
		const int noSym = 63;


		public Buffer buffer; // scanner buffer

		Token t;          // current token
		int ch;           // current input character
		int pos;          // byte position of current character
		int col;          // column number of current character
		int line;         // line number of current character
		int oldEols;      // EOLs that appeared in a comment;
		Dictionary<int, int> start; // maps first token character to start state

		Token tokens;     // list of tokens already peeked (first token is a dummy)
		Token pt;         // current peek token

		char[] tval = new char[128]; // text of current token
		int tlen;         // length of current token

		public Scanner(string fileName)
		{
			try
			{
				Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
				this.buffer = new Buffer(stream, false);
				this.Init();
			}
			catch (IOException)
			{
				throw new FatalError("Cannot open file " + fileName);
			}
		}

		public Scanner(Stream s)
		{
			this.buffer = new Buffer(s, true);
			this.Init();
		}

		void Init()
		{
			this.pos = -1; this.line = 1; this.col = 0;
			this.oldEols = 0;
			this.NextCh();
			if (this.ch == 0xEF)
			{ // check optional byte order mark for UTF-8
				this.NextCh(); var ch1 = this.ch;
				this.NextCh(); var ch2 = this.ch;
				if (ch1 != 0xBB || ch2 != 0xBF)
				{
					throw new FatalError(string.Format("illegal byte order mark: EF {0,2:X} {1,2:X}", ch1, ch2));
				}
				this.buffer = new UTF8Buffer(this.buffer); this.col = 0;
				this.NextCh();
			}
			this.start = new Dictionary<int, int>(128);
			for (var i = 65; i <= 65; ++i)
			{
				this.start[i] = 1;
			}

			for (var i = 67; i <= 78; ++i)
			{
				this.start[i] = 1;
			}

			for (var i = 80; i <= 90; ++i)
			{
				this.start[i] = 1;
			}

			for (var i = 97; i <= 122; ++i)
			{
				this.start[i] = 1;
			}

			for (var i = 48; i <= 57; ++i)
			{
				this.start[i] = 31;
			}

			for (var i = 10; i <= 10; ++i)
			{
				this.start[i] = 23;
			}

			for (var i = 44; i <= 44; ++i)
			{
				this.start[i] = 23;
			}

			this.start[46] = 32;
			this.start[34] = 14;
			this.start[66] = 33;
			this.start[79] = 34;
			this.start[8230] = 29;
			this.start[63] = 47;
			this.start[33] = 48;
			this.start[Buffer.EOF] = -1;

			this.pt = this.tokens = new Token();  // first token is a dummy
		}

		void NextCh()
		{
			if (this.oldEols > 0) { this.ch = EOL; this.oldEols--; }
			else
			{
				this.pos = this.buffer.Pos;
				this.ch = this.buffer.Read(); this.col++;
				// replace isolated '\r' by '\n' in order to make
				// eol handling uniform across Windows, Unix and Mac
				if (this.ch == '\r' && this.buffer.Peek() != '\n')
				{
					this.ch = EOL;
				}

				if (this.ch == EOL) { this.line++; this.col = 0; }
			}

		}

		void AddCh()
		{
			if (this.tlen >= this.tval.Length)
			{
				var newBuf = new char[2 * this.tval.Length];
				Array.Copy(this.tval, 0, newBuf, 0, this.tval.Length);
				this.tval = newBuf;
			}
			this.tval[this.tlen++] = (char)this.ch;
			this.NextCh();
		}




		void CheckLiteral()
		{
			switch (this.t.val)
			{
				case "CAN": this.t.kind = 6; break;
				case "IN": this.t.kind = 7; break;
				case "IM": this.t.kind = 8; break;
				case "OUTTA": this.t.kind = 9; break;
				case "MKAY": this.t.kind = 10; break;
				case "R": this.t.kind = 11; break;
				case "IS": this.t.kind = 12; break;
				case "HOW": this.t.kind = 13; break;
				case "HAI": this.t.kind = 14; break;
				case "TO": this.t.kind = 15; break;
				case "1.2": this.t.kind = 16; break;
				case "KTHXBYE": this.t.kind = 17; break;
				case "I": this.t.kind = 18; break;
				case "HAS": this.t.kind = 19; break;
				case "A": this.t.kind = 20; break;
				case "ITZ": this.t.kind = 21; break;
				case ".": this.t.kind = 22; break;
				case "GIMMEH": this.t.kind = 24; break;
				case "LINE": this.t.kind = 25; break;
				case "WORD": this.t.kind = 26; break;
				case "LETTAR": this.t.kind = 27; break;
				case "GTFO": this.t.kind = 28; break;
				case "MOAR": this.t.kind = 29; break;
				case "YR": this.t.kind = 30; break;
				case "TIL": this.t.kind = 31; break;
				case "WILE": this.t.kind = 32; break;
				case "O": this.t.kind = 33; break;
				case "RLY": this.t.kind = 34; break;
				case "YA": this.t.kind = 35; break;
				case "MEBBE": this.t.kind = 36; break;
				case "NO": this.t.kind = 37; break;
				case "WAI": this.t.kind = 38; break;
				case "OIC": this.t.kind = 39; break;
				case "WTF": this.t.kind = 40; break;
				case "OMG": this.t.kind = 41; break;
				case "OMGWTF": this.t.kind = 42; break;
				case "VISIBLE": this.t.kind = 43; break;
				case "INVISIBLE": this.t.kind = 44; break;
				case "NOW": this.t.kind = 46; break;
				case "OF": this.t.kind = 47; break;
				case "AN": this.t.kind = 48; break;
				case "MAEK": this.t.kind = 49; break;
				case "TROOF": this.t.kind = 50; break;
				case "NUMBR": this.t.kind = 51; break;
				case "NUMBAR": this.t.kind = 52; break;
				case "YARN": this.t.kind = 53; break;
				case "NOOB": this.t.kind = 54; break;
				case "WIN": this.t.kind = 55; break;
				case "FAIL": this.t.kind = 56; break;
				case "DUZ": this.t.kind = 57; break;
				case "IF": this.t.kind = 58; break;
				case "U": this.t.kind = 59; break;
				case "SAY": this.t.kind = 60; break;
				case "SO": this.t.kind = 61; break;
				case "FOUND": this.t.kind = 62; break;
				default: break;
			}
		}

		Token NextToken()
		{
			while (this.ch == ' ' || this.ch == 9 || this.ch == 13)
			{
				this.NextCh();
			}

			var apx = 0;
			this.t = new Token
			{
				pos = this.pos,
				col = this.col,
				line = this.line
			};
			int state;
			try { state = this.start[this.ch]; } catch (KeyNotFoundException) { state = 0; }
			this.tlen = 0; this.AddCh();

			switch (state)
			{
				case -1: { this.t.kind = eofSym; break; } // NextCh already done
				case 0: { this.t.kind = noSym; break; }   // NextCh already done
				case 1:
					if (this.ch >= '0' && this.ch <= '9' || this.ch >= 'A' && this.ch <= 'Z' || this.ch == '_' || this.ch >= 'a' && this.ch <= 'z') { this.AddCh(); goto case 1; }
					else { this.t.kind = 1; this.t.val = new string(this.tval, 0, this.tlen); this.CheckLiteral(); return this.t; }
				case 2:
					if (this.ch >= '0' && this.ch <= '9') { this.AddCh(); goto case 2; }
					else if (this.ch == 'E' || this.ch == 'e') { this.AddCh(); goto case 3; }
					else { this.t.kind = 3; this.t.val = new string(this.tval, 0, this.tlen); this.CheckLiteral(); return this.t; }
				case 3:
					if (this.ch >= '0' && this.ch <= '9') { this.AddCh(); goto case 5; }
					else if (this.ch == '+' || this.ch == '-') { this.AddCh(); goto case 4; }
					else { this.t.kind = noSym; break; }
				case 4:
					if (this.ch >= '0' && this.ch <= '9') { this.AddCh(); goto case 5; }
					else { this.t.kind = noSym; break; }
				case 5:
					if (this.ch >= '0' && this.ch <= '9') { this.AddCh(); goto case 5; }
					else { this.t.kind = 3; this.t.val = new string(this.tval, 0, this.tlen); this.CheckLiteral(); return this.t; }
				case 6:
					if (this.ch >= '0' && this.ch <= '9') { this.AddCh(); goto case 7; }
					else { this.t.kind = noSym; break; }
				case 7:
					if (this.ch >= '0' && this.ch <= '9') { this.AddCh(); goto case 7; }
					else if (this.ch == 'E' || this.ch == 'e') { this.AddCh(); goto case 8; }
					else { this.t.kind = 3; this.t.val = new string(this.tval, 0, this.tlen); this.CheckLiteral(); return this.t; }
				case 8:
					if (this.ch >= '0' && this.ch <= '9') { this.AddCh(); goto case 10; }
					else if (this.ch == '+' || this.ch == '-') { this.AddCh(); goto case 9; }
					else { this.t.kind = noSym; break; }
				case 9:
					if (this.ch >= '0' && this.ch <= '9') { this.AddCh(); goto case 10; }
					else { this.t.kind = noSym; break; }
				case 10:
					if (this.ch >= '0' && this.ch <= '9') { this.AddCh(); goto case 10; }
					else { this.t.kind = 3; this.t.val = new string(this.tval, 0, this.tlen); this.CheckLiteral(); return this.t; }
				case 11:
					if (this.ch >= '0' && this.ch <= '9') { this.AddCh(); goto case 13; }
					else if (this.ch == '+' || this.ch == '-') { this.AddCh(); goto case 12; }
					else { this.t.kind = noSym; break; }
				case 12:
					if (this.ch >= '0' && this.ch <= '9') { this.AddCh(); goto case 13; }
					else { this.t.kind = noSym; break; }
				case 13:
					if (this.ch >= '0' && this.ch <= '9') { this.AddCh(); goto case 13; }
					else { this.t.kind = 3; this.t.val = new string(this.tval, 0, this.tlen); this.CheckLiteral(); return this.t; }
				case 14:
					if (this.ch <= 9 || this.ch >= 11 && this.ch <= 12 || this.ch >= 14 && this.ch <= '!' || this.ch >= '#' && this.ch <= '9' || this.ch >= ';' && this.ch <= 65535) { this.AddCh(); goto case 14; }
					else if (this.ch == '"') { this.AddCh(); goto case 22; }
					else if (this.ch == ':') { this.AddCh(); goto case 35; }
					else { this.t.kind = noSym; break; }
				case 15:
					if (this.ch >= '0' && this.ch <= '9' || this.ch >= 'A' && this.ch <= 'F' || this.ch >= 'a' && this.ch <= 'f') { this.AddCh(); goto case 16; }
					else { this.t.kind = noSym; break; }
				case 16:
					if (this.ch >= '0' && this.ch <= '9' || this.ch >= 'A' && this.ch <= 'F' || this.ch >= 'a' && this.ch <= 'f') { this.AddCh(); goto case 36; }
					else if (this.ch == ')') { this.AddCh(); goto case 14; }
					else { this.t.kind = noSym; break; }
				case 17:
					if (this.ch == ')') { this.AddCh(); goto case 14; }
					else { this.t.kind = noSym; break; }
				case 18:
					if (this.ch >= 'A' && this.ch <= 'Z' || this.ch >= 'a' && this.ch <= 'z') { this.AddCh(); goto case 19; }
					else { this.t.kind = noSym; break; }
				case 19:
					if (this.ch >= '0' && this.ch <= '9' || this.ch >= 'A' && this.ch <= 'Z' || this.ch == '_' || this.ch >= 'a' && this.ch <= 'z') { this.AddCh(); goto case 19; }
					else if (this.ch == '}') { this.AddCh(); goto case 14; }
					else { this.t.kind = noSym; break; }
				case 20:
					if (this.ch <= 9 || this.ch >= 11 && this.ch <= 12 || this.ch >= 14 && this.ch <= '!' || this.ch >= '#' && this.ch <= '9' || this.ch >= ';' && this.ch <= 65535) { this.AddCh(); goto case 21; }
					else { this.t.kind = noSym; break; }
				case 21:
					if (this.ch <= 9 || this.ch >= 11 && this.ch <= 12 || this.ch >= 14 && this.ch <= '!' || this.ch >= '#' && this.ch <= '9' || this.ch >= ';' && this.ch <= 92 || this.ch >= '^' && this.ch <= 65535) { this.AddCh(); goto case 21; }
					else if (this.ch == ']') { this.AddCh(); goto case 38; }
					else { this.t.kind = noSym; break; }
				case 22:
					{ this.t.kind = 4; break; }
				case 23:
					{ this.t.kind = 5; this.t.val = new string(this.tval, 0, this.tlen); this.CheckLiteral(); return this.t; }
				case 24:
					if (this.ch == 10) { apx++; this.AddCh(); goto case 25; }
					else if (this.ch <= 9 || this.ch >= 11 && this.ch <= 65535) { this.AddCh(); goto case 24; }
					else { this.t.kind = noSym; break; }
				case 25:
					{
						this.tlen -= apx;
						this.buffer.Pos = this.t.pos; this.NextCh(); this.line = this.t.line; this.col = this.t.col;
						for (var i = 0; i < this.tlen; i++)
						{
							this.NextCh();
						}

						this.t.kind = 64; break;
					}
				case 26:
					if (this.ch <= 'S' || this.ch >= 'U' && this.ch <= 65535) { this.AddCh(); goto case 26; }
					else if (this.ch == 'T') { this.AddCh(); goto case 39; }
					else { this.t.kind = noSym; break; }
				case 27:
					{ this.t.kind = 65; break; }
				case 28:
					if (this.ch == '.') { this.AddCh(); goto case 29; }
					else { this.t.kind = noSym; break; }
				case 29:
					if (this.ch == 10) { this.AddCh(); goto case 30; }
					else if (this.ch <= 9 || this.ch >= 11 && this.ch <= 65535) { this.AddCh(); goto case 29; }
					else { this.t.kind = noSym; break; }
				case 30:
					{ this.t.kind = 66; break; }
				case 31:
					if (this.ch >= '0' && this.ch <= '9') { this.AddCh(); goto case 31; }
					else if (this.ch == '.') { this.AddCh(); goto case 6; }
					else if (this.ch == 'E' || this.ch == 'e') { this.AddCh(); goto case 11; }
					else { this.t.kind = 2; break; }
				case 32:
					if (this.ch >= '0' && this.ch <= '9') { this.AddCh(); goto case 2; }
					else if (this.ch == '.') { this.AddCh(); goto case 28; }
					else { this.t.kind = 5; this.t.val = new string(this.tval, 0, this.tlen); this.CheckLiteral(); return this.t; }
				case 33:
					if (this.ch >= '0' && this.ch <= '9' || this.ch >= 'A' && this.ch <= 'S' || this.ch >= 'U' && this.ch <= 'Z' || this.ch == '_' || this.ch >= 'a' && this.ch <= 'z') { this.AddCh(); goto case 1; }
					else if (this.ch == 'T') { this.AddCh(); goto case 40; }
					else { this.t.kind = 1; this.t.val = new string(this.tval, 0, this.tlen); this.CheckLiteral(); return this.t; }
				case 34:
					if (this.ch >= '0' && this.ch <= '9' || this.ch == 'A' || this.ch >= 'C' && this.ch <= 'Z' || this.ch == '_' || this.ch >= 'a' && this.ch <= 'z') { this.AddCh(); goto case 1; }
					else if (this.ch == 'B') { this.AddCh(); goto case 41; }
					else { this.t.kind = 1; this.t.val = new string(this.tval, 0, this.tlen); this.CheckLiteral(); return this.t; }
				case 35:
					if (this.ch == '"' || this.ch == ')' || this.ch == ':' || this.ch == '>' || this.ch == 'o') { this.AddCh(); goto case 14; }
					else if (this.ch == '(') { this.AddCh(); goto case 15; }
					else if (this.ch == '{') { this.AddCh(); goto case 18; }
					else if (this.ch == '[') { this.AddCh(); goto case 20; }
					else { this.t.kind = noSym; break; }
				case 36:
					if (this.ch >= '0' && this.ch <= '9' || this.ch >= 'A' && this.ch <= 'F' || this.ch >= 'a' && this.ch <= 'f') { this.AddCh(); goto case 37; }
					else if (this.ch == ')') { this.AddCh(); goto case 14; }
					else { this.t.kind = noSym; break; }
				case 37:
					if (this.ch >= '0' && this.ch <= '9' || this.ch >= 'A' && this.ch <= 'F' || this.ch >= 'a' && this.ch <= 'f') { this.AddCh(); goto case 17; }
					else if (this.ch == ')') { this.AddCh(); goto case 14; }
					else { this.t.kind = noSym; break; }
				case 38:
					if (this.ch <= 9 || this.ch >= 11 && this.ch <= 12 || this.ch >= 14 && this.ch <= '!' || this.ch >= '#' && this.ch <= '9' || this.ch >= ';' && this.ch <= 65535) { this.AddCh(); goto case 38; }
					else if (this.ch == '"') { this.AddCh(); goto case 22; }
					else if (this.ch == ':') { this.AddCh(); goto case 35; }
					else { this.t.kind = noSym; break; }
				case 39:
					if (this.ch <= 'K' || this.ch >= 'M' && this.ch <= 65535) { this.AddCh(); goto case 26; }
					else if (this.ch == 'L') { this.AddCh(); goto case 42; }
					else { this.t.kind = noSym; break; }
				case 40:
					if (this.ch >= '0' && this.ch <= '9' || this.ch >= 'A' && this.ch <= 'V' || this.ch >= 'X' && this.ch <= 'Z' || this.ch == '_' || this.ch >= 'a' && this.ch <= 'z') { this.AddCh(); goto case 1; }
					else if (this.ch == 'W') { this.AddCh(); goto case 43; }
					else { this.t.kind = 1; this.t.val = new string(this.tval, 0, this.tlen); this.CheckLiteral(); return this.t; }
				case 41:
					if (this.ch >= '0' && this.ch <= '9' || this.ch >= 'A' && this.ch <= 'S' || this.ch >= 'U' && this.ch <= 'Z' || this.ch == '_' || this.ch >= 'a' && this.ch <= 'z') { this.AddCh(); goto case 1; }
					else if (this.ch == 'T') { this.AddCh(); goto case 44; }
					else { this.t.kind = 1; this.t.val = new string(this.tval, 0, this.tlen); this.CheckLiteral(); return this.t; }
				case 42:
					if (this.ch <= 'C' || this.ch >= 'E' && this.ch <= 65535) { this.AddCh(); goto case 26; }
					else if (this.ch == 'D') { this.AddCh(); goto case 45; }
					else { this.t.kind = noSym; break; }
				case 43:
					if (this.ch >= '0' && this.ch <= '9' || this.ch >= 'A' && this.ch <= 'Z' || this.ch == '_' || this.ch >= 'a' && this.ch <= 'z') { this.AddCh(); goto case 43; }
					else if (this.ch == 10) { apx++; this.AddCh(); goto case 25; }
					else if (this.ch <= 9 || this.ch >= 11 && this.ch <= '/' || this.ch >= ':' && this.ch <= '@' || this.ch >= '[' && this.ch <= '^' || this.ch == '`' || this.ch >= '{' && this.ch <= 65535) { this.AddCh(); goto case 24; }
					else { this.t.kind = 1; this.t.val = new string(this.tval, 0, this.tlen); this.CheckLiteral(); return this.t; }
				case 44:
					if (this.ch >= '0' && this.ch <= '9' || this.ch >= 'A' && this.ch <= 'V' || this.ch >= 'X' && this.ch <= 'Z' || this.ch == '_' || this.ch >= 'a' && this.ch <= 'z') { this.AddCh(); goto case 1; }
					else if (this.ch == 'W') { this.AddCh(); goto case 46; }
					else { this.t.kind = 1; this.t.val = new string(this.tval, 0, this.tlen); this.CheckLiteral(); return this.t; }
				case 45:
					if (this.ch <= 'Q' || this.ch >= 'S' && this.ch <= 65535) { this.AddCh(); goto case 26; }
					else if (this.ch == 'R') { this.AddCh(); goto case 27; }
					else { this.t.kind = noSym; break; }
				case 46:
					if (this.ch >= '0' && this.ch <= '9' || this.ch >= 'A' && this.ch <= 'Z' || this.ch == '_' || this.ch >= 'a' && this.ch <= 'z') { this.AddCh(); goto case 1; }
					else if (this.ch == 9 || this.ch >= 11 && this.ch <= 12 || this.ch == ' ') { this.AddCh(); goto case 26; }
					else { this.t.kind = 1; this.t.val = new string(this.tval, 0, this.tlen); this.CheckLiteral(); return this.t; }
				case 47:
					{ this.t.kind = 23; break; }
				case 48:
					{ this.t.kind = 45; break; }

			}
			this.t.val = new string(this.tval, 0, this.tlen);
			return this.t;
		}

		// get the next token (possibly a token already seen during peeking)
		public Token Scan()
		{
			if (this.tokens.next == null)
			{
				return this.NextToken();
			}
			else
			{
				this.pt = this.tokens = this.tokens.next;
				return this.tokens;
			}
		}

		// peek for the next token, ignore pragmas
		public Token Peek()
		{
			if (this.pt.next == null)
			{
				do
				{
					this.pt = this.pt.next = this.NextToken();
				} while (this.pt.kind > maxT); // skip pragmas
			}
			else
			{
				do
				{
					this.pt = this.pt.next;
				} while (this.pt.kind > maxT);
			}
			return this.pt;
		}

		// make sure that peeking starts at the current scan position
		public void ResetPeek() => this.pt = this.tokens;

	} // end Scanner

}