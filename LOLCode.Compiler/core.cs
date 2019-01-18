using System;

namespace LOLCode.Compiler
{
	public static class Core
	{
		private static object FromString(string a)
		{
			if (a.IndexOf('.') == -1)
			{
				return int.Parse(a);
			}
			else
			{
				return float.Parse(a);
			}
		}

		[LOLCodeFunction]
		public static object SUM(object a, object b)
		{
			if (a is string)
			{
				a = FromString(a as string);
			}

			if (b is string)
			{
				b = FromString(b as string);
			}

			if (a is int && b is int)
			{
				return (int)a + (int)b;
			}
			else if (a is float && b is float)
			{
				return (float)a + (float)b;
			}
			else if (a is int && b is float)
			{
				return (int)a + (float)b;
			}
			else if (a is float && b is int)
			{
				return (float)a + (int)b;
			}
			else
			{
				throw new InvalidOperationException($"Cannot add types \"{a.GetType()}\" and \"{b.GetType()}\"");
			}
		}

		[LOLCodeFunction]
		public static object DIFF(object a, object b)
		{
			if (a is string)
			{
				a = FromString(a as string);
			}

			if (b is string)
			{
				b = FromString(b as string);
			}

			if (a is int && b is int)
			{
				return (int)a - (int)b;
			}
			else if (a is float && b is float)
			{
				return (float)a - (float)b;
			}
			else if (a is int && b is float)
			{
				return (int)a - (float)b;
			}
			else if (a is float && b is int)
			{
				return (float)a - (int)b;
			}
			else
			{
				throw new InvalidOperationException($"Cannot add types \"{a.GetType()}\" and \"{b.GetType()}\"");
			}
		}

		[LOLCodeFunction]
		public static object PRODUKT(object a, object b)
		{
			if (a is string)
			{
				a = FromString(a as string);
			}

			if (b is string)
			{
				b = FromString(b as string);
			}

			if (a is int && b is int)
			{
				return (int)a * (int)b;
			}
			else if (a is float && b is float)
			{
				return (float)a * (float)b;
			}
			else if (a is int && b is float)
			{
				return (int)a * (float)b;
			}
			else if (a is float && b is int)
			{
				return (float)a * (int)b;
			}
			else
			{
				throw new InvalidOperationException($"Cannot add types \"{a.GetType()}\" and \"{b.GetType()}\"");
			}
		}

		[LOLCodeFunction]
		public static object QUOSHUNT(object a, object b)
		{
			if (a is string)
			{
				a = FromString(a as string);
			}

			if (b is string)
			{
				b = FromString(b as string);
			}

			if (a is int && b is int)
			{
				return (int)a / (int)b;
			}
			else if (a is float && b is float)
			{
				return (float)a / (float)b;
			}
			else if (a is int && b is float)
			{
				return (int)a / (float)b;
			}
			else if (a is float && b is int)
			{
				return (float)a / (int)b;
			}
			else
			{
				throw new InvalidOperationException($"Cannot add types \"{a.GetType()}\" and \"{b.GetType()}\"");
			}
		}

		[LOLCodeFunction]
		public static object MOD(object a, object b)
		{
			if (a is string)
			{
				a = FromString(a as string);
			}

			if (b is string)
			{
				b = FromString(b as string);
			}

			if (a is int && b is int)
			{
				return (int)a % (int)b;
			}
			else if (a is float && b is float)
			{
				return (float)a % (float)b;
			}
			else if (a is int && b is float)
			{
				return (int)a % (float)b;
			}
			else if (a is float && b is int)
			{
				return (float)a % (int)b;
			}
			else
			{
				throw new InvalidOperationException($"Cannot add types \"{a.GetType()}\" and \"{b.GetType()}\"");
			}
		}

		[LOLCodeFunction]
		public static object BIGGR(object a, object b)
		{
			if (a is int && b is int)
			{
				return (int)a > (int)b ? a : b;
			}
			else if (a is float && b is float)
			{
				return (float)a > (float)b ? a : b;
			}
			else if (a is int && b is float)
			{
				return (int)a > (float)b ? a : b;
			}
			else if (a is float && b is int)
			{
				return (float)a > (int)b ? a : b;
			}
			else if (a is string && b is string)
			{
				return (a as string).CompareTo(b as string) > 0 ? a : b;
			}
			else
			{
				throw new InvalidOperationException($"Cannot add types \"{a.GetType()}\" and \"{b.GetType()}\"");
			}
		}

		[LOLCodeFunction]
		public static object SMALLR(object a, object b)
		{
			if (a is int && b is int)
			{
				return (int)a > (int)b ? b : a;
			}
			else if (a is float && b is float)
			{
				return (float)a > (float)b ? b : a;
			}
			else if (a is int && b is float)
			{
				return (int)a > (float)b ? b : a;
			}
			else if (a is float && b is int)
			{
				return (float)a > (int)b ? b : a;
			}
			else if (a is string && b is string)
			{
				return (a as string).CompareTo(b as string) > 0 ? b : a;
			}
			else
			{
				throw new InvalidOperationException($"Cannot add types \"{a.GetType()}\" and \"{b.GetType()}\"");
			}
		}

		[LOLCodeFunction]
		public static bool BOTH(bool a, bool b) => a && b;

		[LOLCodeFunction]
		public static bool EITHER(bool a, bool b) => a || b;

		[LOLCodeFunction]
		public static bool WON(bool a, bool b) => a ^ b;

		[LOLCodeFunction]
		public static bool NOT(bool a) => !a;

		[LOLCodeFunction]
		public static bool ALL(params bool[] args)
		{
			var ret = true;
			for (var i = 0; i < args.Length; i++)
			{
				ret &= args[i];
			}

			return ret;
		}

		[LOLCodeFunction]
		public static bool ANY(params bool[] args)
		{
			var ret = false;
			for (var i = 0; i < args.Length; i++)
			{
				ret |= args[i];
			}

			return ret;
		}

		[LOLCodeFunction]
		public static bool SAEM(object a, object b)
		{
			if (a is int && b is float)
			{
				return (int)a == (float)b;
			}

			if (a is float && b is int)
			{
				return (float)a == (int)b;
			}

			return a.Equals(b);
		}

		[LOLCodeFunction]
		public static bool DIFFRINT(object a, object b)
		{
			if (a is int && b is float)
			{
				return (int)a != (float)b;
			}

			if (a is float && b is int)
			{
				return (float)a != (int)b;
			}

			return !a.Equals(b);
		}

		[LOLCodeFunction]
		public static string SMOOSH(params string[] args) => string.Concat(args);

		[LOLCodeFunction]
		public static object UPPIN(object a)
		{
			if (a is string)
			{
				a = FromString(a as string);
			}

			if (a is int)
			{
				return (int)a + 1;
			}

			if (a is float)
			{
				return (float)a + 1;
			}

			throw new ArgumentException($"Cannot call UPPIN on value of type {a.GetType().Name}");
		}

		[LOLCodeFunction]
		public static object NERFIN(object a)
		{
			if (a is string)
			{
				a = FromString(a as string);
			}

			if (a is int)
			{
				return (int)a - 1;
			}

			if (a is float)
			{
				return (float)a - 1;
			}

			throw new ArgumentException($"Cannot call NERFIN on value of type {a.GetType().Name}");
		}
	}
}
