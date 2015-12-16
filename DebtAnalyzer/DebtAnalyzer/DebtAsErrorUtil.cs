using System.Linq;
using Microsoft.CodeAnalysis;

namespace DebtAnalyzer
{
	public static class DebtAsErrorUtil
	{
		public static bool GetDebtAsError(ISymbol methodSymbol)
		{
			var assembly = methodSymbol.ContainingAssembly;
			var maxParameters = assembly.GetAttributes().Where(data => data.AttributeClass.Name == typeof(DebtAsError).Name && data.ConstructorArguments.Length > 0).
				Select(data => new DebtAsError((bool)data.ConstructorArguments[0].Value)).FirstOrDefault() ?? new DebtAsError(false);

			return maxParameters.AsError;
		}
	}
}