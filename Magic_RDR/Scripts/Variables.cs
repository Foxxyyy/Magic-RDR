using Magic_RDR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Magic_RDR
{
	public class Variables
    {
		private ListType Listtype;
		private List<Var> Vars;
		private Dictionary<int, int> VarRemapper;
		private int ScriptParamCount = 0;
		private int ScriptParamStart { get { return Vars.Count - ScriptParamCount; } }
        
		public Variables(ListType type, int varcount)
        {
            Listtype = type;
            Vars = new List<Var>();
            for (int i = 0; i < varcount; i++)
            {
                Vars.Add(new Var(i));
            }
        }

        public Variables(ListType type)
        {
            Listtype = type;
			Vars = new List<Var>();
        }

        public void AddVar(int value)
        {
            Vars.Add(new Var(Vars.Count, value)); //only used for static variables that are pre assigned
        }

        public void AddVar(long value)
        {
            Vars.Add(new Var(Vars.Count, value));
        }

        public void CheckVariables()
        {
            UnusedCheck();
        }

		void BrokenCheck(uint index)
		{
			if (index >= Vars.Count)
			{
				for (int i = Vars.Count; i <= index; i++)
				{
					Vars.Add(new Var(i));
				}
			}
		}

        public string GetVarName(uint index)
        {
	        string name = "";
            Var var = Vars[(int)index];

            if (var.DataType == Stack.DataType.String)
				name = "c";
            else if (var.Immediatesize == 1)
				name = Types.gettype(var.DataType).varletter;
            else if (var.Immediatesize == 3 || var.Immediatesize == 9)
				name = "v";

            switch (Listtype)
            {
                case ListType.Statics: name += index >= ScriptParamStart ? "ScriptParam_" : "Local_"; break;
                case ListType.Vars: name += "Var"; break;
                case ListType.Params: name += "Param"; break;
            }

			return name + (Listtype == ListType.Statics && index >= ScriptParamStart ? index - ScriptParamStart : index).ToString();
        }

		public void SetScriptParamCount(int count)
		{
			if (Listtype == ListType.Statics)
			{
				ScriptParamCount = count;
			}
		}

		public string[] GetDeclaration(bool removeUnusedVars)
		{
			List<string> Working = new List<string>();
			string varlocation = "";
			string datatype = "";

			int i = 0;
			int j = -1;
			foreach (Var var in Vars)
			{
				switch(Listtype)
				{
					case ListType.Statics:
						varlocation = i >= ScriptParamStart ? "ScriptParam_" : "Local_";
						break;
					case ListType.Vars:
						varlocation = "Var";
						break;
					case ListType.Params:
						throw new Exception("Parameters have different declaration");
				}
				j++;

				if (!var.IsUsed)
				{
					i++;
					continue;
				}
				if (Listtype == ListType.Vars && !var.IsCalled)
				{
					i++;
					continue;
				}

				if (var.Immediatesize == 1)
					datatype = Types.gettype(var.DataType).vardec;			
				else if (var.Immediatesize == 3)
					datatype = "vector3 v";
				else if (var.DataType == Stack.DataType.String)
					datatype = "char* c";
				else
					datatype = "struct<" + var.Immediatesize.ToString() + "> ";

				string value = "";
				if (!var.IsArray)
				{
					if (Listtype == ListType.Statics)
					{
						if (var.Immediatesize == 1)
							value = " = " + DataUtils.Represent(Vars[j].Value, var.DataType);
						else if (var.DataType == Stack.DataType.String)
						{
							List<byte> data = new List<byte>();
							for (int l = 0; l < var.Immediatesize; l++)
							{
								data.AddRange(BitConverter.GetBytes(Vars[j + l].Value));
							}
							int len = data.IndexOf(0);
							data.RemoveRange(len, data.Count - len);
							value = " = \"" + Encoding.ASCII.GetString(data.ToArray()) + "\"";
						}
						else if (var.Immediatesize == 3)
						{

							value += " = { " + DataUtils.Represent(Vars[j].Value, Stack.DataType.Float) + ", ";
							value += DataUtils.Represent(Vars[j + 1].Value, Stack.DataType.Float) + ", ";
							value += DataUtils.Represent(Vars[j + 2].Value, Stack.DataType.Float) + " }";
						}
						else if (var.Immediatesize > 1)
						{
							value += " = { " + DataUtils.Represent(Vars[j].Value, Stack.DataType.Int);
							for (int l = 1; l < var.Immediatesize; l++)
							{
								try { value += ", " + DataUtils.Represent(Vars[j + l].Value, Stack.DataType.Int); }
								catch { }
							}
							value += " } ";
						}
					}
				}
				else
				{
					if (Listtype == ListType.Statics)
					{
						if (var.Immediatesize == 1)
						{
							value = " = { ";
							for (int k = 0; k < var.Value; k++)
							{
								value += DataUtils.Represent(Vars[j + 1 + k].Value, var.DataType) + ", ";
							}
							if (value.Length > 2)
							{
								value = value.Remove(value.Length - 2);
							}
							value += " }";
						}
						else if (var.DataType == Stack.DataType.String)
						{
							value = " = { ";
							for (int k = 0; k < var.Value; k++)
							{
								List<byte> data = new List<byte>();
								for (int l = 0; l < var.Immediatesize; l++)
								{
									data.AddRange(BitConverter.GetBytes((int)Vars[j + 1 + var.Immediatesize * k + l].Value));
								}
								value += "\"" + Encoding.ASCII.GetString(data.ToArray()) + "\", ";
							}
							if (value.Length > 2)
							{
								value = value.Remove(value.Length - 2);
							}
							value += " }";
						}
						else if (var.Immediatesize == 3)
						{
							value = " = {";
							for (int k = 0; k < var.Value; k++)
							{
								value += "{ " + DataUtils.Represent(Vars[j + 1 + 3 * k].Value, Stack.DataType.Float) + ", ";
								value += DataUtils.Represent(Vars[j + 2 + 3 * k].Value, Stack.DataType.Float) + ", ";
								value += DataUtils.Represent(Vars[j + 3 + 3 * k].Value, Stack.DataType.Float) + " }, ";
							}
							if (value.Length > 2)
							{
								value = value.Remove(value.Length - 2);
							}
							value += " }";
						}
					}
				}

				string decl = datatype + varlocation + (Listtype == ListType.Statics && i >= ScriptParamStart ? i - ScriptParamStart : i).ToString();
				if (var.IsArray)
					decl += "[" + var.Value.ToString() + "]";
				if (var.DataType == Stack.DataType.String)
					decl += "[" + (var.Immediatesize * 4).ToString() + "]";

				bool add = true;
				if (removeUnusedVars)
                {
					int.TryParse(value, out int parse);
					if (value == " = 0")
						add = false;
				}
				if (add)
					Working.Add(decl + value + ";");
				i++;
			}
			return Working.ToArray();
		}

		public string GetPDec()
		{
			if (Listtype != ListType.Params)
				throw new Exception("Only params use this declaration");

			string decl = "";
			int i = 0;

			foreach (Var var in Vars)
			{
				if (!var.IsUsed)
				{
					i++; //Shift variable
					continue;
				}
				
				string datatype = "";
				if (!var.IsArray)
				{
					if (var.DataType == Stack.DataType.String)
						datatype = "char* c";
					else if (var.Immediatesize == 1)
						datatype = Types.gettype(var.DataType).vardec;
					else if (var.Immediatesize == 3 || var.Immediatesize == 9)
						datatype = "vector3 v";
					else
						datatype = "struct<" + var.Immediatesize.ToString() + "> ";
				}
				else
				{
					if (var.DataType == Stack.DataType.String)
						datatype = "char** c";
					else if (var.Immediatesize == 1)
						datatype = Types.gettype(var.DataType).vararraydec;
					else if (var.Immediatesize == 3)
						datatype = "vector3[] v";
					else
						datatype = "struct<" + var.Immediatesize.ToString() + ">[] ";
				}
				decl += datatype + "Param" + i.ToString() + ", ";
				i++;
			}
			if (decl.Length > 2)
				decl = decl.Remove(decl.Length - 2);
			return decl;
		}

        //Remove unused vars from declaration and shift var indexes down
        private void UnusedCheck()
        {
            VarRemapper = new Dictionary<int, int>();
            for (int i = 0, k = 0; i < Vars.Count; i++)
            {
                if (!Vars[i].IsUsed)
                    continue;
                if (Listtype == ListType.Vars && !Vars[i].IsCalled)
                    continue;

                if (Vars[i].IsArray)
                {
                    for (int j = i + 1; j < Math.Min(Vars.Count, i + 1 + Vars[i].Value * Vars[i].Immediatesize); j++)
                    {
                        Vars[j].DontUse();
                    }
                }
                else if (Vars[i].Immediatesize > 1)
                {
                    for (int j = i + 1; j < Math.Min(Vars.Count, i + Vars[i].Immediatesize); j++)
                    {
						BrokenCheck((uint)j);
                        Vars[j].DontUse();
                    }
                }
                VarRemapper.Add(i, k);
                k++;
            }
        }

        public Stack.DataType GetTypeAtIndex(uint index)
        {
            return Vars[(int)index].DataType;
        }

        public void SetTypeAtIndex(uint index, Stack.DataType type)
        {
            Vars[(int)index].DataType = type;
        }

        public Var GetVarAtIndex(uint index)
        {
			BrokenCheck(index);
            return Vars[(int)index];
        }

        public class Var
        {
			public int Index { get { return _Index; } }
			public long Value { get { return _Value; } set { this._Value = value; } }
			public int Immediatesize { get { return _ImmediateSize; } set { _ImmediateSize = value; } }
			public bool IsUsed { get { return _IsUsed; } }
			public bool IsCalled { get { return _IsCalled; } }
			public bool IsArray { get { return _IsArray; } }
			public Stack.DataType DataType { get { return _Datatype; } set { _Datatype = value; } }

			private int _Index, _ImmediateSize;
			private long _Value;
			private bool _IsArray, _IsUsed, _IsStruct, _IsCalled = false;
			private Stack.DataType _Datatype;

			public Var(int index)
            {
                _Index = index;
				_Value = 0;
				_ImmediateSize = 1;
				_IsArray = false;
				_IsUsed = true;
				_Datatype = Stack.DataType.Unk;
            }

            public Var(int index, long Value)
            {
                _Index = index;
				_Value = Value;
				_ImmediateSize = 1;
				_IsArray = false;
				_IsUsed = true;
				_Datatype = Stack.DataType.Unk;
				_IsStruct = false;
            }
            
			public void MakeArray()
            {
                if (!_IsStruct)
					_IsArray = true;
            }

            public void Call()
            {
				_IsCalled = true;
            }

            public void MakeStructure()
            {
                DataType = Stack.DataType.Unk;
				_IsArray = false;
				_IsStruct = true;
            }

            public void DontUse()
            {
				_IsUsed = false;
            }
        }

        public enum ListType
        {
            Statics,
            Params,
            Vars
        }
    }
}
