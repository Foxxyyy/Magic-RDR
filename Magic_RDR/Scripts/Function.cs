using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Magic_RDR
{
	public class Function
	{
		public string Name { get; private set; }
		public int Pcount { get; private set; }
		public int Vcount { get; private set; }
		public int Rcount { get; private set; }
		public int Location { get; private set; }
		public int MaxLocation { get; private set; }
		public Variables Vars { get; private set; }
		public Variables Params { get; private set; }
		public Types.DataTypes ReturnType { get; private set; }
		public List<byte> CodeBlock { get; set; } //The block of code that the function takes up

		private string tabs = "";
		public static bool PushStringNull, stackVal, writeElse, forLoop;
		public int Offset = 0, LineCount = 0;

		private static Stack Stack;
		private StringBuilder sb = null;
		private List<HLInstruction> Instructions;
		private Dictionary<int, int> InstructionMap;
		private CodePath Outerpath;
		private SwitchStatement OuterSwitch;
		private ScriptFile Scriptfile;
		private ReturnTypes RetType = ReturnTypes.Unkn;
		private FunctionName fnName;

		internal bool Decoded { get; private set; }
		internal bool DecodeStarted = false;
		internal bool preDecoded = false;
		internal bool preDecodeStarted = false;

		public Function(ScriptFile Owner, string name, int pcount, int vcount, int rcount, int location, int locmax)
		{
			Scriptfile = Owner;
			Name = name;
			Pcount = pcount;
			Vcount = vcount;
			Rcount = rcount;
			Location = location;
			MaxLocation = (locmax != -1) ? locmax : Location;

			fnName = new FunctionName(Name, Pcount, Rcount, Location, MaxLocation);
			Scriptfile.FunctionLoc.Add(location, fnName);
			
			Decoded = false;
			Vars = new Variables(Variables.ListType.Vars, vcount - 2);
			Params = new Variables(Variables.ListType.Params, pcount);
		}

		//Disposes of the function and returns the function text
		public override string ToString()
		{
			if (InstructionMap == null || sb == null)
			{
				return Name;
			}

			InstructionMap.Clear();
			Instructions.Clear();
			CodeBlock.Clear();
			Stack.Dispose();

			try
			{
				if (RetType == ReturnTypes.Bool || RetType == ReturnTypes.BoolUnk)
					return FirstLine() + "\r\n" + sb.ToString()
						.Replace("return 0;", "return false;")
						.Replace("return 1;", "return true;")
						.Replace("4294967295", "null");
				else
					return FirstLine() + "\r\n" + sb.ToString();
			}
			finally
			{
				sb.Clear();
				LineCount += 2;
			}
		}

		public string FirstLine() //Gets the first line of the function declaration
		{
			string name, working;

			if (Rcount == 0)
				working = "void ";
			else if (Rcount == 1)
			{
				if (ReturnType.type == Stack.DataType.String || ReturnType.type == Stack.DataType.StringPtr)
					working = "char* ";
				else if (ReturnType.type == Stack.DataType.Bool)
					working = "bool ";
				else if (ReturnType.type == Stack.DataType.Int)
					working = "int ";
				else if (ReturnType.type == Stack.DataType.Float)
					working = "float ";
				else
					working = ReturnType.type.ToString().ToLower() + " ";

				working = working.Replace("unk", "var");
				working = working.Replace("unsure", "var");
			}
			else if (Rcount == 3)
				working = "vector3 ";
			else if (Rcount >= 2)
			{
				if (ReturnType.type == Stack.DataType.String || ReturnType.type == Stack.DataType.StringPtr)
					working = "char* ";
				else if (ReturnType.type == Stack.DataType.Bool)
					working = "bool ";
				else if (ReturnType.type == Stack.DataType.Int)
					working = "int ";
				else if (ReturnType.type == Stack.DataType.Float)
					working = "float ";
				else
					working = "struct<" + (Rcount * 4).ToString() + "> ";
			}
			else
				throw new Exception("Unexpected return count");


			name = working + Name;
			if (name.Contains("var static"))
				name = name.Replace("var static", "static var");
			working = "(" + Params.GetPDec() + ")";

			return name + working + " //Position: 0x" + Location.ToString("X");
		}

		public string GetFrameVarName(uint index) //Determines if a frame variable is a parameter or a variable and returns its name
		{
			if (index < Pcount)
				return Params.GetVarName(index);
			else if (index < Pcount + 2)
				throw new Exception("Unexpected fVar");
			return Vars.GetVarName((uint) (index - 2 - Pcount));
		}

		public Variables.Var GetFrameVar(uint index) //Determines if a frame variable is a parameter or a variable and returns its index
		{
			if (index < Pcount)
				return Params.GetVarAtIndex(index);
			else if (index < Pcount + 2)
				throw new Exception("Unexpected fVar");
			return Vars.GetVarAtIndex((uint) (index - 2 - Pcount));
		}

		int GetCallOffset(int offset, int opcode) //Gets the function offset given the offset and opcode where its called from
		{
			return offset | (opcode - 0x52) << 0x10;
        }

        public FunctionName GetFunctionNameFromOffset(int offset, int opcode) //Gets the function info given the offset where its called from
		{
			int jPos = GetCallOffset(offset, opcode);
			if (Scriptfile.FunctionLoc.ContainsKey(jPos + 2))
				return Scriptfile.FunctionLoc[jPos + 2];
			else if (Scriptfile.FunctionLoc.ContainsKey(jPos))
				return Scriptfile.FunctionLoc[jPos];
			else
				return null;
		}

		public Function GetFunctionFromOffset(int offset) //Gets the function info given the offset where its called from
		{
			foreach (Function function in Scriptfile.Functions)
			{
				return function;
			}
			throw new Exception("Function wasn't found");
		}

		void OpenTab(bool write = true) //Indents everything below by 1 tab space
		{
			if (write)
				WriteLine("{");
			tabs += "\t";
		}

		void CloseTab(bool write = true) //Removes 1 tab space from indentation of everything below it
		{
			if (tabs.Length > 0)
				tabs = tabs.Remove(tabs.Length - 1);

			if (write)
				WriteLine("}");
		}

		//Step done before decoding, getting the variables types
		//Aswell as getting the list of instructions
		//Needs to PreDecode all functions before decoding any as this step
		//Builds the static variables types aswell
		public void PreDecode()
		{
			if (preDecoded || preDecodeStarted)
				return;

			preDecodeStarted = true;
			GetInstructions();
			DecodeInstructionsForVarInfo();
			preDecoded = true;
		}

		public void Decode() //The method that actually decodes the function into high level
		{
			if (Instructions == null)
				return;

			lock (ScriptViewerForm.ThreadLock)
			{
				DecodeStarted = true;
				if (Decoded)
					return;
			}

			Stack = new Stack(); //Set up a stack
			GetInstructions(); //Get the instructions in the function along with their operands
			Outerpath = new CodePath(CodePathType.Main, CodeBlock.Count, -1); //Set up the codepaths to a null item
			OuterSwitch = new SwitchStatement(null, -1);

			sb = new StringBuilder();
			OpenTab();
			Offset = 0;

			bool temp = false; //Write all the function variables declared by the function
			foreach (string s in Vars.GetDeclaration(false))
			{
				WriteLine(s);
				temp = true;
			}

			if (temp)
				WriteLine("");

			try
			{
				while (Offset < Instructions.Count)
				{
                    DecodeInstruction();
                }
			}
			catch
			{
				while (tabs.Length > 1)
				{
					tabs = tabs.Remove(tabs.Length - 1);
					WriteLine("}");
				}
			}

			while (OuterSwitch.Parent != null) //Fix for switches that end at the end of a function
			{
				CloseTab(false);
				CloseTab();
				OuterSwitch = OuterSwitch.Parent;
			}
			if (tabs.Length == 0)
			{
                CloseTab(false);
            }
			CloseTab();

			while (tabs.Length > 0)
			{
				tabs = tabs.Remove(tabs.Length - 1);
				WriteLine("}");
			}

			fnName.retType = ReturnType;
			Decoded = true;
		}

		void WriteLine(string line) //Writes a line to the function text as well as any tab chars needed before it
		{
			if (writeElse)
			{
				writeElse = false;
				WriteLine("else");
				OpenTab(true);
			}
			AppendLine(tabs + line);
		}

		public void AppendLine(string line)
		{
			sb.AppendLine(line);
			LineCount++;
		}

		void checkjumpcodepath() //Check if a jump is jumping out of the function. If not, then add it to the list of instructions
		{
			int cur = Offset;
			HLInstruction temp = new HLInstruction(CodeBlock[Offset], GetArray(2), cur);
			if (temp.GetJumpOffset > 0)
			{
				if (temp.GetJumpOffset < CodeBlock.Count)
				{
					AddInstruction(cur, temp);
					return;
				}
			}

			AddInstruction(cur, new HLInstruction(0, cur)); //If the jump is out the function then its useless so nop this jump
			AddInstruction(cur + 1, new HLInstruction(0, cur + 1));
			AddInstruction(cur + 2, new HLInstruction(0, cur + 2));
		}

		void checkdupforinstruction() //See if a dup is being used for an AND or OR
		{
			int off = 0;
			Start:
			off += 1;

			if (CodeBlock[Offset + off] == 0)
				goto Start;
			if (CodeBlock[Offset + off] == 99)
			{
				Offset = Offset + off + 2;
				return;
			}
			if (CodeBlock[Offset + off] == 6)
			{
				goto Start;
			}
			Instructions.Add(new HLInstruction(CodeBlock[Offset], Offset));
			return;
		}

		/// <summary>
		/// Gets the given amount of bytes from the codeblock at its offset
		/// while advancing its position by how ever many items it uses
		/// </summary>
		/// <param name="items">how many bytes to grab</param>
		/// <returns>the operands for the instruction</returns>
		IEnumerable<byte> GetArray(int items)
		{
			int temp = Offset + 1;
			Offset += items;
			return CodeBlock.GetRange(temp, items);
		}

		void handlejumpcheck() //When we hit a jump, decide how to handle it
		{
			//Check the jump location against each switch statement, to see if it is recognised as a break
			SwitchStatement tempsw = OuterSwitch;
			startsw:
			if (Instructions[Offset].GetJumpOffset == tempsw.BreakOffset)
			{
				WriteLine("break;");
				return;
			}
			else
			{
				if (tempsw.Parent != null)
				{
					tempsw = tempsw.Parent;
					goto startsw;
				}
			}
			int tempoff = 0;
			if (Instructions[Offset + 1].Offset == Outerpath.EndOffset)
			{
				if (Instructions[Offset].GetJumpOffset != Instructions[Offset + 1].Offset)
				{
					if (!Instructions[Offset].IsWhileJump)
					{
						//The jump is detected as being after a for loop
						//finish the current code path and add a new one
						/*if (Outerpath.Type == CodePathType.For)
						{
							CodePath temp = Outerpath;
							Outerpath = Outerpath.Parent;
							Outerpath.ChildPaths.Remove(temp);
							Outerpath = Outerpath.CreateCodePath(CodePathType.If, Instructions[Offset].GetJumpOffset, -1);
							closetab();
						}
						else
						{*/
							//The jump is detected as being an else statement
							//finish the current if code path and add an else code path
							CodePath temp = Outerpath;
							Outerpath = Outerpath.Parent;
							Outerpath.ChildPaths.Remove(temp);
							Outerpath = Outerpath.CreateCodePath(CodePathType.Else, Instructions[Offset].GetJumpOffset, -1);
							CloseTab();
							writeElse = true;
						//}
						return;
					}
					throw new Exception("Shouldnt find a while loop here");
				}
				return;
			}
			start:
			//Check to see if the jump is just jumping past nops(end of code table)
			//should be the only case for finding another jump now
			if (Instructions[Offset].GetJumpOffset != Instructions[Offset + 1 + tempoff].Offset)
			{
				if (Instructions[Offset + 1 + tempoff].Instruction == Instruction.Nop)
				{
					tempoff++;
					goto start;
				}
				else if (Instructions[Offset + 1 + tempoff].Instruction == Instruction.Jump)
				{
					if (Instructions[Offset + 1 + tempoff].GetOperandsAsInt == 0)
					{
						tempoff++;
						goto start;
					}

				}

				if (Instructions[Offset].GetOperandsAsInt != 0)
				{
					WriteLine("Jump @" + Instructions[Offset].GetJumpOffset.ToString() + $"; //curOff = {Instructions[Offset].Offset}");
				}
			}

		}

		//Needs Merging with method below
		bool IsNewCodePath()
		{
			if (Outerpath.Parent != null)
			{
				if (InstructionMap[Outerpath.EndOffset] == Offset)
				{
					return true;
				}
			}
			if (OuterSwitch.Offsets.Count > 0)
			{
				if (Instructions[Offset].Offset == OuterSwitch.Offsets[0])
				{
					return true;
				}
			}
			return false;
		}

		//Checks if the current offset is a new code path, then decides how to handle it
		void HandleNewPath()
		{
			start:
			if (Instructions[Offset].Offset == Outerpath.EndOffset)
			{
				//Offset recognised as the exit instruction of the outermost code path
				//remove outermost code path
				CodePath temp = Outerpath;
				Outerpath = Outerpath.Parent;
				Outerpath.ChildPaths.Remove(temp);
				CloseTab();
				//check next codepath to see if it belongs there aswell
				goto start;
			}
			if (OuterSwitch.Offsets.Count > 0)
			{
				if (Instructions[Offset].Offset == OuterSwitch.Offsets[0])
				{
					CloseTab(false);

					if (OuterSwitch.Offsets.Count == 1)
					{
						//end of switch statement detected
						//remove child class 
						SwitchStatement temp = OuterSwitch;
						OuterSwitch = OuterSwitch.Parent;
						OuterSwitch.ChildSwitches.Remove(temp);
						CloseTab();
						//go check if its the next switch exit instruction
						//probably isnt and the goto can probably be removed
						goto start;
					}
					else
					{
						//more cases left in switch
						//so write the next switch case
						WriteLine("");
						for (int i = 0; i < OuterSwitch.Cases[OuterSwitch.Offsets[0]].Count; i++)
						{
							string temp = OuterSwitch.Cases[OuterSwitch.Offsets[0]][i];
							if (temp == "default")
								WriteLine("default:");
							else
								WriteLine("case " + temp + ":");
						}
						OpenTab(false);

						//remove last switch case from class, so it wont attemp to jump there again
						OuterSwitch.Offsets.RemoveAt(0);

						//as before, probably not needed, so should always skip past here
						goto start;
					}
				}
			}
		}

		//Create a switch statement, then set up the rest of the decompiler to handle the rest of the switch statement
		void HandleSwitch()
		{
			Dictionary<int, List<string>> cases = new Dictionary<int, List<string>>();
			string case_val;
			int offset;
			int defaultloc;
			int breakloc;
			bool usedefault;
			HLInstruction temp;

			for (int i = 0; i < Instructions[Offset].GetOperand(0); i++)
			{
				//Check if the case is a known hash
				case_val = Instructions[Offset].GetSwitchStringCase(i);

				//Get the offset to jump to
				offset = Instructions[Offset].GetSwitchOffset(i);


				if (!cases.ContainsKey(offset))
				{
					//unknown offset
					cases.Add(offset, new List<string>(new string[] {case_val}));
				}
				else
				{
					//This offset is known, multiple cases are jumping to this path
					cases[offset].Add(case_val);
				}
			}

			//Not sure how necessary this step is, but just incase R* compiler doesnt order jump offsets, do it anyway
			List<int> sorted = cases.Keys.ToList();
			sorted.Sort();

			//Handle(skip past) any Nops immediately after switch statement
			int tempoff = 0;
			while (Instructions[Offset + 1 + tempoff].Instruction == Instruction.Nop)
			{
				tempoff++;
			}

			//Extract the location to jump to if no cases match
			defaultloc = Instructions[Offset + 1 + tempoff].GetJumpOffset;

			//We have found the jump location, so that instruction is no longer needed and can be nopped
			Instructions[Offset + 1 + tempoff].NopInstruction();

			//Temporary stage
			breakloc = defaultloc;
			usedefault = true;

			//Check if case last instruction is a jump to default location, if so default location is a break
			//If not break location is where last instruction jumps to
			for (int i = 0; i <= sorted.Count; i++)
			{
				int index = 0;
				if (i == sorted.Count)
					index = InstructionMap[defaultloc] - 1;
				else
					index = InstructionMap[sorted[i]] - 1;
				if (index - 1 == Offset)
				{
					continue;
				}
				temp = Instructions[index];
				if (temp.Instruction != Instruction.Jump)
				{
					continue;
				}
				if (temp.GetJumpOffset == defaultloc)
				{
					usedefault = false;
					breakloc = defaultloc;
					break;
				}
				breakloc = temp.GetJumpOffset;
			}

			if (usedefault)
			{
				//Default location found, best add it in
				if (cases.ContainsKey(defaultloc))
				{
					//Default location shares code path with other known case
					cases[defaultloc].Add("default");
				}
				else
				{
					//Default location is a new code path
					cases.Add(defaultloc, new List<string>(new string[] { "default" }));
					sorted = cases.Keys.ToList();
					sorted.Sort();
				}
			}

			//Found all information about switch, write the first case, the rest will be handled when we get to them
			WriteLine("switch (" + Stack.PopLit() + ")");
			OpenTab();
			for (int i = 0; i < cases[sorted[0]].Count; i++)
			{
				WriteLine("case " + cases[sorted[0]][i] + ":");
			}
			OpenTab(false);
			cases.Remove(sorted[0]);

			//Create the class the rest of the decompiler needs to handle the rest of the switch
			OuterSwitch = OuterSwitch.CreateSwitchStatement(cases, breakloc);
		}

		//If we have a conditional statement determine whether its for an if/while statement then handle it accordingly
		void CheckConditional()
		{
			forLoop = false;
			string tempstring = Stack.PopLit();

			if (!(tempstring.StartsWith("(") && tempstring.EndsWith(")")))
				tempstring = "(" + tempstring + ")";
			if (tempstring == "(!1)")
				return;

			int offset = Instructions[Offset].GetJumpOffset;
			CodePath tempcp = Outerpath;

			/*if ((Instructions[InstructionMap[offset] - 2].Instruction == Instruction.SetFrame1) && (Instructions[InstructionMap[offset] - 3].Instruction == Instruction.Add1)) forLoop = true;
			else if ((Instructions[InstructionMap[offset] - 3].Instruction == Instruction.SetFrame1) && (Instructions[InstructionMap[offset] - 4].Instruction == Instruction.Add1)) forLoop = true;
			else if ((Instructions[InstructionMap[offset] - 4].Instruction == Instruction.SetFrame1) && (Instructions[InstructionMap[offset] - 5].Instruction == Instruction.Add1)) forLoop = true;
			else if ((Instructions[InstructionMap[offset] - 5].Instruction == Instruction.SetFrame1) && (Instructions[InstructionMap[offset] - 6].Instruction == Instruction.Add1)) forLoop = true;
			else if ((Instructions[InstructionMap[offset] - 6].Instruction == Instruction.SetFrame1) && (Instructions[InstructionMap[offset] - 7].Instruction == Instruction.Add1)) forLoop = true;*/

			start:
			if (tempcp.Type == CodePathType.While)
			{
				if (offset == tempcp.EndOffset)
				{
					WriteLine("if " + tempstring);
					OpenTab(false);
					WriteLine("break;");
					CloseTab(false);
					return;
				}
			}

			if (tempcp.Parent != null)
			{
				tempcp = tempcp.Parent;
				goto start;
			}
			HLInstruction jumploc = Instructions[InstructionMap[offset] - 1];

			/*if (forLoop) //iVar8 >= 16
			{
				int index;
				if (tempstring.Contains("<"))
					index = tempstring.IndexOf("<");
				else if (tempstring.Contains(">"))
					index = tempstring.IndexOf(">");
				else goto jump;

				//Remove brackets
				tempstring = tempstring.Remove(0, 1);
				tempstring = tempstring.Remove(tempstring.Length - 1, 1);

				string init = tempstring.Remove(index - 2, tempstring.Length - index + 2);
				tempstring = string.Format("for ({0}; {1}; {2}++)", init, tempstring, init);
				writeline(tempstring);
				Outerpath = Outerpath.CreateCodePath(CodePathType.For, Instructions[Offset].GetJumpOffset, -1);
				opentab();
			}

			jump:*/

			if (tempstring == "(1)")
			{
				tempstring = "(true)";
				WriteLine("while " + tempstring);
				Outerpath = Outerpath.CreateCodePath(CodePathType.While, Instructions[Offset].GetJumpOffset, -1);
				OpenTab();
			}
			else if (jumploc.IsWhileJump && jumploc.GetJumpOffset < Instructions[Offset].Offset && !forLoop)
			{
				jumploc.NopInstruction();
				if (tempstring == "(1)")
					tempstring = "(true)";
				WriteLine("while " + tempstring);
				Outerpath = Outerpath.CreateCodePath(CodePathType.While, Instructions[Offset].GetJumpOffset, -1);
				OpenTab();
			}
			else if (!forLoop)
			{
				bool written = false;
				if (writeElse)
				{
					if (Outerpath.EndOffset == Instructions[Offset].GetJumpOffset)
					{
						writeElse = false;
						CodePath temp = Outerpath;
						Outerpath = Outerpath.Parent;
						Outerpath.ChildPaths.Remove(temp);
						Outerpath = Outerpath.CreateCodePath(CodePathType.If, Instructions[Offset].GetJumpOffset, -1);
						WriteLine("else if " + tempstring);
						OpenTab();
						written = true;
					}
					else if (Instructions[InstructionMap[Instructions[Offset].GetJumpOffset] - 1].Instruction == Instruction.Jump)
					{
						if (Outerpath.EndOffset == Instructions[InstructionMap[Instructions[Offset].GetJumpOffset] - 1].GetJumpOffset)
						{
							writeElse = false;
							CodePath temp = Outerpath;
							Outerpath = Outerpath.Parent;
							Outerpath.ChildPaths.Remove(temp);
							Outerpath = Outerpath.CreateCodePath(CodePathType.If, Instructions[Offset].GetJumpOffset, -1);
							WriteLine("else if " + tempstring);
							OpenTab();
							written = true;
						}
					}
				}
				if (!written)
				{
					WriteLine("if " + tempstring);
					Outerpath = Outerpath.CreateCodePath(CodePathType.If, Instructions[Offset].GetJumpOffset, -1);
					OpenTab();
				}
			}
		}

		//Turns the raw code into a list of instructions
		public void GetInstructions()
		{
			Offset = CodeBlock[4] + 5;
			Instructions = new List<HLInstruction>();
			InstructionMap = new Dictionary<int, int>();

			int curoff;
			try
			{
				while (Offset < CodeBlock.Count)
				{
					curoff = Offset;
					switch (CodeBlock[Offset])
					{
						case 37: AddInstruction(curoff, new HLInstruction(CodeBlock[Offset], GetArray(1), curoff)); break; //PushB1
						case 38: AddInstruction(curoff, new HLInstruction(CodeBlock[Offset], GetArray(2), curoff)); break; //PushB2
						case 39: AddInstruction(curoff, new HLInstruction(CodeBlock[Offset], GetArray(3), curoff)); break; //PushB3
						case 40:
						case 41: AddInstruction(curoff, new HLInstruction(CodeBlock[Offset], GetArray(4), curoff)); break; //fPush
						case 42: checkdupforinstruction(); break; //Dup
						case 44: AddInstruction(curoff, new HLInstruction(CodeBlock[Offset], GetArray(2), curoff)); break; //Native
						case 45: break; //Function
						case 46: AddInstruction(curoff, new HLInstruction(CodeBlock[Offset], GetArray(2), curoff)); break; //Return
						case 52:
						case 53:
						case 54:
						case 55:
						case 56:
						case 57:
						case 58:
						case 59:
						case 60:
						case 61:
						case 62:
						case 63:
						case 64: AddInstruction(curoff, new HLInstruction(CodeBlock[Offset], GetArray(1), curoff)); break; //Mult1
						case 65:
						case 66:
						case 67:
						case 68:
						case 69:
						case 70:
						case 71:
						case 72:
						case 73:
						case 74:
						case 75:
						case 76:
						case 77:
						case 78:
						case 79:
						case 80:
						case 81:
						case 82:
						case 83:
						case 84:
						case 85:
						case 86:
						case 87:
						case 88:
						case 89:
						case 90:
						case 91:
						case 92:
						case 93:
						case 94:
						case 95:
						case 96:
						case 97: AddInstruction(curoff, new HLInstruction(CodeBlock[Offset], GetArray(2), curoff)); break; //Call2hF
						case 98: checkjumpcodepath(); break; //Jump
						case 99:
						case 100:
						case 101:
						case 102:
						case 103:
						case 104:
						case 105: AddInstruction(curoff, new HLInstruction(CodeBlock[Offset], GetArray(2), curoff)); break; //JumpGT
						case 106:
						case 107:
						case 108:
						case 109: AddInstruction(curoff, new HLInstruction(CodeBlock[Offset], GetArray(3), curoff)); break; //PushI24
						case 110: //Switch
							AddInstruction(curoff, new HLInstruction(CodeBlock[Offset], GetArray(1 + CodeBlock[Offset + 1] * 6), curoff));
							break;
						case 111: //PushString
							AddInstruction(curoff, new HLInstruction(CodeBlock[Offset], GetArray(1 + CodeBlock[Offset + 1]), curoff));
							break;
						case 112: //PushArrayP
							AddInstruction(curoff, new HLInstruction(CodeBlock[Offset], GetArray(5 + CodeBlock[Offset + 1]), curoff));
							break;
						case 114:
						case 115:
						case 116:
						case 117: AddInstruction(curoff, new HLInstruction(CodeBlock[Offset], GetArray(1), curoff)); break;
						default:
							if (CodeBlock[Offset] <= 155)
								AddInstruction(curoff, new HLInstruction(CodeBlock[Offset], curoff));
							break;
					}
					Offset++;
				}
			}
			catch (Exception ex)
			{

			}
		}

		//Adds an instruction to the list of instructions then adds the offset to a dictionary
		void AddInstruction(int offset, HLInstruction instruction)
		{
			Instructions.Add(instruction);
			InstructionMap.Add(offset, Instructions.Count - 1);
		}

		//Decodes the instruction at the current offset
		public void DecodeInstruction()
		{
			object temp;
			string tempstring = "";
			int returnVal = 0;
			int CallOpcode = 0;

			if (IsNewCodePath())
			{
                HandleNewPath();
            }

			switch (Instructions[Offset].Instruction)
			{
				case Instruction.Nop:
					break;
				case Instruction.iAdd:
					Stack.Op_Add();
					break;
				case Instruction.fAdd:
					Stack.Op_Addf();
					break;
				case Instruction.iSub:
					Stack.Op_Sub();
					break;
				case Instruction.fSub:
					Stack.Op_Subf();
					break;
				case Instruction.iMult:
					Stack.Op_Mult();
					break;
				case Instruction.fMult:
					Stack.Op_Multf();
					break;
				case Instruction.iDiv:
					Stack.Op_Div();
					break;
				case Instruction.fDiv:
					Stack.Op_Divf();
					break;
				case Instruction.iMod:
					Stack.Op_Mod();
					break;
				case Instruction.fMod:
					Stack.Op_Modf();
					break;
				case Instruction.iNot:
					Stack.Op_Not();
					break;
				case Instruction.iNeg:
					Stack.Op_Neg();
					break;
				case Instruction.fNeg:
					Stack.Op_Negf();
					break;
				case Instruction.iCmpEq:
				case Instruction.fCmpEq:
					Stack.Op_CmpEQ();
					break;
				case Instruction.iCmpNe:
				case Instruction.fCmpNe:
					Stack.Op_CmpNE();
					break;
				case Instruction.iCmpGt:
				case Instruction.fCmpGt:
					Stack.Op_CmpGT();
					break;
				case Instruction.iCmpGe:
				case Instruction.fCmpGe:
					Stack.Op_CmpGE();
					break;
				case Instruction.iCmpLt:
				case Instruction.fCmpLt:
					Stack.Op_CmpLT();
					break;
				case Instruction.iCmpLe:
				case Instruction.fCmpLe:
					Stack.Op_CmpLE();
					break;
				case Instruction.vAdd:
					Stack.Op_Vadd();
					break;
				case Instruction.vSub:
					Stack.Op_VSub();
					break;
				case Instruction.vMult:
					Stack.Op_VMult();
					break;
				case Instruction.vDiv:
					Stack.Op_VDiv();
					break;
				case Instruction.vNeg:
					Stack.Op_VNeg();
					break;
				case Instruction.And:
					Stack.Op_And();
					break;
				case Instruction.Or:
					Stack.Op_Or();
					break;
				case Instruction.Xor:
					Stack.Op_Xor();
					break;
				case Instruction.ItoF:
					Stack.Op_Itof();
					break;
				case Instruction.FtoI:
					Stack.Op_FtoI();
					break;
				case Instruction.FtoV:
					Stack.Op_FtoV();
					break;
				case Instruction.iPushByte1:
					Stack.Push(Instructions[Offset].GetOperand(0));
					break;
				case Instruction.iPushByte2:
					Stack.Push(Instructions[Offset].GetOperand(0), Instructions[Offset].GetOperand(1));
					break;
				case Instruction.iPushByte3:
					Stack.Push(Instructions[Offset].GetOperand(0), Instructions[Offset].GetOperand(1),
						Instructions[Offset].GetOperand(2));
					break;
				case Instruction.iPushInt:
					Stack.Push(Instructions[Offset].GetInt);
					break;
				case Instruction.iPushI24:
					tempstring = "";
					Stack.Push(Instructions[Offset].GetOperandsAsInt.ToString(), Stack.DataType.Int);
					break;
				case Instruction.iPushShort:
					Stack.Push(Instructions[Offset].GetOperandsAsInt);
					break;
				case Instruction.fPush:
					Stack.Push(Instructions[Offset].GetFloat);
					break;
				case Instruction.dup:
					Stack.Dup();
					break;
				case Instruction.pop:
					temp = Stack.Drop();
					if (temp is string)
						WriteLine(temp as string);
					break;
				case Instruction.Native:
					uint natHash = Scriptfile.NativeTable.GetNativeHashFromIndex((int)Instructions[Offset].GetNativeIndex1);
					string natStr = Scriptfile.NativeTable.GetNativeFromIndex((int)Instructions[Offset].GetNativeIndex1);
					tempstring = Stack.NativeCallTest(natHash, natStr, Instructions[Offset].GetNativeParams, Instructions[Offset].GetNativeReturns ? 1 : 0);	
					if (tempstring != "")
						WriteLine(tempstring);
					break;

				case Instruction.Enter: break;

				case Instruction.ReturnP0R0:
				case Instruction.ReturnP1R0:
				case Instruction.ReturnP2R0:
				case Instruction.ReturnP3R0: WriteLine("return;"); break;

				case Instruction.ReturnP0R1:
				case Instruction.ReturnP1R1:
				case Instruction.ReturnP2R1:
				case Instruction.ReturnP3R1: returnVal = 1; goto case Instruction.Return;

				case Instruction.ReturnP0R2:
				case Instruction.ReturnP1R2:
				case Instruction.ReturnP2R2:
				case Instruction.ReturnP3R2: returnVal = 2; goto case Instruction.Return;

				case Instruction.ReturnP0R3:
				case Instruction.ReturnP1R3:
				case Instruction.ReturnP2R3:
				case Instruction.ReturnP3R3: returnVal = 3; goto case Instruction.Return;

				case Instruction.Return:
					if (returnVal == 1)
					{
						returnVal = 0;
						tempstring = Stack.PopListForCall(1);
						ReturnCheck(tempstring);
						WriteLine("return " + tempstring + ";");
					}
					else if (returnVal == 2)
					{
						returnVal = 0;
						tempstring = Stack.PopListForCall(2);
						ReturnCheck(tempstring);
						WriteLine("return " + tempstring + ";");
					}
					else if (returnVal == 3)
					{
						returnVal = 0;
						tempstring = Stack.PopListForCall(3);
						ReturnCheck(tempstring);
						WriteLine("return " + tempstring + ";");
					}
					else
					{
						tempstring = Stack.PopListForCall(Instructions[Offset].GetOperand(1));
						switch (Instructions[Offset].GetOperand(1))
						{
							case 0:
								if (Offset < Instructions.Count - 1) WriteLine("return;");
								break;
							case 1:
								ReturnCheck(tempstring);
								WriteLine("return " + tempstring + ";");
								break;
							default:
								if (Stack.TopType == Stack.DataType.String)
								{
									RetType = ReturnTypes.String;
									ReturnType = Stack.DataType.String;
								}
								if (tempstring.StartsWith("s")) ReturnType = Types.gettype(Stack.DataType.String);
								else if (tempstring.StartsWith("c")) ReturnType = Types.gettype(Stack.DataType.StringPtr);
								else if (tempstring.StartsWith("b")) ReturnType = Types.gettype(Stack.DataType.Bool);
								WriteLine("return " + tempstring + ";");
								break;
						}
					}
					break;
				case Instruction.pGet: Stack.Op_RefGet(); break;
				case Instruction.pSet:
					if (Stack.PeekVar(1) == null)
						WriteLine(Stack.Op_RefSet());			
					else if (Stack.PeekVar(1).IsArray)
						Stack.Op_RefSet();
					else WriteLine(Stack.Op_RefSet());
						break;
				case Instruction.pPeekSet:
					if (Stack.PeekVar(1) == null) WriteLine(Stack.Op_PeekSet());
					else if (Stack.PeekVar(1).IsArray) Stack.Op_PeekSet();
					else WriteLine(Stack.Op_PeekSet()); break;

				case Instruction.ToStack: Stack.Op_ToStack(); break;
				case Instruction.FromStack: stackVal = true; WriteLine(Stack.Op_FromStack()); stackVal = false; break;

				case Instruction.pArray1:
				case Instruction.pArray2: Stack.Op_ArrayGetP(Instructions[Offset].GetOperandsAsUInt); break;

				case Instruction.ArrayGet1:
				case Instruction.ArrayGet2: Stack.Op_ArrayGet(Instructions[Offset].GetOperandsAsUInt); break;

				case Instruction.ArraySet1:
				case Instruction.ArraySet2: WriteLine(Stack.Op_ArraySet(Instructions[Offset].GetOperandsAsUInt)); break;

				case Instruction.pFrame1:
				case Instruction.pFrame2:
					Stack.PushPVar(GetFrameVarName(Instructions[Offset].GetOperandsAsUInt),
						GetFrameVar(Instructions[Offset].GetOperandsAsUInt)); break;

				case Instruction.GetFrame1:
				case Instruction.GetFrame2:
					Stack.PushVar(GetFrameVarName(Instructions[Offset].GetOperandsAsUInt),
					GetFrameVar(Instructions[Offset].GetOperandsAsUInt)); break;

				case Instruction.SetFrame1:
				case Instruction.SetFrame2:
					tempstring = Stack.Op_Set(GetFrameVarName(Instructions[Offset].GetOperandsAsUInt),
						GetFrameVar(Instructions[Offset].GetOperandsAsUInt));
					if (GetFrameVar(Instructions[Offset].GetOperandsAsUInt).DataType == Stack.DataType.Bool)
						tempstring = tempstring.Replace("= 0;", "= false;").Replace("= 1;", "= true;");
					if (!GetFrameVar(Instructions[Offset].GetOperandsAsUInt).IsArray)
						WriteLine(tempstring); break;

				case Instruction.pStatic1:
				case Instruction.pStatic2:
					Stack.PushPVar(Scriptfile.Statics.GetVarName(Instructions[Offset].GetOperandsAsUInt),
					Scriptfile.Statics.GetVarAtIndex(Instructions[Offset].GetOperandsAsUInt));
					break;
					
				case Instruction.StaticGet1:
				case Instruction.StaticGet2:
					Stack.PushVar(Scriptfile.Statics.GetVarName(Instructions[Offset].GetOperandsAsUInt),
					Scriptfile.Statics.GetVarAtIndex(Instructions[Offset].GetOperandsAsUInt));					
					break;

				case Instruction.StaticSet1:
				case Instruction.StaticSet2:
					tempstring = Stack.Op_Set(Scriptfile.Statics.GetVarName(Instructions[Offset].GetOperandsAsUInt),
						Scriptfile.Statics.GetVarAtIndex(Instructions[Offset].GetOperandsAsUInt));
					if (Scriptfile.Statics.GetVarAtIndex(Instructions[Offset].GetOperandsAsUInt).DataType == Stack.DataType.Bool)
						tempstring = tempstring.Replace("= 0;", "= false;").Replace("= 1;", "= true;");
					if (!Scriptfile.Statics.GetVarAtIndex(Instructions[Offset].GetOperandsAsUInt).IsArray)
						WriteLine(tempstring); break;

				case Instruction.Add1:
				case Instruction.Add2: Stack.Op_AmmImm(Instructions[Offset].GetOperandsAsInt); break;
				case Instruction.Mult1:
				case Instruction.Mult2: Stack.Op_MultImm(Instructions[Offset].GetOperandsAsInt); break;
				case Instruction.GetStruct1:
				case Instruction.GetStruct2: Stack.Op_GetImm(Instructions[Offset].GetOperandsAsUInt); break;
				case Instruction.SetStruct1:
				case Instruction.SetStruct2: WriteLine(Stack.Op_SetImm(Instructions[Offset].GetOperandsAsUInt)); break;
				case Instruction.pGlobal2:
				case Instruction.pGlobal3: Stack.PushPGlobal(Instructions[Offset].GetGlobalString); break;
				case Instruction.GlobalGet2:
				case Instruction.GlobalGet3: Stack.PushGlobal(Instructions[Offset].GetGlobalString); break;
				case Instruction.GlobalSet2:
				case Instruction.GlobalSet3: WriteLine(Stack.Op_Set(Instructions[Offset].GetGlobalString)); break;

				case Instruction.Jump: handlejumpcheck(); break;
				case Instruction.JumpFalse: goto HandleJump;
				case Instruction.JumpNe: Stack.Op_CmpNE(); goto HandleJump;
				case Instruction.JumpEq: Stack.Op_CmpEQ(); goto HandleJump;
				case Instruction.JumpLe: Stack.Op_CmpLE(); goto HandleJump;
				case Instruction.JumpLt: Stack.Op_CmpLT(); goto HandleJump;
				case Instruction.JumpGe: Stack.Op_CmpGE(); goto HandleJump;
				case Instruction.JumpGt: Stack.Op_CmpGT(); goto HandleJump;

				case Instruction.Call2: CallOpcode = 82; goto HandleCall;
				case Instruction.Call2h1: CallOpcode = 83; goto HandleCall;
				case Instruction.Call2h2: CallOpcode = 84; goto HandleCall;
				case Instruction.Call2h3: CallOpcode = 85; goto HandleCall;
				case Instruction.Call2h4: CallOpcode = 86; goto HandleCall;
				case Instruction.Call2h5: CallOpcode = 87; goto HandleCall;
				case Instruction.Call2h6: CallOpcode = 88; goto HandleCall;
				case Instruction.Call2h7: CallOpcode = 89; goto HandleCall;
				case Instruction.Call2h8: CallOpcode = 90; goto HandleCall;
				case Instruction.Call2h9: CallOpcode = 91; goto HandleCall;
				case Instruction.Call2hA: CallOpcode = 92; goto HandleCall;
				case Instruction.Call2hB: CallOpcode = 93; goto HandleCall;
				case Instruction.Call2hC: CallOpcode = 94; goto HandleCall;
				case Instruction.Call2hD: CallOpcode = 95; goto HandleCall;
				case Instruction.Call2hE: CallOpcode = 96; goto HandleCall;
				case Instruction.Call2hF: CallOpcode = 97; goto HandleCall;

				case Instruction.Switch: HandleSwitch(); break;

				case Instruction.PushString:
                    byte[] tBytes = Instructions[Offset].GetStringBytes;
                    tempstring = string.Format("\"{0}\"", Encoding.GetEncoding(1252).GetString(tBytes));
                    tempstring = tempstring.Replace("\\", "\\\\");
                    tempstring = tempstring.Replace("\n", "\\n");
                    tempstring = tempstring.Remove(1, 1);
                    tempstring = tempstring.Remove(tempstring.Length - 2, 1);
                    if (!DataUtils.IntParse(tempstring, out var tempint))
                        Stack.Push(tempstring, Stack.DataType.StringPtr);
					break;
                case Instruction.PushStringNull:
					tempstring = "\"\"";
					PushStringNull = true;
					Stack.Push(tempstring, Stack.DataType.StringPtr);
					break;

				case Instruction.StrCopy: WriteLine(Stack.op_strcopy(Instructions[Offset].GetOperandsAsInt)); break;
				case Instruction.ItoS: WriteLine(Stack.op_itos(Instructions[Offset].GetOperandsAsInt)); break;
				case Instruction.StrConCat: WriteLine(Stack.op_stradd(Instructions[Offset].GetOperandsAsInt)); break;
				case Instruction.StrConCatInt: WriteLine(Stack.op_straddi(Instructions[Offset].GetOperandsAsInt)); break;
				case Instruction.MemCopy: WriteLine(Stack.op_sncopy()); break;
				case Instruction.Catch: break;
				case Instruction.Throw: break;
				case Instruction.pCall: foreach (string s in Stack.pcall()) WriteLine(s); break;

				case Instruction.iPush_n1:
				case Instruction.iPush_0:
				case Instruction.iPush_1:
				case Instruction.iPush_2:
				case Instruction.iPush_3:
				case Instruction.iPush_4:
				case Instruction.iPush_5:
				case Instruction.iPush_6:
				case Instruction.iPush_7: Stack.Push(Instructions[Offset].GetImmBytePush); break;
				case Instruction.fPush_n1:
				case Instruction.fPush_0:
				case Instruction.fPush_1:
				case Instruction.fPush_2:
				case Instruction.fPush_3:
				case Instruction.fPush_4:
				case Instruction.fPush_5:
				case Instruction.fPush_6:
				case Instruction.fPush_7: Stack.Push(Instructions[Offset].GetImmFloatPush); break;

				HandleJump:
				CheckConditional();
				break;

				HandleCall:
				FunctionName tempf = GetFunctionNameFromOffset(Instructions[Offset].GetOperandsAsInt, CallOpcode);
				if (tempf != null)
					tempstring = Stack.FunctionCall(tempf.Name, tempf.Pcount, tempf.Rcount);
				else
					WriteLine("Unknown_Function();");
				if (tempstring != "" && tempf.Rcount != 1 && tempf.Rcount < 4)
						WriteLine(tempstring);
				break;
			}
			Offset++;
		}

		//Bunch of methods that extracts what data type a static/frame variable is
		#region GetDataType

		public void CheckInstruction(int index, Stack.DataType type, int count = 1)
		{
			if (type == Stack.DataType.Unk)
				return;

			for (int i = 0; i < count; i++)
			{
				Variables.Var Var = Stack.PeekVar(index + i);
				if (Var != null && (Stack.isLiteral(index + i) || Stack.isPointer(index + i)))
				{
					if (Types.gettype(type).precedence < Types.gettype(Var.DataType).precedence)
						continue;
					if (type == Stack.DataType.StringPtr && Stack.isPointer(index + 1))
						Var.DataType = Stack.DataType.String;
					else
						Var.DataType = type;
					continue;
				}

				Function func = Stack.PeekFunc(index + i);
				if(func != null)
				{
					if(Types.gettype(type).precedence < func.ReturnType.precedence)
						continue;
					if(type == Stack.DataType.StringPtr && Stack.isPointer(index + 1))
						func.ReturnType = Types.gettype(Stack.DataType.String);
					else
						func.ReturnType = Types.gettype(type);
					continue;
				}
				if (Stack.isnat(index + i)) ScriptFile.NativeInfo.UpdateReturnType(Stack.PeekNat(index + i), type);
			}
		}

		public void CheckInstructionString(int index, int strsize, int count = 1)
		{
			for (int i = 0; i < count; i++)
			{
				Variables.Var Var = Stack.PeekVar(index + i);
				if (Var != null && (Stack.isLiteral(index + i) || Stack.isPointer(index + i)))
				{
					if (Stack.isPointer(index + i))
					{
						if (Var.Immediatesize == 1 || Var.Immediatesize == strsize / 4)
						{
							Var.DataType = Stack.DataType.String;
							Var.Immediatesize = strsize / 4;
						}
					}
					else Var.DataType = Stack.DataType.StringPtr; continue;
				}
				if (Stack.isnat(index + i)) ScriptFile.NativeInfo.UpdateReturnType(Stack.PeekNat(index + i), Stack.DataType.StringPtr);
			}
		}

		public void CheckInstructionString(int strsize, int index = 0)
		{
			Variables.Var Var = Stack.PeekVar(index);
			if (Var != null && Stack.isPointer(index))
			{
				if (Var.Immediatesize == 1 || Var.Immediatesize == strsize/4)
				{
					Var.DataType = Stack.DataType.String;
					Var.Immediatesize = strsize/4;
				}
			}

		}

		public void SetImmediate(int size)
		{
			Variables.Var Var = Stack.PeekVar(0);
			if (Var != null && Stack.isPointer(0))
			{
				if (Var.DataType == Stack.DataType.String)
				{
					if (Var.Immediatesize != size)
					{
						Var.Immediatesize = size;
						Var.MakeStructure();
					}
				}
				else
				{
					Var.Immediatesize = size;
					Var.MakeStructure();
				}
			}
		}

		public void CheckImmediate(int size)
		{
			Variables.Var Var = Stack.PeekVar(0);
			if (Var != null && Stack.isPointer(0))
			{
				if (Var.Immediatesize < size)
					Var.Immediatesize = size;
				Var.MakeStructure();
			}
		}

		public void CheckArray(uint width, int size = -1)
		{
			Variables.Var Var = Stack.PeekVar(0);
			if (Var != null && Stack.isPointer(0))
			{
				if (Var.Value < size) Var.Value = size;
				Var.Immediatesize = (int) width;
				Var.MakeArray();
			}
			CheckInstruction(1, Stack.DataType.Int);
		}

		public void SetArray(Stack.DataType type)
		{
			if (type == Stack.DataType.Unk)
				return;

			Variables.Var Var = Stack.PeekVar(0);
			if (Var != null && Stack.isPointer(0))
				Var.DataType = type;
		}

		public void ReturnCheck(string temp)
		{
			if (Rcount != 1)
				return;
			if (ReturnType.type == Stack.DataType.Float)
				return;
			if (ReturnType.type == Stack.DataType.Int)
				return;
			if (ReturnType.type == Stack.DataType.Bool)
				return;

			if (temp.EndsWith("f"))
				ReturnType = Types.gettype(Stack.DataType.Float);

            if (int.TryParse(temp, out int tempint))
            {
                ReturnType = Types.gettype(Stack.DataType.Int);
                return;
            }
            if (temp.StartsWith("joaat("))
			{
				ReturnType = Types.gettype(Stack.DataType.Int);
				return;
			}

			if (temp.StartsWith("Function_"))
			{
				string loc = temp.Remove(temp.IndexOf("(")).Substring(9);
				if (int.TryParse(loc, out tempint))
				{
					if (Scriptfile.Functions[tempint] == this) return;

					if (!Scriptfile.Functions[tempint].Decoded)
					{
						if (!Scriptfile.Functions[tempint].DecodeStarted)
							Scriptfile.Functions[tempint].Decode();
                        else
						{
							while (!Scriptfile.Functions[tempint].Decoded)
							{
								Thread.Sleep(1);
							}
						}
					}
					switch (Scriptfile.Functions[tempint].ReturnType.type)
					{
						case Stack.DataType.Float:
						case Stack.DataType.Bool:
						case Stack.DataType.Int:
							ReturnType = Scriptfile.Functions[tempint].ReturnType;
							break;
					}
					return;
				}
			}
			if (temp.EndsWith(")") && !temp.StartsWith("("))
			{
				ReturnType = Types.gettype(Stack.DataType.Unsure);
				return;
			}
			ReturnType = Types.gettype(Stack.DataType.Unsure);
		}

		public void DecodeInstructionsForVarInfo()
		{
			Stack = new Stack();
			ReturnType = Types.gettype(Stack.DataType.Unk);
			int tempint;
			string tempstring;

			for (int i = 0; i < Instructions.Count; i++)
			{
				try
				{
					var ins = Instructions[i];
					if (ins.IsReturnInstruction)
					{
						break;
					}

					switch (ins.Instruction)
					{
						case Instruction.Nop:
							break;
						case Instruction.iAdd:
							CheckInstruction(0, Stack.DataType.Int, 2);
							Stack.Op_Add();
							break;
						case Instruction.fAdd:
							CheckInstruction(0, Stack.DataType.Float, 2);
							Stack.Op_Addf();
							break;
						case Instruction.iSub:
							CheckInstruction(0, Stack.DataType.Int, 2);
							Stack.Op_Sub();
							break;
						case Instruction.fSub:
							CheckInstruction(0, Stack.DataType.Float, 2);
							Stack.Op_Subf();
							break;
						case Instruction.iMult:
							CheckInstruction(0, Stack.DataType.Int, 2);
							Stack.Op_Mult();
							break;
						case Instruction.fMult:
							CheckInstruction(0, Stack.DataType.Float, 2);
							Stack.Op_Multf();
							break;
						case Instruction.iDiv:
							CheckInstruction(0, Stack.DataType.Int, 2);
							Stack.Op_Div();
							break;
						case Instruction.fDiv:
							CheckInstruction(0, Stack.DataType.Float, 2);
							Stack.Op_Divf();
							break;
						case Instruction.iMod:
							CheckInstruction(0, Stack.DataType.Int, 2);
							Stack.Op_Mod();
							break;
						case Instruction.fMod:
							CheckInstruction(0, Stack.DataType.Float, 2);
							Stack.Op_Modf();
							break;
						case Instruction.iNot:
							CheckInstruction(0, Stack.DataType.Bool);
							Stack.Op_Not();
							break;
						case Instruction.iNeg:
							CheckInstruction(0, Stack.DataType.Int);
							Stack.Op_Neg();
							break;
						case Instruction.fNeg:
							CheckInstruction(0, Stack.DataType.Float);
							Stack.Op_Negf();
							break;
						case Instruction.iCmpEq:
							CheckInstruction(0, Stack.DataType.Int, 2);
							Stack.Op_CmpEQ();
							break;
						case Instruction.fCmpEq:
							CheckInstruction(0, Stack.DataType.Float, 2);
							Stack.Op_CmpEQ();
							break;
						case Instruction.iCmpNe:
							CheckInstruction(0, Stack.DataType.Int, 2);
							Stack.Op_CmpEQ();
							break;
						case Instruction.fCmpNe:
							CheckInstruction(0, Stack.DataType.Float, 2);
							Stack.Op_CmpEQ();
							break;
						case Instruction.iCmpGt:
							CheckInstruction(0, Stack.DataType.Int, 2);
							Stack.Op_CmpEQ();
							break;
						case Instruction.fCmpGt:
							CheckInstruction(0, Stack.DataType.Float, 2);
							Stack.Op_CmpEQ();
							break;
						case Instruction.iCmpGe:
							CheckInstruction(0, Stack.DataType.Int, 2);
							Stack.Op_CmpEQ();
							break;
						case Instruction.fCmpGe:
							CheckInstruction(0, Stack.DataType.Float, 2);
							Stack.Op_CmpEQ();
							break;
						case Instruction.iCmpLt:
							CheckInstruction(0, Stack.DataType.Int, 2);
							Stack.Op_CmpEQ();
							break;
						case Instruction.fCmpLt:
							CheckInstruction(0, Stack.DataType.Float, 2);
							Stack.Op_CmpEQ();
							break;
						case Instruction.iCmpLe:
							CheckInstruction(0, Stack.DataType.Int, 2);
							Stack.Op_CmpEQ();
							break;
						case Instruction.fCmpLe:
							CheckInstruction(0, Stack.DataType.Float, 2);
							Stack.Op_CmpEQ();
							break;
						case Instruction.vAdd:
							Stack.Op_Vadd();
							break;
						case Instruction.vSub:
							Stack.Op_VSub();
							break;
						case Instruction.vMult:
							Stack.Op_VMult();
							break;
						case Instruction.vDiv:
							Stack.Op_VDiv();
							break;
						case Instruction.vNeg:
							Stack.Op_VNeg();
							break;
						case Instruction.And:
							Stack.Op_And();
							break;
						case Instruction.Or:
							Stack.Op_Or();
							break;
						case Instruction.Xor:
							CheckInstruction(0, Stack.DataType.Int, 2);
							Stack.Op_Xor();
							break;
						case Instruction.ItoF:
							CheckInstruction(0, Stack.DataType.Int);
							Stack.Op_Itof();
							break;
						case Instruction.FtoI:
							CheckInstruction(0, Stack.DataType.Float);
							Stack.Op_FtoI();
							break;
						case Instruction.FtoV:
							CheckInstruction(0, Stack.DataType.Float);
							Stack.Op_FtoV();
							break;
						case Instruction.iPushByte1:
							Stack.Push(ins.GetOperand(0));
							break;
						case Instruction.iPushByte2:
							Stack.Push(ins.GetOperand(0), ins.GetOperand(1));
							break;
						case Instruction.iPushByte3:
							Stack.Push(ins.GetOperand(0), ins.GetOperand(1), ins.GetOperand(2));
							break;
						case Instruction.iPushInt:
							Stack.Push(ins.GetOperandsAsInt.ToString(), Stack.DataType.Int);
							break;
						case Instruction.iPushI24:
						case Instruction.iPushShort:
							Stack.Push(ins.GetOperandsAsInt.ToString(), Stack.DataType.Int);
							break;
						case Instruction.fPush:
							Stack.Push(ins.GetFloat);
							break;
						case Instruction.dup:
							Stack.Dup();
							break;
						case Instruction.pop:
							Stack.Drop();
							break;
						case Instruction.Native:
							uint hash = Scriptfile.NativeTable.GetNativeHashFromIndex((int)ins.GetNativeIndex1);
							Stack.NativeCallTest(hash, Scriptfile.NativeTable.GetNativeFromIndex((int)ins.GetNativeIndex1), ins.GetNativeParams, ins.GetNativeReturns ? 1 : 0);
							break;
						case Instruction.Enter: break;
						case Instruction.Return:
							Stack.PopListForCall(ins.GetOperand(1));
							break;
						case Instruction.pGet:
							Stack.Op_RefGet();
							break;
						case Instruction.pSet:
							if (Stack.PeekVar(1) == null)
							{
								Stack.Drop();
								Stack.Drop();
								break;
							}
							if (Stack.TopType == Stack.DataType.Int)
							{
								tempstring = Stack.PopLit();
								if (DataUtils.IntParse(tempstring, out tempint))
								{
									Stack.PeekVar(0).Value = tempint;
								}
								break;
							}
							Stack.Drop();
							break;
						case Instruction.pPeekSet:
							if (Stack.PeekVar(1) == null)
							{
								Stack.Drop();
								break;
							}
							if (Stack.TopType == Stack.DataType.Int)
							{
								tempstring = Stack.PopLit();
								if (DataUtils.IntParse(tempstring, out tempint))
								{
									Stack.PeekVar(0).Value = tempint;
								}
							}
							break;
						case Instruction.ToStack:
							tempint = int.Parse(Stack.PeekItem(1));
							SetImmediate(tempint);
							Stack.Op_ToStack();
							break;
						case Instruction.FromStack:
							tempint = int.Parse(Stack.PeekItem(1));
							SetImmediate(tempint);
							Stack.Op_FromStack();
							break;
						case Instruction.pArray1:
						case Instruction.pArray2:
							if (!int.TryParse(Stack.PeekItem(1), out tempint)) tempint = -1;
							CheckArray(ins.GetOperandsAsUInt, tempint);
							Stack.Op_ArrayGetP(ins.GetOperandsAsUInt);
							break;
						case Instruction.ArrayGet1:
						case Instruction.ArrayGet2:
							if (!DataUtils.IntParse(Stack.PeekItem(1), out tempint)) tempint = -1;
							CheckArray(ins.GetOperandsAsUInt, tempint);
							Stack.Op_ArrayGet(ins.GetOperandsAsUInt);
							break;
						case Instruction.ArraySet1:
						case Instruction.ArraySet2:
							if (!DataUtils.IntParse(Stack.PeekItem(1), out tempint))
							{
								tempint = -1;
							}
							CheckArray(ins.GetOperandsAsUInt, tempint);
							SetArray(Stack.ItemType(2));
							Variables.Var Var = Stack.PeekVar(0);
							if (Var != null && Stack.isPointer(0))
							{
								CheckInstruction(2, Var.DataType);
							}
							Stack.Op_ArraySet(ins.GetOperandsAsUInt);
							break;
						case Instruction.pFrame1:
						case Instruction.pFrame2:
							Stack.PushPVar("FrameVar", GetFrameVar(ins.GetOperandsAsUInt));
							GetFrameVar(ins.GetOperandsAsUInt).Call();
							break;
						case Instruction.GetFrame1:
						case Instruction.GetFrame2:
							Stack.PushVar("FrameVar", GetFrameVar(ins.GetOperandsAsUInt));
							GetFrameVar(ins.GetOperandsAsUInt).Call();
							break;
						case Instruction.SetFrame1:
						case Instruction.SetFrame2:
							if (Stack.TopType != Stack.DataType.Unk)
							{
								if (Types.gettype(Stack.TopType).precedence > Types.gettype(GetFrameVar(ins.GetOperandsAsUInt).DataType).precedence)
								{
									GetFrameVar(ins.GetOperandsAsUInt).DataType = Stack.TopType;
								}
							}
							else
							{
								CheckInstruction(0, GetFrameVar(ins.GetOperandsAsUInt).DataType);
							}
							tempstring = Stack.PopLit();
							if (Stack.TopType == Stack.DataType.Int)
							{
								tempstring = Stack.PopLit();
								if (ins.GetOperandsAsUInt > Pcount && DataUtils.IntParse(tempstring, out tempint))
								{
                                    GetFrameVar(ins.GetOperandsAsUInt).Value = tempint;
                                }
							}
							else
							{
								Stack.Drop();
							}
							GetFrameVar(ins.GetOperandsAsUInt).Call();
							break;
						case Instruction.pStatic1:
						case Instruction.pStatic2:
							Stack.PushPVar("Static", Scriptfile.Statics.GetVarAtIndex(ins.GetOperandsAsUInt));
							break;
						case Instruction.StaticGet1:
						case Instruction.StaticGet2:
							Stack.PushVar("Static", Scriptfile.Statics.GetVarAtIndex(ins.GetOperandsAsUInt));
							break;
						case Instruction.StaticSet1:
						case Instruction.StaticSet2:
							if (Stack.TopType != Stack.DataType.Unk)
								Scriptfile.Statics.SetTypeAtIndex(ins.GetOperandsAsUInt, Stack.TopType);
							else
								CheckInstruction(0, Scriptfile.Statics.GetTypeAtIndex(ins.GetOperandsAsUInt));
							Stack.Drop();
							break;
						case Instruction.Add1:
						case Instruction.Add2:
						case Instruction.Mult1:
						case Instruction.Mult2:
							CheckInstruction(0, Stack.DataType.Int);
							Stack.Op_AmmImm(ins.GetOperandsAsInt);
							break;
						case Instruction.GetStruct1:
						case Instruction.GetStruct2:
							CheckImmediate((int)ins.GetOperandsAsUInt + 1);
							Stack.Op_GetImm(ins.GetOperandsAsUInt);
							break;
						case Instruction.SetStruct1:
						case Instruction.SetStruct2:
							CheckImmediate((int)ins.GetOperandsAsUInt + 1);
							Stack.Op_SetImm(ins.GetOperandsAsUInt);
							break;
						case Instruction.pGlobal2:
						case Instruction.pGlobal3:
							Stack.PushPointer("Global_" + ins.GetOperandsAsUInt.ToString());
							break;
						case Instruction.GlobalGet2:
						case Instruction.GlobalGet3:
							Stack.Push("Global_" + ins.GetOperandsAsUInt.ToString());
							break;
						case Instruction.GlobalSet2:
						case Instruction.GlobalSet3:
							Stack.Op_Set("Global_" + ins.GetOperandsAsUInt.ToString());
							break;
						case Instruction.Jump:
							break;
						case Instruction.JumpFalse:
							CheckInstruction(0, Stack.DataType.Bool);
							Stack.Drop();
							break;
						case Instruction.JumpNe:
							CheckInstruction(0, Stack.DataType.Int, 2);
							Stack.Drop();
							Stack.Drop();
							break;
						case Instruction.JumpEq:
							CheckInstruction(0, Stack.DataType.Int, 2);
							Stack.Drop();
							Stack.Drop();
							break;
						case Instruction.JumpLe:
							CheckInstruction(0, Stack.DataType.Int, 2);
							Stack.Drop();
							Stack.Drop();
							break;
						case Instruction.JumpLt:
							CheckInstruction(0, Stack.DataType.Int, 2);
							Stack.Drop();
							Stack.Drop();
							break;
						case Instruction.JumpGe:
							CheckInstruction(0, Stack.DataType.Int, 2);
							Stack.Drop();
							Stack.Drop();
							break;
						case Instruction.JumpGt:
							CheckInstruction(0, Stack.DataType.Int, 2);
							Stack.Drop();
							Stack.Drop();
							break;
						case Instruction.Call2h1:
						case Instruction.Call2h2:
						case Instruction.Call2h3:
						case Instruction.Call2h4:
						case Instruction.Call2h5:
						case Instruction.Call2h6:
						case Instruction.Call2h7:
						case Instruction.Call2h8:
						case Instruction.Call2h9:
						case Instruction.Call2hA:
						case Instruction.Call2hB:
						case Instruction.Call2hC:
						case Instruction.Call2hD:
						case Instruction.Call2hE:
						case Instruction.Call2hF:
						case Instruction.Call2:
							Function func = GetFunctionFromOffset(ins.GetOperandsAsInt);
							if (!func.preDecodeStarted)
							{
								func.PreDecode();
							}
							if (func.preDecoded)
							{
								for (int j = 0; j < func.Pcount; j++)
								{
									CheckInstruction(func.Pcount - j - 1, func.Params.GetTypeAtIndex((uint)j));
									if (Stack.ItemType(func.Pcount - j - 1) != Stack.DataType.Unk)
									{
										if (Types.gettype(Stack.ItemType(func.Pcount - j - 1)).precedence >
											Types.gettype(func.Params.GetTypeAtIndex((uint)j)).precedence)
										{
											func.Params.SetTypeAtIndex((uint)j, Stack.ItemType(func.Pcount - j - 1));
										}
									}
									CheckInstruction(func.Pcount - j - 1, func.Params.GetTypeAtIndex((uint)j));
								}
							}
							Stack.FunctionCall(func);
							break;
						case Instruction.Switch:
							CheckInstruction(0, Stack.DataType.Int);
							break;
						case Instruction.PushString:
							break;
						case Instruction.StrCopy:
							CheckInstructionString(0, ins.GetOperandsAsInt, 2);
							Stack.op_strcopy(ins.GetOperandsAsInt);
							break;
						case Instruction.ItoS:
							CheckInstructionString(0, ins.GetOperandsAsInt);
							CheckInstruction(1, Stack.DataType.Int);
							Stack.op_itos(ins.GetOperandsAsInt);
							break;
						case Instruction.StrConCat:
							CheckInstructionString(0, ins.GetOperandsAsInt, 2);
							Stack.op_stradd(ins.GetOperandsAsInt);
							break;
						case Instruction.StrConCatInt:
							CheckInstructionString(0, ins.GetOperandsAsInt);
							CheckInstruction(1, Stack.DataType.Int);
							Stack.op_straddi(ins.GetOperandsAsInt);
							break;
						case Instruction.MemCopy:
							Stack.op_sncopy();
							break;
						case Instruction.Catch:
							break;
						case Instruction.Throw:
							break;
						case Instruction.pCall:
							Stack.pcall();
							break;
						case Instruction.iPush_n1:
						case Instruction.iPush_0:
						case Instruction.iPush_1:
						case Instruction.iPush_2:
						case Instruction.iPush_3:
						case Instruction.iPush_4:
						case Instruction.iPush_5:
						case Instruction.iPush_6:
						case Instruction.iPush_7:
							Stack.Push(ins.GetImmBytePush);
							break;
						case Instruction.fPush_n1:
						case Instruction.fPush_0:
						case Instruction.fPush_1:
						case Instruction.fPush_2:
						case Instruction.fPush_3:
						case Instruction.fPush_4:
						case Instruction.fPush_5:
						case Instruction.fPush_6:
						case Instruction.fPush_7:
							Stack.Push(ins.GetImmFloatPush);
							break;
					}
				}
				catch(Exception e)
				{

				}
			}
			Vars.CheckVariables();
			Params.CheckVariables();
		}

		#endregion
	}

	public class FunctionName
	{
		private string _Name;
		private int _ParameterCount, _ReturnCount, _MinLoc, _MaxLoc;
		private ReturnTypes _ReturnType;
		private Types.DataTypes _DataType;

		internal int MaxLoc { get { return _MaxLoc; } }
		public ReturnTypes RetType { get { return _ReturnType; } set { _ReturnType = value; } }

		internal FunctionName(string name, int paramCount, int returnCount, int minLoc, int maxLoc)
		{
			_ParameterCount = paramCount;
			_ReturnCount = returnCount;
			_MinLoc = minLoc;
			_MaxLoc = maxLoc;
			_Name = name;
			_ReturnType = ReturnTypes.Unkn;
			_DataType = Types.gettype(Stack.DataType.Unk);
		}

		public string Name
		{
			get { return _Name; }
		}

		public int Pcount
		{
			get { return _ParameterCount; }
		}

		public int Rcount
		{
			get { return _ReturnCount; }
		}

		internal int MinLoc
		{
			get { return _MinLoc; }
		}

		public Types.DataTypes retType
		{
			get { return _DataType; }
			set { _DataType = value; }
		}
	}

	public enum ReturnTypes
	{
		Unkn,
		Unsure,
		Int,
		Float,
		Bool,
		BoolUnk,
		String,
		StringPtr,
		Ambiguous
	}
}
