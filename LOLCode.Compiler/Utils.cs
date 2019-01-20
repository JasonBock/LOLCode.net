using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LOLCode.Compiler
{
	public abstract class Utils
	{
		public static string ReadWord(TextReader reader)
		{
			var sb = new StringBuilder();
			int c;
			while (true)
			{
				c = reader.Read();
				if (c == -1)
				{
					return string.Empty;
				}

				if (!char.IsWhiteSpace((char)c))
				{
					break;
				}
			}

			sb.Append(c);
			while (true)
			{
				c = reader.Read();
				if (c == -1 || char.IsWhiteSpace((char)c))
				{
					return sb.ToString();
				}

				sb.Append(c);
			}
		}

		public static bool ToBool(object b)
		{
			if (b is null)
			{
				return false;
			}

			if (b is int && ((int)b) == 0)
			{
				return false;
			}

			if (b is float && ((float)b) == 0)
			{
				return false;
			}

			if (b is string && ((string)b) == string.Empty)
			{
				return false;
			}

			if (b is bool && !(bool)b)
			{
				return false;
			}

			return true;
		}

		public static string ToString(object obj)
		{
			if (obj is null)
			{
				// TODO: 
				// Arguably this should throw ArgumentNullException
				throw new InvalidCastException("Cannot cast NOOB to string");
			}

			return obj.ToString();
		}

		public static int ToInt(object obj)
		{
			if (obj is int)
			{
				return (int)obj;
			}

			if (obj is float)
			{
				return (int)(float)obj;
			}

			if (obj is bool)
			{
				return ((bool)obj) ? 1 : 0;
			}

			if (obj is string)
			{
				if (!int.TryParse(obj as string, out var val))
				{
					throw new InvalidCastException("Cannot cast non-numeric YARN to NUMBR");
				}

				return val;
			}

			throw new InvalidCastException($"Cannot cast type \"{obj.GetType().Name}\" to NUMBR");
		}

		public static float ToFloat(object obj)
		{
			if (obj is int)
			{
				return (int)obj;
			}

			if (obj is float)
			{
				return (float)obj;
			}

			if (obj is bool)
			{
				return ((bool)obj) ? 1 : 0;
			}

			if (obj is string)
			{
				if (!float.TryParse(obj as string, out var val))
				{
					throw new InvalidCastException("Cannot cast non-numeric YARN to NUMBAR");
				}

				return val;
			}

			throw new InvalidCastException($"Cannot cast type \"{obj.GetType().Name}\" to NUMBAR");
		}

		public static void PrintObject(TextWriter writer, object obj, bool newline)
		{
			string pattern;
			if (newline)
			{
				pattern = "{0}" + Environment.NewLine;
			}
			else
			{
				pattern = "{0}";
			}

			if (obj.GetType() != typeof(Dictionary<object, object>))
			{
				writer.Write(pattern, obj);
			}
			else
			{
				var dict = obj as Dictionary<object, object>;
				foreach (var o2 in dict.Values)
				{
					writer.Write(pattern, o2);
				}
			}
		}
	}
}