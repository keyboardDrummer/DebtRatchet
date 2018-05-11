using System;

namespace DebtRatchet
{
	/// <summary>
	/// Specifies if designer types (e.g. resource files) are ignored when counting lines in types. The default is true.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	public class IgnoreDesignerTypes : Attribute
	{
		public IgnoreDesignerTypes(bool doIgnore)
		{
			DoIgnore = doIgnore;
		}

		public bool DoIgnore { get; }
	}
}