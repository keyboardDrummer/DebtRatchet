using System;

namespace DebtRatchet
{
	/// <summary>
	/// Signifies that the annotated type contains technical debt that we do not want to fix right now.
	/// The parameters FieldCount and LineCount determine what amount of technical debt we accept.
	/// If the type becomes more debtful, then DebtRatchet will report this.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
	public class DebtType : Attribute
	{
		/// <summary>
		/// The maximum number of fields that we allow in the type. Includes properties with backing fields.
		/// </summary>
		public int FieldCount { get; set; }

		/// <summary>
		/// The maximum number of lines that we allow in the type. Includes blank lines.
		/// </summary>
		public int LineCount { get; set; }
	}
}