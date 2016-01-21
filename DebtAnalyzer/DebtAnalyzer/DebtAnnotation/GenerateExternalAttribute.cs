using System;

namespace DebtAnalyzer
{
	[AttributeUsage(AttributeTargets.Assembly)]
	public class GenerateExternalAttribute : Attribute
	{
		public bool UseExternal { get; }

		public GenerateExternalAttribute(bool useExternal)
		{
			UseExternal = useExternal;
		}
	}
}