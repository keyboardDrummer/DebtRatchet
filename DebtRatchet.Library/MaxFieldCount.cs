using System;

namespace DebtRatchet
{
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