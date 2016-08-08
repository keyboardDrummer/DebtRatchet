using System;

namespace DebtAnalyzer
{
	[AttributeUsage(AttributeTargets.Assembly)]
	public class MaxTypeLength : Attribute
	{
		public MaxTypeLength(int length)
		{
			Length = length;
		}

		public int Length { get; }
	}
}