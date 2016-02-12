using System;

namespace DebtAnalyzer.Attributes
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