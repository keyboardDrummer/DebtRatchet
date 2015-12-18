namespace DebtAnalyzer.Test
{
	static class DebtAnalyzerTestUtil
	{
		public static string DebtMethodAnnotation => @"using System;

namespace DebtAnalyzer
{
    
	[AttributeUsage(AttributeTargets.Method)]
    public class DebtMethod : Attribute
    {
		public int ParameterCount { get; set; }    
		public int LineCount { get; set; }
    }
}";


		public static string DebtAsError => @"
using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using DebtAnalyzer;

[assembly: DebtSeverity(Severity.Error)]
namespace DebtAnalyzer
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
}";
	}
}