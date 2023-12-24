using Magic_RDR.Application;
using System;
using System.Collections.Generic;
using System.IO;

namespace Magic_RDR
{
	public class NativeTable
	{
		public static List<string> _natives;
		private List<uint> _nativehash;

		public NativeTable(object reader, int position, int length)
		{
			int count = 0;
			uint nat;

			IOReader Reader = (IOReader)reader;
			Reader.BaseStream.Position = position;
			_natives = new List<string>();
			_nativehash = new List<uint>();

			while (count < length)
			{
				nat = Reader.ReadUInt32();
				_nativehash.Add(nat);

				if (NativeFile.HashDB.ContainsKey(nat))
					_natives.Add(NativeFile.HashDB[nat].ToUpper());
				else
				{
					string temps = nat.ToString("X");
					while (temps.Length < 8)
						temps = "0" + temps;
					_natives.Add("UNK_0x" + temps);
				}
				count++;
			}
		}

		public string[] GetNativeTable()
		{
			List<string> table = new List<string>();
			int i = 0;
			foreach (string native in _natives)
			{
				table.Add(i++.ToString("X2") + ": " + native);
			}
			return table.ToArray();
		}

		public string[] GetNativeHeader()
		{
			List<string> NativesHeader = new List<string>();
			foreach (uint hash in _nativehash)
			{
				NativesHeader.Add(ScriptFile.NativeInfo.GetNativeInfo(hash));
			}
			return NativesHeader.ToArray();
		}

		public string GetNativeFromIndex(int index)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException("Index must be a positive integer");
			if (index >= _natives.Count) throw
					new ArgumentOutOfRangeException("Index is greater than native table size");
			return _natives[index];
		}

		public uint GetNativeHashFromIndex(int index)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException("Index must be a positive integer");
			if (index >= _nativehash.Count)
				throw new ArgumentOutOfRangeException("Index is greater than native table size");
			return _nativehash[index];
		}
	}
}
