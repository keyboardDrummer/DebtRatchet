using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DebtAnalyzer
{
	public class MethodParameterCountAnalyzer
	{
		public const string DiagnosticId = "MaxParameterCount";

		private static readonly LocalizableString title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString messageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
		public const int MaximumParameterCount = 5;

		public void AnalyzeSymbol(SymbolAnalysisContext context, Dictionary<string, DebtMethod> names)
		{
			var methodSymbol = (IMethodSymbol)context.Symbol;
			var maxParameterCount = GetMaxParameterCount(methodSymbol);
			var previousParameterCount = GetPreviousParameterCount(methodSymbol, names);
			var parameterCount = methodSymbol.Parameters.Length;
			if (parameterCount > previousParameterCount && parameterCount > maxParameterCount)
			{
				var severity = DebtAsErrorUtil.GetDiagnosticSeverity(methodSymbol);
				var descriptor = CreateDiagnosticDescriptor(severity);
				var diagnostic = Diagnostic.Create(descriptor, methodSymbol.Locations[0], methodSymbol.Name, parameterCount, maxParameterCount);

				context.ReportDiagnostic(diagnostic);
			}
		}

		public DiagnosticDescriptor CreateDiagnosticDescriptor(DiagnosticSeverity severity)
		{
			return new DiagnosticDescriptor(DiagnosticId, title, messageFormat, "Debt", severity, true);
		}

		static int GetPreviousParameterCount(IMethodSymbol methodSymbol, Dictionary<string, DebtMethod> names)
		{
			var fromDirectAttribute = DebtAnalyzer.GetDebtMethods(methodSymbol.GetAttributes()).FirstOrDefault();
			var fullName = DebtAnalyzer.GetFullName(methodSymbol);
			return (fromDirectAttribute ?? names.Get(fullName, () => null))?.ParameterCount ?? 0;
		}

		static int GetMaxParameterCount(IMethodSymbol methodSymbol)
		{
			var assembly = methodSymbol.ContainingAssembly;
			var maxParameters = assembly.GetAttributes().Where(data => data.AttributeClass.Name == typeof (MaxParameters).Name && data.ConstructorArguments.Length > 0).
				Select(data => new MaxParameters((int) data.ConstructorArguments[0].Value)).FirstOrDefault() ?? new MaxParameters(MaximumParameterCount);

			return maxParameters.ParameterCount;
		}
	}
}
