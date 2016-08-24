using System;

namespace DebtRatchet
{
	/// <summary>
	/// Signifies that the annotated method contains technical debt that we do not want to fix right now.
	/// The parameters ParameterCount and LineCount determine what amount of technical debt we accept.
	/// If the method becomes more debtful, then DebtRatchet will report this.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public class MethodHasDebt : Attribute
    {
		/// <summary>
		/// The maximum amount of parameters that we allow in the method.
		/// </summary>
		public int ParameterCount { get; set; }

		/// <summary>
		/// The maximum number of lines that we allow in the method. Includes blank lines.
		/// </summary>
		public int LineCount { get; set; }
    }
}