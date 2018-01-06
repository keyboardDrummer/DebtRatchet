using System;

namespace DebtRatchet
{
	/// <summary>
	/// Specifies the maximum number of parameters that we allow our methods to have. The default is 8.
	/// </summary>
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