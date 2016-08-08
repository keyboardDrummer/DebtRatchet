using System;

namespace DebtRatchet
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