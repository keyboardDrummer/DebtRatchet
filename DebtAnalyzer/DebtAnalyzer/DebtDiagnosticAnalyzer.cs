using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DebtAnalyzer
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class DebtDiagnosticAnalyzer : DiagnosticAnalyzer
	{

		public const string DiagnosticId = "DebtAnalyzer";

		private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
		public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, "Debt", DiagnosticSeverity.Warning, isEnabledByDefault: true);
		// You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
		public const int MaximumParameterCount = 5;


		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
		}

		private static void AnalyzeSymbol(SymbolAnalysisContext context)
		{
			var methodSymbol = (IMethodSymbol)context.Symbol;
			var attribute = methodSymbol.GetAttributes()
				.Where(data => false) //data.AttributeClass.Name == typeof (DebtMethod).Name)
				.Select(data => new DebtMethod((int)data.ConstructorArguments[0].Value)).FirstOrDefault() ?? new DebtMethod(0);

			var parameterCount = methodSymbol.Parameters.Length;
			if (parameterCount > attribute.ParameterCount && parameterCount > MaximumParameterCount)
			{
				// For all such symbols, produce a diagnostic.
				var diagnostic = Diagnostic.Create(Rule, methodSymbol.Locations[0], methodSymbol.Name, parameterCount, MaximumParameterCount);

				context.ReportDiagnostic(diagnostic);
			}
		}
	}
}
