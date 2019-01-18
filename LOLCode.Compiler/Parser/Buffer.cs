using System.IO;

namespace LOLCode.Compiler.Parser
{
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
}
