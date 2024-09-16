using System;
using System.Collections.Generic;	  

namespace Magic_RDR
{
	class NativeParamInfo
	{
		Dictionary<uint, Tuple<Stack.DataType, Stack.DataType[]>> Natives;

		public NativeParamInfo()
		{
			Natives = new Dictionary<uint, Tuple<Stack.DataType, Stack.DataType[]>>();
		}

		public void UpdateNative(uint hash, Stack.DataType returns, params Stack.DataType[] param)
		{
			lock (ScriptViewerForm.ThreadLock)
			{
				if (!Natives.ContainsKey(hash))
				{
					Natives.Add(hash, new Tuple<Stack.DataType, Stack.DataType[]>(returns, param));
					return;
				}
			}

			Stack.DataType current = Natives[hash].Item1;
			Stack.DataType[] currentParam = Natives[hash].Item2;

			if (Types.gettype(current).precedence < Types.gettype(returns).precedence)
			{
				current = returns;
			}
			for (int i = 0; i < currentParam.Length; i++)
			{
				if (i >= param.Length) continue;
				if (Types.gettype(currentParam[i]).precedence < Types.gettype(param[i]).precedence)
				{
					currentParam[i] = param[i];
				}
			}
			Natives[hash] = new Tuple<Stack.DataType, Stack.DataType[]>(current, currentParam);
		}

		public bool UpdateParam(uint hash, Stack.DataType type, int paramindex)
		{
			if (!Natives.ContainsKey(hash))
				return false;

			Stack.DataType[] paramslist = Natives[hash].Item2;
			paramslist[paramindex] = type;
			Natives[hash] = new Tuple<Stack.DataType, Stack.DataType[]>(Natives[hash].Item1, paramslist);
			return true;
		}

		public Stack.DataType GetReturnType(uint hash)
		{
			if (!Natives.ContainsKey(hash))
				return Stack.DataType.Unk;
			return Natives[hash].Item1;
		}

		public Stack.DataType GetParameterType(uint hash, int index)
		{
			if (!Natives.ContainsKey(hash))
				return Stack.DataType.Unk;
			if (index >= Natives[hash].Item2.Length)
				return Stack.DataType.Unk;
			return Natives[hash].Item2[index];
		}

		public void UpdateReturnType(uint hash, Stack.DataType returns, bool over = false)
		{
			if (!Natives.ContainsKey(hash))
				return;
			if (Types.gettype(Natives[hash].Item1).precedence < Types.gettype(returns).precedence || over)
				Natives[hash] = new Tuple<Stack.DataType, Stack.DataType[]>(returns, Natives[hash].Item2);
		}

		public string GetNativeInfo(uint hash)
		{
			if (!Natives.ContainsKey(hash))
				throw new Exception("Native not found");

			string native, nativetype = "";
			bool isKnown = false;

			if (NativeFile.HashDB.ContainsKey(hash))
			{
				native = NativeFile.HashDB[hash].ToUpper();
			}
			else
			{
				native = hash.ToString("X");
				while (native.Length < 8) native = "0" + native;
				native = "UNK_0x" + native;
			}

			string dec = (isKnown ? nativetype : Types.gettype(Natives[hash].Item1).returntype) + native + "(";
			int max = Natives[hash].Item2.Length;
			
			if (max == 0)
				return dec + ");";

			for (int i = 0; i < max; i++)
			{
				dec += Types.gettype(Natives[hash].Item2[i]).vardec + i + ", ";
			}
			return dec.Remove(dec.Length - 2) + ");";
		}

		public bool StringTypeExists(string str) //Can be used in the future for proper natives types (iterators, layouts, actors, etc..)
		{
			foreach (Types.DataTypes type in Types._types)
			{
				if (type.singlename == str)
					return true;
			}
			return false;
		}

		public static string IntToHex(int value)
		{
			string str = value.ToString("X");
			while (str.Length < 8)
			{
				str = "0" + str;
			}
			return "0x" + str;
		}
	}
}
