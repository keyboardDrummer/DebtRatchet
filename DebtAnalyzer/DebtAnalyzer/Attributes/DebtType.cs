using System;

namespace DebtAnalyzer
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class DebtType : Attribute
	{
		public int FieldCount { get; set; }
		public int LineCount { get; set; }
	}
}