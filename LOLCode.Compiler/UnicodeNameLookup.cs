using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace LOLCode.Compiler
{
	internal abstract class UnicodeNameLookup
	{
		private static Dictionary<string, string> names = null;

		private static void LoadDictionary()
		{
			names = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
			var br = new BinaryReader(new GZipStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("LOLCode.Compiler.UnicodeNames.dat"), CompressionMode.Decompress));

			var count = br.ReadInt32();
			for (var i = 0; i < count; i++)
			{
				names.Add(br.ReadString(), br.ReadString());
			}

			br.BaseStream.Close();
		}

		public static string GetUnicodeCharacter(string name)
		{
			if (names == null)
			{
				LoadDictionary();
			}

			if (!names.TryGetValue(name, out var val))
			{
				return null;
			}

			return val;
		}
	}
}
