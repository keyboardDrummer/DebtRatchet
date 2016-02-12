using System;

namespace DebtAnalyzer
{
	[AttributeUsage(AttributeTargets.Assembly)]
	public class MaxParameters : Attribute
	{
		public MaxParameters(int parameterCount)
		{
			ParameterCount = parameterCount;
		}

		public int ParameterCount { get; }
	}
}