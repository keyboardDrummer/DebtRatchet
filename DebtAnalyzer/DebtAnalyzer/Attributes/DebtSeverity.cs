using System;

namespace DebtAnalyzer.Attributes
{
	public enum Severity { Info, Warning, Error }

	[AttributeUsage(AttributeTargets.Assembly)]
	public class DebtSeverity : Attribute
	{
		public DebtSeverity(Severity severity)
		{
			Severity = severity;
		}

		public Severity Severity { get; }
	}
}