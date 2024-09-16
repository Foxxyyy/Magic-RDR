using Magic_RDR;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Magic_RDR
{
	public class Stack
	{
		List<StackValue> _stack;

		public DataType TopType
		{
			get
			{
				if (_stack.Count == 0)
					return DataType.Unk;
				return _stack[_stack.Count - 1].Datatype;
			}
		}

		public Stack()
		{
			_stack = new List<StackValue>();
		}

		public void Dispose()
		{
			_stack.Clear();
		}

		public void Push(string value, DataType Datatype = DataType.Unk)
		{
			_stack.Add(new StackValue(StackValue.Type.Literal, value, Datatype));
		}

		public void PushGlobal(string value)
		{
			_stack.Add(StackValue.Global(StackValue.Type.Literal, value));
		}

		public void PushPGlobal(string value)
		{
			_stack.Add(StackValue.Global(StackValue.Type.Pointer, value));
		}

		private void PushCond(string value)
		{
			_stack.Add(new StackValue(StackValue.Type.Literal, value, DataType.Bool));
		}

		private void Push(StackValue item)
		{
			_stack.Add(item);
		}

		public void PushString(string value)
		{
			_stack.Add(new StackValue(StackValue.Type.Literal, value.ToString(), DataType.StringPtr));
		}

		public void Push(params int[] values)
		{
			foreach (int value in values)
			{
				_stack.Add(new StackValue(StackValue.Type.Literal, unchecked ((uint) value).ToString(), DataType.Int));
			}
		}

		public void PushHexInt(uint value)
		{
			_stack.Add(new StackValue(StackValue.Type.Literal, DataUtils.FormatHexHash(value), DataType.Int));
		}

		public void PushVar(string value, Variables.Var Variable)
		{
			if (value.StartsWith("v") && Variable.Value == 0)
			{
				value += ".x";
			}
			_stack.Add(new StackValue(StackValue.Type.Literal, value, Variable));
		}

		public void PushPVar(string value, Variables.Var Variable)
		{
			_stack.Add(new StackValue(StackValue.Type.Pointer, value, Variable));
		}

		public void Push(float value)
		{
			_stack.Add(new StackValue(StackValue.Type.Literal, value.ToString() + (Math.Round(value) == value ? ".0f" : "f"), DataType.Float));
		}

		public void PushPointer(string value)
		{
			_stack.Add(new StackValue(StackValue.Type.Pointer, value));
		}

		private void PushStruct(string value, int size)
		{
			_stack.Add(new StackValue(value, size));
		}

		private void PushVector(string value)
		{
			_stack.Add(new StackValue(value, 3, true));
		}

		private void PushString(string value, int size)
		{
			_stack.Add(new StackValue(size, value));
		}

		private StackValue Pop()
		{
			int index = _stack.Count - 1;
			if (index < 0) return new StackValue(StackValue.Type.Literal, "StackVal");
			StackValue val = _stack[index];
			_stack.RemoveAt(index);
			return val;
		}

		public object Drop()
		{
			StackValue val = Pop();
			if (val.Value.Contains("(") && val.Value.EndsWith(")"))
				if (val.Value.IndexOf("(") > 4)
					return val.Value.ToString() + ";";
			return null;
		}

		private StackValue[] PopList(int size)
		{
			int count = 0;
			List<StackValue> items = new List<StackValue>();
			while (count < size)
			{
				StackValue top = Pop();
				switch (top.ItemType)
				{
					case StackValue.Type.Literal:
						items.Add(top);
						count++;
						break;
					case StackValue.Type.Pointer:
						if (top.isNotVar)
							items.Add(new StackValue(StackValue.Type.Literal, "&(" + top.Value + ")"));
						else
							items.Add(new StackValue(StackValue.Type.Literal, "&" + top.Value));
						count++;
						break;
					case StackValue.Type.Struct:
						count ++;
						items.Add(new StackValue(StackValue.Type.Literal, top.Value));
						break;
					default:
						throw new Exception("Unexpected Stack Type: " + top.ItemType.ToString());
				}
			}
			items.Reverse();
			return items.ToArray();
		}

		private StackValue[] PopTest(int size)
		{
			int count = 0;
			List<StackValue> items = new List<StackValue>();

			while (count < size)
			{
				StackValue top = Pop();
				switch (top.ItemType)
				{
					case StackValue.Type.Literal:
						items.Add(top);
						count++;
						break;
					case StackValue.Type.Pointer:
						if (top.isNotVar)
							items.Add(new StackValue(StackValue.Type.Literal, "&(" + top.Value + ")"));
						else
							items.Add(new StackValue(StackValue.Type.Literal, "&" + top.Value));
						count++;
						break;
					case StackValue.Type.Struct:
						//if (count + top.StructSize > size)
							//throw new Exception("Struct size too large");
						count += top.StructSize;
						items.Add(new StackValue(top.Value, top.StructSize));
						break;
					default:
						throw new Exception("Unexpected Stack Type: " + top.ItemType.ToString());
				}
			}
			items.Reverse();
			return items.ToArray();
		}

		private string PopVector()
		{
			StackValue[] data = PopList(3);
			switch (data.Length)
			{
				case 1:
					return data[0].Value;
				case 3:
					return "Vector(" + data[2].Value + ", " + data[1].Value + ", " + data[0].Value + ")";
				case 2:
					return "Vector(" + data[1].Value + ", " + data[0].Value + ")";
			}
			throw new Exception("Unexpected data length");
		}

		private StackValue Peek()
		{
			return _stack[_stack.Count - 1];		
		}

		public void Dup()
		{
			StackValue top = Peek();
			if (top.Value.Contains("(") && top.Value.Contains(")"))
				Push("Stack.Peek()");
			else
				Push(top);
		}

		public string PopLit()
		{
			StackValue val = Pop();
			if (val.ItemType != StackValue.Type.Literal)
			{
				if (val.ItemType == StackValue.Type.Pointer)
				{
					return "&" + val.Value;
				}
				else return val.Value;
            }			
			return val.Value;
		}

		private string PeekLit()
		{
			StackValue val = Peek();
			if (val.ItemType != StackValue.Type.Literal)
			{
				if(val.ItemType == StackValue.Type.Pointer)
				{
					return "&" + val.Value;
				}
				else
					throw new Exception("Not a literal item recieved");
			}
			return val.Value;
		}

		private string PeekPointerRef()
		{
			StackValue val = Peek();
			if (val.ItemType == StackValue.Type.Pointer)
				return val.Value;
			else if (val.ItemType == StackValue.Type.Literal)
				return "*(" + val.Value + ")";
			throw new Exception("Not a pointer item recieved");
		}

		private string PopPointer()
		{
			StackValue val = Pop();
			if (val.ItemType == StackValue.Type.Pointer || val.ItemType == StackValue.Type.Struct)
			{
				if (val.isNotVar)
					return "&(" + val.Value + ")";
				else
					return "&" + val.Value;
			}
			else if (val.ItemType == StackValue.Type.Literal) return val.Value;
			throw new Exception("Not a pointer item recieved");
		}

		private string PopPointerRef()
		{
			StackValue val = Pop();
			if (val.ItemType == StackValue.Type.Pointer)
				return val.Value;
			else if (val.ItemType == StackValue.Type.Literal)
				if (Function.PushStringNull == true)
				{
					Function.PushStringNull = false;
					return val.Value.Contains(" ") ? "(" + val.Value + ")" : val.Value;
				}
				else return "*" + (val.Value.Contains(" ") ? "(" + val.Value + ")" : val.Value);
			throw new Exception("Not a pointer item recieved");
		}

		public string FormatInteger(string value)
		{
			int val;
			if (!int.TryParse(value, out val))
				return value.ToString();
			if (val > 0xFFFFFF || val < -0xFFFFFF)
				return string.Format("0x{0:x}", val);
			else
				return val.ToString();
		}

		public string PopListForCall(int size)
		{
			if (size == 0)
				return "";
			string items = "";

			foreach (StackValue val in PopList(size))
			{
				switch (val.ItemType)
				{
					case StackValue.Type.Literal:
						string newVal = FormatInteger(val.Value);
						items += newVal + ", ";
						break;
					case StackValue.Type.Pointer:
						if (val.isNotVar)
							items += "&(" + val.Value + "), ";
						else
							items += "&" + val.Value + ", ";
						break;
					case StackValue.Type.Struct:
						items += val.Value + ", ";
						break;
					default:
						throw new Exception("Unexpected Stack Type\n" + val.ItemType.ToString());
				}
			}
			return items.Remove(items.Length - 2);
		}

		private string[] EmptyStack()
		{
			List<string> stack = new List<string>();
			foreach (StackValue val in _stack)
			{
				switch (val.ItemType)
				{
					case StackValue.Type.Literal:
						stack.Add(val.Value);
						break;
					case StackValue.Type.Pointer:
						if (val.isNotVar)
							stack.Add("&(" + val.Value + ")");
						else
							stack.Add("&" + val.Value);
						break;
					case StackValue.Type.Struct:
						stack.Add(val.Value);
						break;
					default:
						throw new Exception("Unexpeced Stack Type\n" + val.ItemType.ToString());
				}
			}
			_stack.Clear();
			return stack.ToArray();
		}

		public string FunctionCall(string name, int pcount, int rcount)
		{
			string functionline = name + "(" + PopListForCall(pcount) + ")";
			if (rcount == 0) return functionline + ";";
			else if (rcount == 1)
			{
				Push(functionline);
				return functionline + ";";
			}
			else if (rcount > 1)
			{
				PushStruct(functionline, rcount);
				return functionline + ";";
			}
			throw new Exception("Error in return items count");
		}
		public string FunctionCall(Function function)
		{
			string functionline = function.Name + "(" + PopListForCall(function.Pcount) + ")";
			if (function.Rcount == 0) return functionline + ";";
			else if (function.Rcount == 1)
			{
				Push(new StackValue(StackValue.Type.Literal, functionline, function));
				return functionline + ";";
			}
			else if (function.Rcount > 1)
			{
				PushStruct(functionline, function.Rcount);
				return functionline + ";";
			}
			throw new Exception("Error in return items count");
		}

		public string NativeCallTest(uint hash, string name, int pcount, int rcount)
		{
			string functionline = name + "(";
			
			List<DataType> _params = new List<DataType>();
			int count = 0;
			foreach (StackValue val in PopTest(pcount))
			{
				switch (val.ItemType)
				{
					case StackValue.Type.Literal:
						if (val.Variable != null)
						{
							if (Types.gettype(val.Variable.DataType).precedence < Types.gettype(ScriptFile.NativeInfo.GetParameterType(hash, count)).precedence)
							{
								val.Variable.DataType = ScriptFile.NativeInfo.GetParameterType(hash, count);
							}
							else if (Types.gettype(val.Variable.DataType).precedence > Types.gettype(ScriptFile.NativeInfo.GetParameterType(hash, count)).precedence)
							{
								ScriptFile.NativeInfo.UpdateParam(hash, val.Variable.DataType, count);
							}
						}
						if (val.Datatype == DataType.Bool || ScriptFile.NativeInfo.GetParameterType(hash, count) == DataType.Bool)
						{
							if (val.Value == "0")
								functionline += "false, ";
							else if (val.Value == "1")
								functionline += "true, ";
							else
								functionline += val.Value + ", ";
						}
						else if (val.Datatype == DataType.Int && ScriptFile.NativeInfo.GetParameterType(hash, count) == DataType.Float)
						{
							uint tempu;
							if (uint.TryParse(val.Value, out tempu))
							{
								tempu = DataUtils.SwapEndian(tempu);
								float floatval = DataUtils.SwapEndian(BitConverter.ToSingle(BitConverter.GetBytes(tempu), 0));
								functionline += floatval.ToString() + "f, ";
							}
							else
								functionline += val.Value + ", ";
						}
						else
							functionline += val.Value + ", ";
						_params.Add(val.Datatype);
						count++;
						break;
					case StackValue.Type.Pointer:
						if (val.isNotVar)
							functionline += "&(" + val.Value + "), ";
						else
							functionline += "&" + val.Value + ", ";
						if (Types.hasptr(val.Datatype))
							_params.Add(Types.getpointerver(val.Datatype));
						else
							_params.Add(val.Datatype);
						count++;
						break;
					case StackValue.Type.Struct:
						functionline += val.Value + ", ";
						if (val.StructSize == 3 && val.Datatype == DataType.Vector3)
						{
							_params.AddRange(new DataType[] {DataType.Float, DataType.Float, DataType.Float});
							count += 3;
						}
						else
						{
							for (int i = 0; i < val.StructSize; i++)
							{
								_params.Add(DataType.Unk);
								count++;
							}
						}
						break;
					default:
						throw new Exception("Unexpected Stack Type\n" + val.ItemType.ToString());
				}
			}
			if (pcount > 0)
				functionline = functionline.Remove(functionline.Length - 2) + ")";
			else
				functionline += ")";
			if (rcount == 0)
			{
				ScriptFile.NativeInfo.UpdateNative(hash, DataType.None, _params.ToArray());
				return functionline + ";";
			}
			else if (rcount == 1)
			{
				ScriptFile.NativeInfo.UpdateNative(hash, ScriptFile.NativeInfo.GetReturnType(hash), _params.ToArray());
				PushNative(functionline, hash, ScriptFile.NativeInfo.GetReturnType(hash));
			}
			else if (rcount > 1)
			{
				if (rcount == 2)
					ScriptFile.NativeInfo.UpdateNative(hash, DataType.Unk, _params.ToArray());
				else if (rcount == 3)
					ScriptFile.NativeInfo.UpdateNative(hash, DataType.Vector3, _params.ToArray());
				else
					throw new Exception("Error in return items count");
				PushStructNative(functionline, hash, rcount, ScriptFile.NativeInfo.GetReturnType(hash));
			}
			else throw new Exception("Error in return items count");
			return "";
		}

		#region Opcodes

		public void Op_Add()
		{
			var s1 = Pop();
			var s2 = Pop();

			if (s1.ItemType == StackValue.Type.Literal && s2.ItemType == StackValue.Type.Literal)
			{
				Push("(" + s2.Value + " + " + s1.Value + ")", DataType.Int);
				return;
			}
			if (s2.ItemType == StackValue.Type.Pointer && s1.ItemType == StackValue.Type.Literal)
			{
				Push("(&" + s2.Value + " + " + s1.Value + ")", DataType.Unk);
				return;
			}
			else if (s1.ItemType == StackValue.Type.Pointer && s2.ItemType == StackValue.Type.Literal)
			{
				Push("(&" + s1.Value + " + " + s2.Value + ")", DataType.Unk);
				return;
			}
			else
			{
                Push("(" + s2.Value + " + " + s1.Value + ")", DataType.Int);
                return;
            }
		}

		public void Op_Addf()
		{
			string s1, s2;
			s1 = PopLit();
			s2 = PopLit();
			Push("(" + s2 + " + " + s1 + ")", DataType.Float);
		}

		public void Op_Sub()
		{
			StackValue s1, s2;
			s1 = Pop();
			s2 = Pop();
			if (s1.ItemType == StackValue.Type.Literal && s2.ItemType == StackValue.Type.Literal)
			{
				Push("(" + s2.Value + " - " + s1.Value + ")", DataType.Int);
				return;
			}
			if (s2.ItemType == StackValue.Type.Pointer && s1.ItemType == StackValue.Type.Literal)
			{
				Push("(&" + s2.Value + " - " + s1.Value + ")", DataType.Unk);
				return;
			}
			else if (s1.ItemType == StackValue.Type.Pointer && s2.ItemType == StackValue.Type.Literal)
			{
				Push("(&" + s1.Value + " - " + s2.Value + ")", DataType.Unk);
				return;
			}
			else
            {
                Push("(" + s2.Value + " - " + s1.Value + ")", DataType.Int);
                return;
            }
        }

		public void Op_Subf()
		{
			string s1, s2;
			s1 = PopLit();
			s2 = PopLit();
			Push("(" + s2 + " - " + s1 + ")", DataType.Float);
		}

		public void Op_Mult()
		{
			string s1, s2;
			s1 = PopLit();
			s2 = PopLit();
			Push("(" + s2 + " * " + s1 + ")", DataType.Int);
		}

		public void Op_Multf()
		{
			string s1, s2;
			s1 = PopLit();
			s2 = PopLit();
			Push("(" + s2 + " * " + s1 + ")", DataType.Float);
		}

		public void Op_Div()
		{
			string s1, s2;
			s1 = PopLit();
			s2 = PopLit();
			Push("(" + s2 + " / " + s1 + ")", DataType.Int);
		}

		public void Op_Divf()
		{
			string s1, s2;
			s1 = PopLit();
			s2 = PopLit();
			Push("(" + s2 + " / " + s1 + ")", DataType.Float);
		}

		public void Op_Mod()
		{
			string s1, s2;
			s1 = PopLit();
			s2 = PopLit();
			Push("(" + s2 + " % " + s1 + ")", DataType.Int);
		}

		public void Op_Modf()
		{
			string s1, s2;
			s1 = PopLit();
			s2 = PopLit();
			Push("(" + s2 + " % " + s1 + ")", DataType.Float);
		}

		public void Op_Not()
		{
			string s1;
			s1 = PopLit();
			if (s1.StartsWith("!(") && s1.EndsWith(")"))
				PushCond(s1.Remove(s1.Length - 1).Substring(2));
			else if (s1.StartsWith("(") && s1.EndsWith(")"))
				PushCond("!" + s1);
			else if (!(s1.Contains("&&") && s1.Contains("||") && s1.Contains("^")))
			{
				if (s1.StartsWith("!"))
					PushCond(s1.Substring(1));
				else
					PushCond("!" + s1);
			}
			else
				PushCond("!(" + s1 + ")");
		}

		public void Op_Neg()
		{
			string s1;
			s1 = PopLit();
			Push("-" + s1, DataType.Int);
		}

		public void Op_Negf()
		{
			string s1;
			s1 = PopLit();
			Push("-" + s1, DataType.Float);
		}

		public void Op_CmpEQ()
		{
			string s1, s2;
			s1 = PopLit();
			s2 = PopLit();
			PushCond(s2 + " != " + s1);

		}

		public void Op_CmpNE()
		{
			string s1, s2;
			s1 = PopLit();
			s2 = PopLit();
			PushCond(s2 + " == " + s1);
		}

		public void Op_CmpGE()
		{
			string s1, s2;
			s1 = PopLit();
			s2 = PopLit();
			PushCond(s2 + " <= " + s1);
		}

		public void Op_CmpGT()
		{
			string s1, s2;
			s1 = PopLit();
			s2 = PopLit();
			PushCond(s2 + " < " + s1);
		}

		public void Op_CmpLE()
		{
			string s1, s2;
			s1 = PopLit();
			s2 = PopLit();
			PushCond(s2 + " >= " + s1);
		}

		public void Op_CmpLT()
		{
			string s1, s2;
			s1 = PopLit();
			s2 = PopLit();
			PushCond(s2 + " > " + s1);
		}

		public void Op_Vadd()
		{
			string s1, s2;
			s1 = PopVector();
			s2 = PopVector();
			PushVector(s2 + " + " + s1);
		}

		public void Op_VSub()
		{
			string s1, s2;
			s1 = PopVector();
			s2 = PopVector();
			PushVector(s2 + " - " + s1);
		}

		public void Op_VMult()
		{
			string s1, s2;
			s1 = PopVector();
			s2 = PopVector();
			PushVector(s2 + " * " + s1);
		}

		public void Op_VDiv()
		{
			string s1, s2;
			s1 = PopVector();
			s2 = PopVector();
			PushVector(s2 + " / " + s1);
		}

		public void Op_VNeg()
		{
			string s1;
			s1 = PopVector();
			PushVector("-" + s1);
		}

		public void Op_FtoV()
		{
			StackValue top = Pop();
			if (top.Value.Contains("(") && top.Value.Contains(")"))
				PushVector("FtoV(" + top.Value + ")");
			else
			{
				Push(top.Value, DataType.Float);
				Push(top.Value, DataType.Float);
				Push(top.Value, DataType.Float);
			}

		}

		public void Op_Itof()
		{
			Push("IntToFloat(" + PopLit() + ")", DataType.Float);
		}

		public void Op_FtoI()
		{
			Push("FloatToInt(" + PopLit() + ")", DataType.Int);
		}

		public void Op_And()
		{
			StackValue s1 = Pop();
			StackValue s2 = Pop();

            if (s1.ItemType != StackValue.Type.Literal && s2.ItemType != StackValue.Type.Literal)
                throw new Exception("Not a literal item recieved");

            if (s1.Datatype == DataType.Bool || s2.Datatype == DataType.Bool)
				PushCond("(" + s2.Value + " && " + s1.Value + ")");
			else if (DataUtils.IntParse(s1.Value, out int temp) || DataUtils.IntParse(s2.Value, out temp))
				Push(s2.Value + " & " + s1.Value, DataType.Int);
			else
				Push("(" + s2.Value + " && " + s1.Value + ")");
		}

		public void Op_Or()
		{
			StackValue s1 = Pop();
			StackValue s2 = Pop();

			if (s1.ItemType != StackValue.Type.Literal && s2.ItemType != StackValue.Type.Literal)
				throw new Exception("Not a literal item recieved");

            if (s1.Datatype == DataType.Bool || s2.Datatype == DataType.Bool)
				PushCond("(" + s2.Value + " || " + s1.Value + ")");
			else if (DataUtils.IntParse(s1.Value, out int temp) || DataUtils.IntParse(s2.Value, out temp))
				Push(s2.Value + " | " + s1.Value, DataType.Int);
			else
				Push("(" + s2.Value + " || " + s1.Value + ")");
		}

		public void Op_Xor()
		{
			string s1, s2;
			s1 = PopLit();
			s2 = PopLit();
			Push(s2 + " ^ " + s1, DataType.Int);
		}

		string PopStructAccess()
		{
			StackValue val = Pop();
			if (val.ItemType == StackValue.Type.Pointer)
				return val.Value + ".";
			else if (val.ItemType == StackValue.Type.Literal)
				return (val.Value.Contains(" ") ? "(" + val.Value + ")" : val.Value) + "->";
			throw
				new Exception("Not a pointer item recieved");
		}

		public void Op_GetImm(uint immediate)
		{
			if (immediate == 4 || immediate == 8)
			{
				string val;
				switch (immediate)
				{
					case 4:
						val = PopStructAccess();
						if (!val.StartsWith("v")) return;
						Push(new StackValue(StackValue.Type.Literal, val + "y"));
						return;
					case 8:
						val = PopStructAccess();
						if (!val.StartsWith("v")) return;
						Push(new StackValue(StackValue.Type.Literal, val + "z"));
						return;
				}
			}
			Push(new StackValue(StackValue.Type.Literal, PopStructAccess() + "f_" + immediate.ToString()));
		}

		public string Op_SetImm(uint immediate)
		{
			string imm = "f_" + immediate.ToString();
			string pointer = PopStructAccess();
			if (PeekVar(0) != null && pointer.StartsWith("v"))
			{
				if (immediate == 4 || immediate == 8)
				{
					switch (immediate)
					{
						case 4:
							imm = "y";
							break;
						case 8:
							imm = "z";
							break;
					}
				}
			}
			//string pointer = PopStructAccess();
			string valuePop = PopLit();
			return setcheck(pointer + imm, valuePop);
		}

		/// <summary>
		/// returns a string saying the size of an array if its > 1
		/// </summary>
		/// <param name="immediate"></param>
		/// <returns></returns>
		private string getarray(uint immediate)
		{
			if (immediate == 1)
				return "";
			return immediate.ToString();
		}

		public string PopArrayAccess()
		{
			StackValue val = Pop();
			if(val.ItemType == StackValue.Type.Pointer)
				return val.Value;
			else if(val.ItemType == StackValue.Type.Literal)
				return $"(*{val.Value})";
			throw new Exception("Not a pointer item recieved");
		}

		public void Op_ArrayGet(uint immediate)
		{
			string arrayloc = PopArrayAccess();
			string index = PopLit();
			Push(new StackValue(StackValue.Type.Literal, arrayloc + "[" + index + getarray(immediate) + "]"));
		}

		public string Op_ArraySet(uint immediate)
		{
			string arrayloc = PopArrayAccess();
			string index = PopLit();
			string value = PopLit();
			return setcheck(arrayloc + "[" + index + getarray(immediate) + "]", value);
		}

		public void Op_ArrayGetP(uint immediate)
		{
			string arrayloc;
			string index;
			if (Peek().ItemType == StackValue.Type.Pointer)
			{
				arrayloc = PopArrayAccess();
				index = PopLit();
				Push(new StackValue(StackValue.Type.Pointer, arrayloc + "[" + index + getarray(immediate) + "]"));
			}
			else if (Peek().ItemType == StackValue.Type.Literal)
			{
				arrayloc = PopLit();
				index = PopLit();
				Push(new StackValue(StackValue.Type.Literal, arrayloc + "[" + index + getarray(immediate) + "]"));
			}
			else throw new Exception("Unexpected Stack Value :" + Peek().ItemType.ToString());
		}

		public void Op_RefGet()
		{
			Push(new StackValue(StackValue.Type.Literal, PopPointerRef()));
		}

		public void Op_ToStack()
		{
			string pointer, count;
			int amount = 0;
			if (TopType == DataType.StringPtr || TopType == DataType.String)
			{
				pointer = PopPointerRef();
				count = PopLit();
				if (!DataUtils.IntParse(count, out amount)) throw new Exception("Expecting the amount to push");
				PushString(pointer, amount);
			}
			else
			{
				pointer = PopPointerRef();
				count = PopLit();
				if (!DataUtils.IntParse(count, out amount)) throw new Exception("Expecting the amount to push");
				PushStruct(pointer, amount);
			}
		}

		int GetIndex(int index)
		{
			int actindex = 0;
			if (_stack.Count == 0)
			{
				return -1;
			}
			for (int i = 0; i < index; i++)
			{
				int stackIndex = _stack.Count - i - 1;
				if (stackIndex < 0) return -1;
				if (_stack[stackIndex].ItemType == StackValue.Type.Struct && _stack[stackIndex].Datatype != DataType.Vector3)
				{
					index -= _stack[stackIndex].StructSize - 1;
				}
				if (i < index) actindex++;
			}
			return actindex < _stack.Count ? actindex : -1;
		}

		public string PeekItem(int index)
		{
			int newIndex = GetIndex(index);
			if (newIndex == -1)
			{
				return "";
			}
			StackValue val = _stack[_stack.Count - newIndex - 1];
			if (val.ItemType != StackValue.Type.Literal)
			{
				if(val.ItemType == StackValue.Type.Pointer)
				{
					return "&" + val.Value;
				}
				else throw new Exception("Not a literal item recieved");
			}
			return val.Value;
		}

		public Variables.Var PeekVar(int index)
		{
			int newIndex = GetIndex(index);
			if (newIndex == -1) return null;
			return _stack[_stack.Count - newIndex - 1].Variable;
		}

		public Function PeekFunc(int index)
		{
			int newIndex = GetIndex(index);
			if(newIndex == -1) return null;
			return _stack[_stack.Count - newIndex - 1].Function;
		}
		public uint PeekNat(int index)
		{
			int newIndex = GetIndex(index);
			if (newIndex == -1) return 0;
			return _stack[_stack.Count - newIndex - 1].NatHash;
		}

		public ulong PeekNat64(int index)
		{
			int newIndex = GetIndex(index);
			if (newIndex == -1) return 0;
			return _stack[_stack.Count - newIndex - 1].X64NatHash;
		}

		public bool isnat(int index)
		{
			int newIndex = GetIndex(index);
			if (newIndex == -1) return false;
			return _stack[_stack.Count - newIndex - 1].isNative;
		}

		public bool isPointer(int index)
		{
			int newIndex = GetIndex(index);
			if (newIndex == -1)
			{
				return false;
			}
			return _stack[_stack.Count - newIndex - 1].ItemType == StackValue.Type.Pointer;
		}

		public bool isLiteral(int index)
		{
			int newIndex = GetIndex(index);
			if (newIndex == -1)
			{
				return false;
			}
			return _stack[_stack.Count - newIndex - 1].ItemType == StackValue.Type.Literal;
		}

		public void PushNative(string value, uint hash, DataType type)
		{
			Push(new StackValue(value, hash, type));
		}

		public void PushNative(string value, ulong hash, DataType type)
		{
			Push(new StackValue(value, hash, type));
		}

		public void PushStructNative(string value, uint hash, int structsize, DataType dt = DataType.Unk)
		{
			Push(new StackValue(value, structsize, hash, dt));
		}

		public void PushStructNative(string value, ulong hash, int structsize, DataType dt = DataType.Unk)
		{
			Push(new StackValue(value, structsize, hash, dt));
		}

		public DataType ItemType(int index)
		{
			int newIndex = GetIndex(index);
			if (newIndex == -1)
			{
				return DataType.Unk;
			}
			return _stack[_stack.Count - newIndex - 1].Datatype;
		}

		public string Op_FromStack()
		{
			string pointer, count;
			pointer = PopPointerRef();
			count = PopLit();
			int amount = 0;
			int.TryParse(count, out amount);
			string res = pointer + " = { ";
			foreach (StackValue val in PopList(amount)) res += val.Value + ", ";
			return res.Remove(res.Length - 2) + " };";
		}

		public void Op_AmmImm(int immediate)
		{
			if (immediate < 0) Push(PopLit() + " - " + (-immediate).ToString());
			else if (immediate == 0) { }
			else Push(PopLit() + " + " + immediate.ToString());
		}

		public void Op_MultImm(int immediate)
		{
			Push(PopLit() + " * " + immediate.ToString());
		}

		public string Op_RefSet()
		{
			string pointer, value;
			pointer = PopPointerRef();
			value = PopLit();
			return setcheck(pointer, value);
		}

		public string Op_PeekSet()
		{
			string pointer, value;
			value = PopLit();
			pointer = PeekPointerRef();
			return setcheck(pointer, value);
		}

		public string Op_Set(string location)
		{
			return setcheck(location, PopLit());
		}

		public string Op_Set(string location, Variables.Var Variable)
		{
			if (Variable.Immediatesize == 3)
			{
				location += ".x";
			}
			return Op_Set(location);
		}

		public void Op_Hash()
		{
			Push("Hash(" + PopLit() + ")", DataType.Int);
		}

		public string op_strcopy(int size)
		{
			string pointer = PopPointer();
			string pointer2 = PopPointer();
			return "strcpy(" + pointer + ", " + pointer2 + ", " + size.ToString() + ");";
		}

		public string op_stradd(int size)
		{
			string pointer = PopPointer();
			string pointer2 = PopPointer();
			return "stradd(" + pointer + ", " + pointer2 + ", " + size.ToString() + ");";
		}

		public string op_straddi(int size)
		{
			string pointer = PopPointer();
			string inttoadd = PopLit();
			return "straddi(" + pointer + ", " + inttoadd + ", " + size.ToString() + ");";
		}

		public string op_itos(int size)
		{
			string pointer = PopPointer();
			string intval = PopLit();
			return "itos(" + pointer + ", " + intval + ", " + size.ToString() + ");";
		}

		public string op_sncopy()
		{
			string pointer = PopPointer();
			string value = PopLit();
			string count = PopLit();
			int amount;

			if (!DataUtils.IntParse(count, out amount))
				throw new Exception("Int Stack value expected");
			return "memcpy(" + pointer + ", " + PopListForCall(amount) + ", " + value + ");";
		}

		public string[] pcall()
		{
			List<string> temp = new List<string>();
			string loc = PopLit();
			foreach (string s in EmptyStack())
			{
				temp.Add("Stack.Push(" + s + ");");
			}
			temp.Add("Call_Loc(" + loc + ");");
			return temp.ToArray();
		}

		/// <summary>
		/// Detects if you can use var++, var *= num etc
		/// </summary>
		/// <param name="loc"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public string setcheck(string loc, string value)
		{
			if (!value.StartsWith(loc + " "))
				return loc + " = " + value + ";";

			string temp = value.Substring(loc.Length + 1);
			string op = temp.Remove(temp.IndexOf(' '));
			string newval = temp.Substring(temp.IndexOf(' ') + 1);
			if (newval == "1" || newval == "1f")
			{
				if (op == "+") return loc + "++;";
				if (op == "-") return loc + "--;";
			}
			return loc + " " + op + "= " + newval + ";";
		}

		#endregion

		#region subclasses

		public enum DataType
		{
			Int,
			IntPtr,
			Float,
			FloatPtr,
			String,
			StringPtr,
			Bool,
			Unk,
			UnkPtr,
			Unsure,
			None, //For Empty returns
			Vector3
		}

		private class StackValue
		{
			public enum Type
			{
				Literal,
				Pointer,
				Struct
			}

			string _value;
			Type _type;
			int _structSize;
			DataType _datatype;
			Variables.Var _var = null;
			uint _hash = 0;
			ulong _xhash = 0;
			bool global = false;
			Function _function = null;

			public StackValue(Type type, string value)
			{
				_type = type;
				_value = value;
				_structSize = 0;
				_datatype = DataType.Unk;
			}

			public StackValue(Type type, string name, Variables.Var var)
			{
				_type = type;
				_value = name;
				_structSize = 0;
				_datatype = var.DataType;
				_var = var;
			}

			public StackValue(Type type, string name, Function function)
			{
				_type = type;
				_value = name;
				_structSize = 0;
				_datatype = function.ReturnType.type;
				_function = function;
			}

			public StackValue(Type type, string value, DataType datatype)
			{
				_type = type;
				_value = value;
				_structSize = 0;
				_datatype = datatype;
			}

			public StackValue(string value, uint hash, DataType datatype)
			{
				_type = Type.Literal;
				_value = value;
				_structSize = 0;
				_hash = hash;
				_datatype = datatype;
			}

			public StackValue(string value, ulong hash, DataType datatype)
			{
				_type = Type.Literal;
				_value = value;
				_structSize = 0;
				_xhash = hash;
				_datatype = datatype;
			}

			public StackValue(string value, int structsize, uint hash, DataType datatype = DataType.Unk)
			{
				_type = Type.Struct;
				_value = value;
				_structSize = structsize;
				_hash = hash;
				_datatype = datatype;
			}

			public StackValue(string value, int structsize, ulong hash, DataType datatype = DataType.Unk)
			{
				_type = Type.Struct;
				_value = value;
				_structSize = structsize;
				_xhash = hash;
				_datatype = datatype;
			}

			public StackValue(string value, int structsize, bool Vector = false)
			{
				_type = Type.Struct;
				_value = value;
				_structSize = structsize;
				_datatype = (Vector && structsize == 3) ? DataType.Vector3 : DataType.Unk;
			}

			public StackValue(int stringsize, string value)
			{
				_type = Type.Struct;
				_value = value;
				_structSize = stringsize;
				_datatype = DataType.String;
			}

			public static StackValue Global(Type type, string name)
			{
				StackValue G = new StackValue(type, name);
				G.global = true;
				return G;
			}

			public string Value
			{
				get { return _value; }
			}

			public Type ItemType
			{
				get { return _type; }
			}

			public int StructSize
			{
				get { return _structSize; }
			}

			public DataType Datatype
			{
				get { return _datatype; }
			}

			public Variables.Var Variable
			{
				get { return _var; }
			}

			public Function Function
			{
				get { return _function; }
			}
			public bool isNative
			{
				get { return _hash != 0 || _xhash != 0; }
			}

			public uint NatHash
			{
				get { return _hash; }
			}

			public ulong X64NatHash
			{
				get { return _xhash; }
			}

			public bool isNotVar
			{
				get { return Variable == null && !global; }
			}

		}

		[Serializable]
		private class StackEmptyException : Exception
		{
			public StackEmptyException() : base() { }

			public StackEmptyException(string message) : base(message) { }

			public StackEmptyException(string message, Exception innerexception) : base(message, innerexception) { }
		}
		#endregion
	}

	public static class Types
	{
		public static DataTypes[] _types = new DataTypes[]
		{
			new DataTypes(Stack.DataType.Bool, 4, "bool", "b"),
			new DataTypes(Stack.DataType.Float, 3, "float", "f"),
			new DataTypes(Stack.DataType.Int, 3, "int", "i"),
			new DataTypes(Stack.DataType.String, 3, "char[]", "c"),
			new DataTypes(Stack.DataType.StringPtr, 3, "char*", "c"),
			new DataTypes(Stack.DataType.Unk, 0, "var", "u"),
			new DataTypes(Stack.DataType.Unsure, 1, "var", "u"),
			new DataTypes(Stack.DataType.IntPtr, 3, "int*", "i"),
			new DataTypes(Stack.DataType.UnkPtr, 0, "var*", "u"),
			new DataTypes(Stack.DataType.FloatPtr, 3, "float*", "f"),
			new DataTypes(Stack.DataType.Vector3, 2, "Vector3", "v"),
			new DataTypes(Stack.DataType.None, 4, "void", "f"),
		};

		public static DataTypes gettype(Stack.DataType type)
		{
			foreach (DataTypes d in _types)
			{
				if (d.type == type) return d;
			}
			throw new Exception("Unknown return type");
		}

		public static byte indexof(Stack.DataType type)
		{
			for (byte i = 0; i < _types.Length; i++)
			{
				if (_types[i].type == type) return i;
			}
			return 255;
		}

		public static Stack.DataType getatindex(byte index)
		{
			return _types[index].type;
		}

		public static Stack.DataType getpointerver(Stack.DataType type)
		{
			switch (type)
			{
				case Stack.DataType.Int: return Stack.DataType.IntPtr;
				case Stack.DataType.Unk: return Stack.DataType.UnkPtr;
				case Stack.DataType.Float: return Stack.DataType.FloatPtr;
				default: return type;
			}
		}

		public static bool hasptr(Stack.DataType type)
		{
			switch (type)
			{
				case Stack.DataType.Int:
				case Stack.DataType.Unk:
				case Stack.DataType.Unsure:
				case Stack.DataType.Float: return true;
				default: return false;
			}
		}

		public struct DataTypes
		{
			public Stack.DataType type;
			public int precedence;
			public string singlename;
			public string varletter;

			public DataTypes(Stack.DataType type, int precedence, string singlename, string varletter)
			{
				this.type = type;
				this.precedence = precedence;
				this.singlename = singlename;
				this.varletter = varletter;
			}

			public string returntype
			{
				get { return singlename + " "; }
			}

			public string vardec
			{
				get { return singlename + " " + varletter; }
			}

			public string vararraydec
			{
				get { return singlename + "[] " + varletter; }
			}

			public void check(DataTypes Second)
			{
				if (this.precedence < Second.precedence)
				{
					this = Second;
				}
			}

            public static implicit operator DataTypes(Stack.DataType v)
            {
                throw new NotImplementedException();
            }
        }
	}
}
