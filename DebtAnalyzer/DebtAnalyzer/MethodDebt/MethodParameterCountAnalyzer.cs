using System.Linq;
using DebtAnalyzer.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace DebtAnalyzer.MethodDebt
{
	public class MethodParameterCountAnalyzer
	{
		public const string DiagnosticId = "MaxParameterCount";

		static readonly LocalizableString title = new LocalizableResourceString(nameof(Resources.TooManyParametersTitle), Resources.ResourceManager, typeof(Resources));
		static readonly LocalizableString messageFormat = new LocalizableResourceString(nameof(Resources.TooManyParametersMessage), Resources.ResourceManager, typeof(Resources));
		public static int DefaultMaximumParameterCount = 8;

		public void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
		{
			var method = (BaseMethodDeclarationSyntax)context.Node;
			var methodSymbol = (IMethodSymbol)context.SemanticModel.GetDeclaredSymbol(method);
			var maxParameterCount = GetMaxParameterCount(methodSymbol.ContainingAssembly);
			var previousParameterCount = GetPreviousParameterCount(methodSymbol);
			var parameterCount = methodSymbol.Parameters.Length;
			if (parameterCount > previousParameterCount && parameterCount > maxParameterCount)
			{
				var severity = DebtAsErrorUtil.GetDiagnosticSeverity(methodSymbol);
				var descriptor = CreateDiagnosticDescriptor(severity);
				var identifier = method.GetIdentifier();
				var diagnostic = Diagnostic.Create(descriptor, identifier.GetLocation(), identifier.Text, parameterCount, maxParameterCount);

				context.ReportDiagnostic(diagnostic);
			}
		}

		public DiagnosticDescriptor CreateDiagnosticDescriptor(DiagnosticSeverity severity)
		{
			return new DiagnosticDescriptor(DiagnosticId, title, messageFormat, "Debt", severity, true);
		}

		static int GetPreviousParameterCount(IMethodSymbol methodSymbol)
		{
			return MethodDebtAnalyzer.GetDebtMethods(methodSymbol.GetAttributes()).FirstOrDefault()?.ParameterCount ?? 0;
		}

		public static int GetMaxParameterCount(IAssemblySymbol assembly)
		{
			return assembly.GetAttributes().Where(data => data.AttributeClass.Name == typeof (MaxParameters).Name && data.ConstructorArguments.Length > 0).
				Select(data => data.ConstructorArguments[0].Value as int?).FirstOrDefault() ?? DefaultMaximumParameterCount;
		}
	}
}
