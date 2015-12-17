using System;

namespace DebtAnalyzer
{
	[AttributeUsage(AttributeTargets.Assembly)]
	class MaximumMethodLength : Attribute
	{
		public MaximumMethodLength(int length)
		{
			Length = length;
		}

		public int Length { get; }
	}
}