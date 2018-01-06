using System;

namespace DebtRatchet
{
	/// <summary>
	/// Specifies the maximum number of fields that we allow our types to have. The default is 50.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	public class MaxFieldCount : Attribute
	{
		public MaxFieldCount(int maxFieldCount)
		{
			Count = maxFieldCount;
		}

		public int Count { get; }
	}
}