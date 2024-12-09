using System;
using System.Collections.Generic;
using System.Linq;

namespace Magic_RDR
{
	internal class HLInstruction
	{
		private int _Offset;
		private Instruction _Instruction;
		public readonly byte[] _Operands;

		public HLInstruction(byte instruction, IEnumerable<byte> operands, int offset)
		{
			_Instruction = (Instruction)instruction;
			_Operands = operands.ToArray();
			_Offset = offset;
		}

		public HLInstruction(byte instruction, int offset)
		{
			_Instruction = (Instruction)instruction;
			_Operands = new byte[0];
			_Offset = offset;
		}

		public Instruction Instruction
		{
			get { return _Instruction; }
		}

		public void NopInstruction()
		{
			_Instruction = Instruction.Nop;
		}

		public int Offset
		{
			get { return _Offset; }
		}

		public int InstructionLength
		{
			get { return 1 + _Operands.Count(); }
		}

		public int GetOperandsAsInt
		{
			get
			{
				switch (_Operands.Count())
				{
					case 1:
						return _Operands[0];
					case 2:
						return DataUtils.SwapEndian(BitConverter.ToInt16(_Operands, 0));
					case 3:
						return _Operands[0] << 16 | _Operands[1] << 8 | _Operands[2];
					case 4:
						return DataUtils.SwapEndian(BitConverter.ToInt32(_Operands, 0));
				}
				throw new Exception("Invalid amount of operands (" + _Operands.Count().ToString() + ")");
			}
		}

		public byte[] GetStringBytes
        {
			get
            {
				if (_Instruction == Instruction.PushString)
					return _Operands.ToArray();
				else
					throw new Exception("Not A PushString");
            }
        }

		public float GetFloat
		{
			get
			{
				if (_Operands.Count() != 4)
					throw new Exception("Not a Float");
				return
					DataUtils.SwapEndian(BitConverter.ToSingle(_Operands, 0));
			}
		}

		public int GetInt
		{
			get
			{
				if (_Operands.Count() != 4)
					throw new Exception("Not a Int32");
				return
					DataUtils.SwapEndian(BitConverter.ToInt32(_Operands, 0));
			}
		}

		public byte GetOperand(int index)
		{
			return _Operands[index];
		}

		public uint GetOperandsAsUInt
		{
			get
			{
				switch (_Operands.Count())
				{
					case 1:
						return _Operands[0];
					case 2:
						return DataUtils.SwapEndian(BitConverter.ToUInt16(_Operands, 0));
					case 3:
						return (uint) (_Operands[0] << 16 | _Operands[1] << 8 | _Operands[2]);
					case 4:
						return DataUtils.SwapEndian(BitConverter.ToUInt32(_Operands, 0));
				}
				throw new Exception("Invalid amount of operands (" + _Operands.Count().ToString() + ")");
			}
		}

		public int GetJumpOffset
		{
			get
			{
				if (!IsJumpInstruction)
					throw new Exception("Not A Jump");
				return DataUtils.SwapEndian(BitConverter.ToInt16(_Operands, 0)) + _Offset + 3;
			}
		}

		public byte GetNativeParams
		{
			get
			{
				if (_Instruction == Instruction.Native)
					return (byte)((_Operands[0] & 0x3e) >> 1);
				throw new Exception("Not A Native");
			}
		}

		public bool GetNativeReturns
		{
			get
			{
				if (_Instruction == Instruction.Native)
					return (_Operands[0] & 1) == 1 ? true : false;
				throw new Exception("Not A Native");
			}
		}

		public ushort GetNativeIndex
		{
			get
			{
				if (_Instruction == Instruction.Native)
					return BitConverter.ToUInt16(_Operands, 0);
				throw new Exception("Not A Native");
			}
		}

		public uint GetNativeIndex1
		{
			get
			{
				if (_Instruction != Instruction.Native)
					throw new Exception("Not A Native");

				uint aVal = _Operands[0];
				uint bVal = _Operands[1];
				return Convert.ToUInt32((aVal << 2) & 768) | bVal;
			}
		}

		public string GetSwitchStringCase(int index)
		{
			if (_Instruction == Instruction.Switch)
			{
				int cases = GetOperand(0);
				if (index >= cases)
				{
                    throw new Exception("Out Or Range Script Case");
                }
				return NativeParamInfo.IntToHex((int)DataUtils.SwapEndian(BitConverter.ToUInt32(_Operands, 1 + index * 6)));
			}
			throw new Exception("Not A Switch Statement");
		}

		public int GetSwitchOffset(int index)
		{
			if (_Instruction == Instruction.Switch)
			{
				int cases = GetOperand(0);
				if (index >= cases)
					throw new Exception("Out of range script case");
				return _Offset + 8 + index * 6 + DataUtils.SwapEndian(BitConverter.ToInt16(_Operands, 5 + index * 6));
			}
			throw new Exception("Not A Switch Statement");
		}

		public int GetImmBytePush
		{
			get
			{
				int _instruction = (int) Instruction;
				if (_instruction >= 138 && _instruction <= 146) //GTA V : 109 to 117
				{
					return _instruction - 139; //GTA V : 110
				}
				throw new Exception("Not An Immediate Int Push");
			}
		}

		public float GetImmFloatPush
		{
			get
			{
				int _instruction = (int) Instruction;
				if (_instruction >= 147 && _instruction <= 155) //GTA V : 118 to 126
				{
					return _instruction - 148; //GTA V : 119
				}
				throw new Exception("Not An Immediate Float Push");
			}
		}

		public bool IsReturnInstruction
        {
			get
			{
				return ((int)_Instruction > 121 && (int)_Instruction < 138) || (int)_Instruction == 46;
			}
		}

		public bool IsCallInstruction
		{
			get
			{
				return (int)_Instruction > 81 && (int)_Instruction < 98;
			}
		}

		public bool IsJumpInstruction
		{
			get
			{
				return (int)_Instruction > 97 && (int)_Instruction < 106; //GTA V : instruction > 84 && instruction < 93
			}
		}

		public bool IsConditionJump
		{
			get
			{
				return (int)_Instruction > 98 && (int)_Instruction < 106; //GTA V : instruction > 85 && instruction < 93
			}
		}

		public bool IsWhileJump
		{
			get
			{
				if (_Instruction == Instruction.Jump)
				{
					if (GetJumpOffset <= 0)
						return false;
					return GetOperandsAsInt < 0;
				}
				return false;
			}
		}

		public string GetGlobalString
		{
			get
			{
				switch (_Instruction)
				{
					case Instruction.pGlobal2:
					case Instruction.GlobalGet2:
					case Instruction.GlobalSet2:
					case Instruction.pGlobal3:
					case Instruction.GlobalGet3:
					case Instruction.GlobalSet3:
						return "Global_" + GetOperandsAsUInt.ToString();
				}
				throw new Exception("Not a global variable");
			}
		}

        public override string ToString()
        {
            return Instruction.ToString();
        }
    }

	internal enum Instruction
	{
		Nop = 0,
		iAdd, //1
		iSub, //2
		iMult, //3
		iDiv, //4
		iMod, //5
		iNot, //6
		iNeg, //7
		iCmpEq, //8
		iCmpNe, //9
		iCmpGt, //10
		iCmpGe, //11
		iCmpLt, //12
		iCmpLe, //13
		fAdd, //14
		fSub, //15
		fMult, //16
		fDiv, //17
		fMod, //18
		fNeg, //19
		fCmpEq, //20
		fCmpNe, //21
		fCmpGt, //22
		fCmpGe, //23
		fCmpLt, //24
		fCmpLe, //25
		vAdd, //26
		vSub, //27
		vMult, //28
		vDiv, //29
		vNeg, //30
		And, //31
		Or, //32
		Xor, //33
		ItoF, //34
		FtoI, //35
		FtoV, //36
		iPushByte1, //37
		iPushByte2, //38
		iPushByte3, //39
		iPushInt, //40
		fPush, //41
		dup, //42
		pop, //43
		Native, //44
		Enter, //45
		Return, //46
		pGet, //47
		pSet, //48
		pPeekSet, //49
		ToStack, //50
		FromStack, //51
		pArray1, //52
		ArrayGet1, //53
		ArraySet1, //54
		pFrame1, //55
		GetFrame1, //56
		SetFrame1, //57
		pStatic1, //58
		StaticGet1, //59
		StaticSet1, //60
		Add1, //61
		GetStruct1, //62
		SetStruct1, //63
		Mult1, //64
		iPushShort, //65
		Add2, //66
		GetStruct2, //67
		SetStruct2, //68
		Mult2, //69
		pArray2, //70
		ArrayGet2, //71
		ArraySet2, //72
		pFrame2, //73
		GetFrame2, //74
		SetFrame2, //75
		pStatic2, //76
		StaticGet2, //77
		StaticSet2, //78
		pGlobal2, //79
		GlobalGet2, //80
		GlobalSet2, //81
		Call2, //82
		Call2h1, //83
		Call2h2, //84
		Call2h3, //85
		Call2h4, //86
		Call2h5, //87
		Call2h6, //88
		Call2h7, //89
		Call2h8, //90
		Call2h9, //91
		Call2hA, //92
		Call2hB, //93
		Call2hC, //94
		Call2hD, //95
		Call2hE, //96
		Call2hF, //97
		Jump, //98
		JumpFalse, //99
		JumpNe, //100
		JumpEq, //101
		JumpLe, //102
		JumpLt, //103
		JumpGe, //104
		JumpGt, //105
		pGlobal3, //106
		GlobalGet3, //107
		GlobalSet3, //108
		iPushI24, //109
		Switch, //110
		PushString, //111
		PushArrayP, //112
		PushStringNull, //113
		StrCopy, //114
		ItoS, //115
		StrConCat, //116
		StrConCatInt, //117
		MemCopy, //118
		Catch, //119 //No handling of these as I'm unsure exactly how they work
		Throw, //120 //No script files in the game use these opcodes
		pCall, //121
		ReturnP0R0, //122
		ReturnP0R1, //123
		ReturnP0R2, //124
		ReturnP0R3, //125
		ReturnP1R0, //126
		ReturnP1R1, //127
		ReturnP1R2, //128
		ReturnP1R3, //129
		ReturnP2R0, //130
		ReturnP2R1, //131
		ReturnP2R2, //132
		ReturnP2R3, //133
		ReturnP3R0, //134
		ReturnP3R1, //135
		ReturnP3R2, //136
		ReturnP3R3, //137
		iPush_n1, //138
		iPush_0, //139
		iPush_1, //140
		iPush_2, //141
		iPush_3, //142
		iPush_4, //143
		iPush_5, //144
		iPush_6, //145
		iPush_7, //146
		fPush_n1, //147
		fPush_0, //148
		fPush_1, //149
		fPush_2, //150
		fPush_3, //151
		fPush_4, //152
		fPush_5, //153
		fPush_6, //154
		fPush_7, //155
        PatchRet, //156
        PatchTrap0, //157
        PatchTrap1, //158
        PatchTrap2, //159
        PatchTrap3, //160
        PatchTrap4, //161
        PatchTrap5, //162
        PatchTrap6, //163
        PatchTrap7, //164
        PatchTrap8, //165
        PatchTrap9, //166
        PatchTrapA, //167
        PatchTrapB, //168
        PatchTrapC, //169
        PatchTrapD, //170
        PatchTrapE, //171
        PatchTrapF, //172
        CallPatch, //173
        CallOutOfPatch, //174
        LoadRef, //175
        StoreRef, //176
        StoreVector, //177
        MakeVector //178
    }
}
