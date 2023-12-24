using System.Collections.Generic;
using System.Linq;

namespace Magic_RDR
{
	internal class CodePath
	{
		public CodePath Parent;
		public int EndOffset;
		public int BreakOffset;
		public CodePathType Type;
		public List<CodePath> ChildPaths;

		public CodePath(CodePathType type, int endOffset, int breakOffset)
		{
			Parent = null;
			Type = type;
			EndOffset = endOffset;
			BreakOffset = breakOffset;
			ChildPaths = new List<CodePath>();
		}

		public CodePath(CodePath parent, CodePathType type, int endOffset, int breakOffset)
		{
			Parent = parent;
			Type = type;
			EndOffset = endOffset;
			BreakOffset = breakOffset;
			ChildPaths = new List<CodePath>();
		}

		public CodePath CreateCodePath(CodePathType type, int endOffset, int breakOffset)
		{
			CodePath path = new CodePath(this, type, endOffset, breakOffset);
			ChildPaths.Add(path);
			return path;
		}

	}

	internal enum CodePathType
	{
		While,
		If,
		Else,
		Main,
		For
	}

	internal class SwitchStatement
	{
		public Dictionary<int, List<string>> Cases;
		public List<int> Offsets;
		public int BreakOffset;
		public SwitchStatement Parent;
		public List<SwitchStatement> ChildSwitches;

		public SwitchStatement(Dictionary<int, List<string>> cases, int breakOffset)
		{
			Parent = null;
			Cases = cases;
			BreakOffset = breakOffset;
			ChildSwitches = new List<SwitchStatement>();
			Offsets = Cases == null ? new List<int>() : Cases.Keys.ToList();
			Offsets.Add(breakOffset);
		}

		public SwitchStatement(SwitchStatement parent, Dictionary<int, List<string>> cases, int breakOffset)
		{
			Parent = parent;
			Cases = cases;
			BreakOffset = breakOffset;
			ChildSwitches = new List<SwitchStatement>();
			Offsets = Cases == null ? new List<int>() : Cases.Keys.ToList();
			Offsets.Add(BreakOffset);
		}

		public SwitchStatement CreateSwitchStatement(Dictionary<int, List<string>> cases, int breakOffset)
		{
			SwitchStatement statement = new SwitchStatement(this, cases, breakOffset);
			ChildSwitches.Add(statement);
			return statement;
		}
	}
}
