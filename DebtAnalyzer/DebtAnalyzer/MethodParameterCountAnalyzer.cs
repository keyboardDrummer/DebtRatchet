using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DebtAnalyzer
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class MethodParameterCountAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "DebtAnalyzer";

		private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
		public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, "Debt", DiagnosticSeverity.Warning, isEnabledByDefault: true);
		public const int MaximumParameterCount = 5;


		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
		}

		private static void AnalyzeSymbol(SymbolAnalysisContext context)
		{
			var methodSymbol = (IMethodSymbol)context.Symbol;
			var maxParameterCount = GetMaxParameterCount(methodSymbol);
			var previousParameterCount = GetPreviousParameterCount(methodSymbol);
			var parameterCount = methodSymbol.Parameters.Length;
			if (parameterCount > previousParameterCount && parameterCount > maxParameterCount)
			{
				var diagnostic = Diagnostic.Create(Rule, methodSymbol.Locations[0], methodSymbol.Name, parameterCount, maxParameterCount);

				context.ReportDiagnostic(diagnostic);
			}
		}

		static int GetPreviousParameterCount(IMethodSymbol methodSymbol)
		{
			var debtAttribute = methodSymbol.GetAttributes()
				.Where(data => data.AttributeClass.Name == typeof (DebtMethod).Name && data.ConstructorArguments.Length > 0)
				.Select(data => new DebtMethod((int) data.ConstructorArguments[0].Value)).FirstOrDefault() ?? new DebtMethod(0);

			return debtAttribute.ParameterCount;
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
