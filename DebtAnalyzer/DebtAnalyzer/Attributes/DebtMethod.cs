using System;

namespace DebtAnalyzer
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = true)]
    public class DebtMethod : Attribute
    {
		public int ParameterCount { get; set; }    
		public int LineCount { get; set; }
    }
}