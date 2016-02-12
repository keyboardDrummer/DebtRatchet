using System;

namespace DebtAnalyzer.Attributes
{
	[AttributeUsage(AttributeTargets.Assembly)]
	public class MaxMethodLength : Attribute
	{
		public MaxMethodLength(int length)
		{
			Length = length;
		}

		public int Length { get; }
	}
}