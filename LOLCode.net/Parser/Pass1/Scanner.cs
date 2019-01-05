
using System;
using System.IO;
using System.Collections.Generic;

namespace notdot.LOLCode.Parser.Pass1 {

internal class Token {
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
internal class Buffer {
	public const int EOF = char.MaxValue + 1;
	const int MAX_BUFFER_LENGTH = 64 * 1024; // 64KB
	byte[] buf;         // input buffer
	int bufStart;       // position of first byte in buffer relative to input stream
	int bufLen;         // length of buffer
	int fileLen;        // length of input stream
	int pos;            // current position in buffer
	Stream stream;      // input stream (seekable)
	bool isUserStream;  // was the stream opened by the user?
	
	public Buffer (Stream s, bool isUserStream) {
		stream = s; this.isUserStream = isUserStream;
		fileLen = bufLen = (int) s.Length;
		if (stream.CanSeek && bufLen > MAX_BUFFER_LENGTH) bufLen = MAX_BUFFER_LENGTH;
		buf = new byte[bufLen];
		bufStart = Int32.MaxValue; // nothing in the buffer so far
		Pos = 0; // setup buffer to position 0 (start)
		if (bufLen == fileLen) Close();
	}
	
	protected Buffer(Buffer b) { // called in UTF8Buffer constructor
		buf = b.buf;
		bufStart = b.bufStart;
		bufLen = b.bufLen;
		fileLen = b.fileLen;
		pos = b.pos;
		stream = b.stream;
		b.stream = null;
		isUserStream = b.isUserStream;
	}

	~Buffer() { Close(); }
	
	protected void Close() {
		if (!isUserStream && stream != null) {
			stream.Close();
			stream = null;
		}
	}
	
	public virtual int Read () {
		if (pos < bufLen) {
			return buf[pos++];
		} else if (Pos < fileLen) {
			Pos = Pos; // shift buffer start to Pos
			return buf[pos++];
		} else {
			return EOF;
		}
	}

	public int Peek () {
		int curPos = Pos;
		int ch = Read();
		Pos = curPos;
		return ch;
	}
	
	public string GetString (int beg, int end) {
		int len = end - beg;
		char[] buf = new char[len];
		int oldPos = Pos;
		Pos = beg;
		for (int i = 0; i < len; i++) buf[i] = (char) Read();
		Pos = oldPos;
		return new String(buf);
	}

	public int Pos {
		get { return pos + bufStart; }
		set {
			if (value < 0) value = 0; 
			else if (value > fileLen) value = fileLen;
			if (value >= bufStart && value < bufStart + bufLen) { // already in buffer
				pos = value - bufStart;
			} else if (stream != null) { // must be swapped in
				stream.Seek(value, SeekOrigin.Begin);
				bufLen = stream.Read(buf, 0, buf.Length);
				bufStart = value; pos = 0;
			} else {
				pos = fileLen - bufStart; // make Pos return fileLen
			}
		}
	}
}

//-----------------------------------------------------------------------------------
// UTF8Buffer
//-----------------------------------------------------------------------------------
internal class UTF8Buffer: Buffer {
	public UTF8Buffer(Buffer b): base(b) {}

	public override int Read() {
		int ch;
		do {
			ch = base.Read();
			// until we find a uft8 start (0xxxxxxx or 11xxxxxx)
		} while ((ch >= 128) && ((ch & 0xC0) != 0xC0) && (ch != EOF));
		if (ch < 128 || ch == EOF) {
			// nothing to do, first 127 chars are the same in ascii and utf8
			// 0xxxxxxx or end of file character
		} else if ((ch & 0xF0) == 0xF0) {
			// 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
			int c1 = ch & 0x07; ch = base.Read();
			int c2 = ch & 0x3F; ch = base.Read();
			int c3 = ch & 0x3F; ch = base.Read();
			int c4 = ch & 0x3F;
			ch = (((((c1 << 6) | c2) << 6) | c3) << 6) | c4;
		} else if ((ch & 0xE0) == 0xE0) {
			// 1110xxxx 10xxxxxx 10xxxxxx
			int c1 = ch & 0x0F; ch = base.Read();
			int c2 = ch & 0x3F; ch = base.Read();
			int c3 = ch & 0x3F;
			ch = (((c1 << 6) | c2) << 6) | c3;
		} else if ((ch & 0xC0) == 0xC0) {
			// 110xxxxx 10xxxxxx
			int c1 = ch & 0x1F; ch = base.Read();
			int c2 = ch & 0x3F;
			ch = (c1 << 6) | c2;
		}
		return ch;
	}
}

//-----------------------------------------------------------------------------------
// Scanner
//-----------------------------------------------------------------------------------
internal class Scanner {
	const char EOL = '\n';
	const int eofSym = 0; /* pdt */
	const int maxT = 78;
	const int noSym = 78;


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
	
	public Scanner (string fileName) {
		try {
			Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			buffer = new Buffer(stream, false);
			Init();
		} catch (IOException) {
			throw new FatalError("Cannot open file " + fileName);
		}
	}
	
	public Scanner (Stream s) {
		buffer = new Buffer(s, true);
		Init();
	}
	
	void Init() {
		pos = -1; line = 1; col = 0;
		oldEols = 0;
		NextCh();
		if (ch == 0xEF) { // check optional byte order mark for UTF-8
			NextCh(); int ch1 = ch;
			NextCh(); int ch2 = ch;
			if (ch1 != 0xBB || ch2 != 0xBF) {
				throw new FatalError(String.Format("illegal byte order mark: EF {0,2:X} {1,2:X}", ch1, ch2));
			}
			buffer = new UTF8Buffer(buffer); col = 0;
			NextCh();
		}
		start = new Dictionary<int, int>(128);
		for (int i = 65; i <= 65; ++i) start[i] = 1;
		for (int i = 67; i <= 78; ++i) start[i] = 1;
		for (int i = 80; i <= 90; ++i) start[i] = 1;
		for (int i = 95; i <= 95; ++i) start[i] = 1;
		for (int i = 97; i <= 122; ++i) start[i] = 1;
		for (int i = 48; i <= 57; ++i) start[i] = 24;
		for (int i = 10; i <= 10; ++i) start[i] = 16;
		for (int i = 44; i <= 44; ++i) start[i] = 16;
		start[46] = 25; 
		start[34] = 14; 
		start[66] = 26; 
		start[79] = 27; 
		start[8230] = 22; 
		start[63] = 56; 
		start[33] = 58; 
		start[Buffer.EOF] = -1;

		pt = tokens = new Token();  // first token is a dummy
	}
	
	void NextCh() {
		if (oldEols > 0) { ch = EOL; oldEols--; } 
		else {
			pos = buffer.Pos;
			ch = buffer.Read(); col++;
			// replace isolated '\r' by '\n' in order to make
			// eol handling uniform across Windows, Unix and Mac
			if (ch == '\r' && buffer.Peek() != '\n') ch = EOL;
			if (ch == EOL) { line++; col = 0; }
		}

	}

	void AddCh() {
		if (tlen >= tval.Length) {
			char[] newBuf = new char[2 * tval.Length];
			Array.Copy(tval, 0, newBuf, 0, tval.Length);
			tval = newBuf;
		}
		tval[tlen++] = (char)ch;
		NextCh();
	}




	void CheckLiteral() {
		switch (t.val) {
			case "HAI": t.kind = 6; break;
			case "TO": t.kind = 7; break;
			case "1.0": t.kind = 8; break;
			case "IRCSPECZ": t.kind = 9; break;
			case "1.1": t.kind = 10; break;
			case "1.2": t.kind = 11; break;
			case "KTHXBYE": t.kind = 12; break;
			case "HOW": t.kind = 13; break;
			case "DUZ": t.kind = 14; break;
			case "I": t.kind = 15; break;
			case "YR": t.kind = 16; break;
			case "AN": t.kind = 17; break;
			case "IF": t.kind = 18; break;
			case "U": t.kind = 19; break;
			case "SAY": t.kind = 20; break;
			case "SO": t.kind = 21; break;
			case "HAS": t.kind = 22; break;
			case "A": t.kind = 23; break;
			case "ITZ": t.kind = 24; break;
			case "CAN": t.kind = 25; break;
			case "GIMMEH": t.kind = 27; break;
			case "LINE": t.kind = 28; break;
			case "WORD": t.kind = 29; break;
			case "LETTAR": t.kind = 30; break;
			case "GTFO": t.kind = 31; break;
			case "ENUF": t.kind = 32; break;
			case "OV": t.kind = 33; break;
			case "UR": t.kind = 34; break;
			case "MOAR": t.kind = 35; break;
			case "IM": t.kind = 36; break;
			case "IN": t.kind = 37; break;
			case "KTHX": t.kind = 38; break;
			case "OUTTA": t.kind = 39; break;
			case "UPZ": t.kind = 40; break;
			case "NERFZ": t.kind = 41; break;
			case "TIEMZD": t.kind = 42; break;
			case "OVARZ": t.kind = 43; break;
			case "IZ": t.kind = 45; break;
			case "YARLY": t.kind = 46; break;
			case "MEBBE": t.kind = 47; break;
			case "NOWAI": t.kind = 48; break;
			case "WTF": t.kind = 49; break;
			case "OMG": t.kind = 50; break;
			case "OMGWTF": t.kind = 51; break;
			case "BYES": t.kind = 52; break;
			case "DIAF": t.kind = 53; break;
			case "VISIBLE": t.kind = 54; break;
			case "INVISIBLE": t.kind = 55; break;
			case "LOL": t.kind = 57; break;
			case "R": t.kind = 58; break;
			case "AND": t.kind = 59; break;
			case "XOR": t.kind = 60; break;
			case "OR": t.kind = 61; break;
			case "NOT": t.kind = 62; break;
			case "BIGR": t.kind = 63; break;
			case "THAN": t.kind = 64; break;
			case "SMALR": t.kind = 65; break;
			case "LIEK": t.kind = 66; break;
			case "UP": t.kind = 67; break;
			case "NERF": t.kind = 68; break;
			case "TIEMZ": t.kind = 69; break;
			case "OVAR": t.kind = 70; break;
			case "NOOB": t.kind = 71; break;
			case "MAH": t.kind = 72; break;
			case "OF": t.kind = 73; break;
			case "MAEK": t.kind = 74; break;
			case "IS": t.kind = 75; break;
			case "NOW": t.kind = 76; break;
			case "FOUND": t.kind = 77; break;
			default: break;
		}
	}

	Token NextToken() {
		while (ch == ' ' || ch == 9 || ch == 13) NextCh();

		int apx = 0;
		t = new Token();
		t.pos = pos; t.col = col; t.line = line; 
		int state;
		try { state = start[ch]; } catch (KeyNotFoundException) { state = 0; }
		tlen = 0; AddCh();
		
		switch (state) {
			case -1: { t.kind = eofSym; break; } // NextCh already done
			case 0: { t.kind = noSym; break; }   // NextCh already done
			case 1:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 1;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 2:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 2;}
				else if (ch == 'E' || ch == 'e') {AddCh(); goto case 3;}
				else {t.kind = 3; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 3:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 5;}
				else if (ch == '+' || ch == '-') {AddCh(); goto case 4;}
				else {t.kind = noSym; break;}
			case 4:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 5;}
				else {t.kind = noSym; break;}
			case 5:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 5;}
				else {t.kind = 3; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 6:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 7;}
				else {t.kind = noSym; break;}
			case 7:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 7;}
				else if (ch == 'E' || ch == 'e') {AddCh(); goto case 8;}
				else {t.kind = 3; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 8:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 10;}
				else if (ch == '+' || ch == '-') {AddCh(); goto case 9;}
				else {t.kind = noSym; break;}
			case 9:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 10;}
				else {t.kind = noSym; break;}
			case 10:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 10;}
				else {t.kind = 3; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 11:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 13;}
				else if (ch == '+' || ch == '-') {AddCh(); goto case 12;}
				else {t.kind = noSym; break;}
			case 12:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 13;}
				else {t.kind = noSym; break;}
			case 13:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 13;}
				else {t.kind = 3; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 14:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '9' || ch >= ';' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 14;}
				else if (ch == '"') {AddCh(); goto case 15;}
				else if (ch == ':') {AddCh(); goto case 28;}
				else {t.kind = noSym; break;}
			case 15:
				{t.kind = 4; break;}
			case 16:
				{t.kind = 5; break;}
			case 17:
				if (ch == 10) {apx++; AddCh(); goto case 18;}
				else if (ch <= 9 || ch >= 11 && ch <= 65535) {AddCh(); goto case 17;}
				else {t.kind = noSym; break;}
			case 18:
				{
					tlen -= apx;
					buffer.Pos = t.pos; NextCh(); line = t.line; col = t.col;
					for (int i = 0; i < tlen; i++) NextCh();
					t.kind = 79; break;}
			case 19:
				if (ch <= 'S' || ch >= 'U' && ch <= 65535) {AddCh(); goto case 19;}
				else if (ch == 'T') {AddCh(); goto case 30;}
				else {t.kind = noSym; break;}
			case 20:
				{t.kind = 80; break;}
			case 21:
				if (ch == '.') {AddCh(); goto case 22;}
				else {t.kind = noSym; break;}
			case 22:
				if (ch == 10) {AddCh(); goto case 23;}
				else if (ch <= 9 || ch >= 11 && ch <= 65535) {AddCh(); goto case 22;}
				else {t.kind = noSym; break;}
			case 23:
				{t.kind = 81; break;}
			case 24:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 24;}
				else if (ch == '.') {AddCh(); goto case 6;}
				else if (ch == 'E' || ch == 'e') {AddCh(); goto case 11;}
				else {t.kind = 2; break;}
			case 25:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 2;}
				else if (ch == '.') {AddCh(); goto case 21;}
				else {t.kind = 5; break;}
			case 26:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'S' || ch >= 'U' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 1;}
				else if (ch == 'T') {AddCh(); goto case 31;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 27:
				if (ch >= '0' && ch <= '9' || ch == 'A' || ch >= 'C' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 1;}
				else if (ch == 'B') {AddCh(); goto case 32;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 28:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= 39 || ch >= ')' && ch <= '9' || ch >= ';' && ch <= 'Z' || ch >= ']' && ch <= 'z' || ch >= '|' && ch <= 65535) {AddCh(); goto case 14;}
				else if (ch == '"') {AddCh(); goto case 33;}
				else if (ch == ':') {AddCh(); goto case 28;}
				else if (ch == '(') {AddCh(); goto case 34;}
				else if (ch == '{') {AddCh(); goto case 35;}
				else if (ch == '[') {AddCh(); goto case 36;}
				else {t.kind = noSym; break;}
			case 29:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '9' || ch >= ';' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 29;}
				else if (ch == ':') {AddCh(); goto case 37;}
				else if (ch == '"') {AddCh(); goto case 15;}
				else {t.kind = noSym; break;}
			case 30:
				if (ch <= 'K' || ch >= 'M' && ch <= 65535) {AddCh(); goto case 19;}
				else if (ch == 'L') {AddCh(); goto case 38;}
				else {t.kind = noSym; break;}
			case 31:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'V' || ch >= 'X' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 1;}
				else if (ch == 'W') {AddCh(); goto case 39;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 32:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'S' || ch >= 'U' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 1;}
				else if (ch == 'T') {AddCh(); goto case 40;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 33:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '9' || ch >= ';' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 14;}
				else if (ch == '"') {AddCh(); goto case 15;}
				else if (ch == ':') {AddCh(); goto case 28;}
				else {t.kind = 4; break;}
			case 34:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '/' || ch >= ';' && ch <= '@' || ch >= 'G' && ch <= '[' || ch >= ']' && ch <= '`' || ch >= 'g' && ch <= 65535) {AddCh(); goto case 14;}
				else if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 41;}
				else if (ch == '"') {AddCh(); goto case 15;}
				else if (ch == ':') {AddCh(); goto case 28;}
				else {t.kind = noSym; break;}
			case 35:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '9' || ch >= ';' && ch <= '@' || ch == '[' || ch >= ']' && ch <= '^' || ch == '`' || ch >= '{' && ch <= 65535) {AddCh(); goto case 14;}
				else if (ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 42;}
				else if (ch == '"') {AddCh(); goto case 15;}
				else if (ch == ':') {AddCh(); goto case 28;}
				else {t.kind = noSym; break;}
			case 36:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '9' || ch >= ';' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 29;}
				else if (ch == ':') {AddCh(); goto case 37;}
				else if (ch == '"') {AddCh(); goto case 15;}
				else {t.kind = noSym; break;}
			case 37:
				if (ch == '[') {AddCh(); goto case 43;}
				else if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= 39 || ch >= ')' && ch <= '9' || ch >= ';' && ch <= 'Z' || ch >= ']' && ch <= 'z' || ch >= '|' && ch <= 65535) {AddCh(); goto case 29;}
				else if (ch == '"') {AddCh(); goto case 33;}
				else if (ch == ':') {AddCh(); goto case 37;}
				else if (ch == '(') {AddCh(); goto case 44;}
				else if (ch == '{') {AddCh(); goto case 45;}
				else {t.kind = noSym; break;}
			case 38:
				if (ch <= 'C' || ch >= 'E' && ch <= 65535) {AddCh(); goto case 19;}
				else if (ch == 'D') {AddCh(); goto case 46;}
				else {t.kind = noSym; break;}
			case 39:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 39;}
				else if (ch == 10) {apx++; AddCh(); goto case 18;}
				else if (ch <= 9 || ch >= 11 && ch <= '/' || ch >= ':' && ch <= '@' || ch >= '[' && ch <= '^' || ch == '`' || ch >= '{' && ch <= 65535) {AddCh(); goto case 17;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 40:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'V' || ch >= 'X' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 1;}
				else if (ch == 'W') {AddCh(); goto case 47;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 41:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '/' || ch >= ';' && ch <= '@' || ch >= 'G' && ch <= '[' || ch >= ']' && ch <= '`' || ch >= 'g' && ch <= 65535) {AddCh(); goto case 14;}
				else if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 48;}
				else if (ch == '"') {AddCh(); goto case 15;}
				else if (ch == ':') {AddCh(); goto case 28;}
				else {t.kind = noSym; break;}
			case 42:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '/' || ch >= ';' && ch <= '@' || ch == '[' || ch >= ']' && ch <= '^' || ch == '`' || ch >= '{' && ch <= 65535) {AddCh(); goto case 14;}
				else if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 42;}
				else if (ch == '"') {AddCh(); goto case 15;}
				else if (ch == ':') {AddCh(); goto case 28;}
				else {t.kind = noSym; break;}
			case 43:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '9' || ch >= ';' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 29;}
				else if (ch == ':') {AddCh(); goto case 37;}
				else if (ch == '"') {AddCh(); goto case 15;}
				else {t.kind = noSym; break;}
			case 44:
				if (ch == ':') {AddCh(); goto case 37;}
				else if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '/' || ch >= ';' && ch <= '@' || ch >= 'G' && ch <= '[' || ch >= ']' && ch <= '`' || ch >= 'g' && ch <= 65535) {AddCh(); goto case 29;}
				else if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 49;}
				else if (ch == '"') {AddCh(); goto case 15;}
				else {t.kind = noSym; break;}
			case 45:
				if (ch == ':') {AddCh(); goto case 37;}
				else if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '9' || ch >= ';' && ch <= '@' || ch == '[' || ch >= ']' && ch <= '^' || ch == '`' || ch >= '{' && ch <= 65535) {AddCh(); goto case 29;}
				else if (ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 50;}
				else if (ch == '"') {AddCh(); goto case 15;}
				else {t.kind = noSym; break;}
			case 46:
				if (ch <= 'Q' || ch >= 'S' && ch <= 65535) {AddCh(); goto case 19;}
				else if (ch == 'R') {AddCh(); goto case 20;}
				else {t.kind = noSym; break;}
			case 47:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 1;}
				else if (ch == 9 || ch >= 11 && ch <= 12 || ch == ' ') {AddCh(); goto case 19;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 48:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '/' || ch >= ';' && ch <= '@' || ch >= 'G' && ch <= '[' || ch >= ']' && ch <= '`' || ch >= 'g' && ch <= 65535) {AddCh(); goto case 14;}
				else if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 51;}
				else if (ch == '"') {AddCh(); goto case 15;}
				else if (ch == ':') {AddCh(); goto case 28;}
				else {t.kind = noSym; break;}
			case 49:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '/' || ch >= ';' && ch <= '@' || ch >= 'G' && ch <= '[' || ch >= ']' && ch <= '`' || ch >= 'g' && ch <= 65535) {AddCh(); goto case 29;}
				else if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 52;}
				else if (ch == '"') {AddCh(); goto case 15;}
				else if (ch == ':') {AddCh(); goto case 37;}
				else {t.kind = noSym; break;}
			case 50:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '/' || ch >= ';' && ch <= '@' || ch == '[' || ch >= ']' && ch <= '^' || ch == '`' || ch >= '{' && ch <= 65535) {AddCh(); goto case 29;}
				else if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 50;}
				else if (ch == '"') {AddCh(); goto case 15;}
				else if (ch == ':') {AddCh(); goto case 37;}
				else {t.kind = noSym; break;}
			case 51:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '/' || ch >= ';' && ch <= '@' || ch >= 'G' && ch <= '[' || ch >= ']' && ch <= '`' || ch >= 'g' && ch <= 65535) {AddCh(); goto case 14;}
				else if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 53;}
				else if (ch == '"') {AddCh(); goto case 15;}
				else if (ch == ':') {AddCh(); goto case 28;}
				else {t.kind = noSym; break;}
			case 52:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '/' || ch >= ';' && ch <= '@' || ch >= 'G' && ch <= '[' || ch >= ']' && ch <= '`' || ch >= 'g' && ch <= 65535) {AddCh(); goto case 29;}
				else if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 54;}
				else if (ch == '"') {AddCh(); goto case 15;}
				else if (ch == ':') {AddCh(); goto case 37;}
				else {t.kind = noSym; break;}
			case 53:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '9' || ch >= ';' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 14;}
				else if (ch == '"') {AddCh(); goto case 15;}
				else if (ch == ':') {AddCh(); goto case 28;}
				else {t.kind = noSym; break;}
			case 54:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '/' || ch >= ';' && ch <= '@' || ch >= 'G' && ch <= '[' || ch >= ']' && ch <= '`' || ch >= 'g' && ch <= 65535) {AddCh(); goto case 29;}
				else if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 55;}
				else if (ch == '"') {AddCh(); goto case 15;}
				else if (ch == ':') {AddCh(); goto case 37;}
				else {t.kind = noSym; break;}
			case 55:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '9' || ch >= ';' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 29;}
				else if (ch == '"') {AddCh(); goto case 15;}
				else if (ch == ':') {AddCh(); goto case 37;}
				else {t.kind = noSym; break;}
			case 56:
				{t.kind = 26; break;}
			case 57:
				{t.kind = 44; break;}
			case 58:
				if (ch == '!') {AddCh(); goto case 57;}
				else {t.kind = 56; break;}

		}
		t.val = new String(tval, 0, tlen);
		return t;
	}
	
	// get the next token (possibly a token already seen during peeking)
	public Token Scan () {
		if (tokens.next == null) {
			return NextToken();
		} else {
			pt = tokens = tokens.next;
			return tokens;
		}
	}

	// peek for the next token, ignore pragmas
	public Token Peek () {
		if (pt.next == null) {
			do {
				pt = pt.next = NextToken();
			} while (pt.kind > maxT); // skip pragmas
		} else {
			do {
				pt = pt.next;
			} while (pt.kind > maxT);
		}
		return pt;
	}
	
	// make sure that peeking starts at the current scan position
	public void ResetPeek () { pt = tokens; }

} // end Scanner

}