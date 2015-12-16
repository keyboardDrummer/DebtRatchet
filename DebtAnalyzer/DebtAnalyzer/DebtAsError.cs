using System;

namespace DebtAnalyzer
{
	[AttributeUsage(AttributeTargets.Assembly)]
	public class DebtAsError : Attribute
	{
		public DebtAsError(bool asError)
		{
			AsError = asError;
		}

		public bool AsError { get; }
	}
}