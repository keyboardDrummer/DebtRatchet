using System;

namespace DebtRatchet
{
	/// <summary>
	/// Specifies the maximum number of lines that we allow our methods to have. The default is 50.
	/// </summary>
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