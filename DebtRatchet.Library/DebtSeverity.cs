using System;

namespace DebtRatchet
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