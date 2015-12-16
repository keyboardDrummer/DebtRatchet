using System.Linq;
using Microsoft.CodeAnalysis;

namespace DebtAnalyzer
{
	public static class DebtAsErrorUtil
	{
		public static DiagnosticSeverity GetDiagnosticSeverity(ISymbol symbol)
		{
			return GetDebtAsError(symbol) ? DiagnosticSeverity.Error : DiagnosticSeverity.Warning;
		}

		public static bool GetDebtAsError(ISymbol symbol)
		{
			var assembly = symbol.ContainingAssembly;
			var maxParameters = assembly.GetAttributes().Where(data => data.AttributeClass.Name == typeof(DebtAsError).Name && data.ConstructorArguments.Length > 0).
				Select(data => new DebtAsError((bool)data.ConstructorArguments[0].Value)).FirstOrDefault() ?? new DebtAsError(false);

			return maxParameters.AsError;
		}
	}
}