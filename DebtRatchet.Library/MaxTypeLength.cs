using System;

namespace DebtRatchet
{
	/// <summary>
	/// Specifies the maximum number of lines that we allow our types to have. The default is 1000.
	/// </summary>
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