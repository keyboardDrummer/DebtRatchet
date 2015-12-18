using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DebtAnalyzer
{
	public static class DebtAsErrorUtil
	{
		public static DiagnosticSeverity GetDiagnosticSeverity(ISymbol symbol)
		{
			switch (GetDebtAsError(symbol))
			{
				case Severity.Info: return DiagnosticSeverity.Info;
				case Severity.Warning: return DiagnosticSeverity.Warning;
				case Severity.Error: return DiagnosticSeverity.Error;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static Severity GetDebtAsError(ISymbol symbol)
		{
			var assembly = symbol.ContainingAssembly;
			int? intEnum = assembly.GetAttributes().Where(data => data.AttributeClass.Name == typeof (DebtSeverity).Name && data.ConstructorArguments.Length > 0).
				Select(data => data.ConstructorArguments[0].Value as int?).FirstOrDefault();
			return ((Severity?)intEnum) ?? Severity.Info;
		}
	}
}