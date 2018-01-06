using System;

namespace DebtRatchet
{
	/// <summary>
	/// Describes how severe an increase in technical debt is.
	/// </summary>
	public enum Severity
	{
		/// <summary>
		/// Increases in technical debt cause a compilation information message.
		/// </summary>
		Info,
		/// <summary>
		/// Increases in technical debt cause a compilation warning.
		/// </summary>
		/// 
		Warning,
		/// <summary>
		/// Increases in technical debt cause a compilation error.
		/// </summary>
		Error
	}

	/// <summary>
	/// Determines what happens when DebtRachet finds code that is too debtful.
	/// </summary>
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