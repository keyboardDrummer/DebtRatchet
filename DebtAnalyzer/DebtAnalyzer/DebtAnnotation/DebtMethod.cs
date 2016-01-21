using System;

namespace DebtAnalyzer
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class DebtMethod : Attribute
    {
		public int ParameterCount { get; set; }    
		public int LineCount { get; set; }
		public string Target { get; set; }
    }
}