using System.Linq;
using DebtAnalyzer.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DebtAnalyzer.MethodDebt
{
	public class MethodLengthAnalyzer
	{
		public const string DiagnosticId = "MethodLengthAnalyzer";

		public static int DefaultMaximumMethodLength = 50;

		public void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
		{
			var method = (BaseMethodDeclarationSyntax)context.Node;
			var methodSymbol = context.SemanticModel.GetDeclaredSymbol(method);
			if (RoslynUtil.IsSymbolGenerated(methodSymbol))
			{
				return;
			}

			var methodLength = GetMethodLength(method);
			var maxLineCount = GetMaxLineCount(methodSymbol.ContainingAssembly);
			var previousMethodLength = GetPreviousMethodLength(methodSymbol);
			if (methodLength > previousMethodLength && methodLength > maxLineCount)
			{
				var severity = DebtAsErrorUtil.GetDiagnosticSeverity(methodSymbol);
				var diagnosticDescriptor = CreateDiagnosticDescriptor(severity);
				var identifier = method.GetIdentifier();
				var diagnostic = Diagnostic.Create(diagnosticDescriptor, identifier.GetLocation(), identifier.Text, methodLength, maxLineCount);

				context.ReportDiagnostic(diagnostic);
			}
		}


		public DiagnosticDescriptor CreateDiagnosticDescriptor(DiagnosticSeverity severity)
		{
			return new DiagnosticDescriptor(DiagnosticId, "Method is too long.",
				"Method {0} is {1} lines long while it should not be longer than {2} lines.", "Debt", severity, true);
		}

		public static int GetMethodLength(BaseMethodDeclarationSyntax method)
		{
			SyntaxTree tree = method.SyntaxTree;
			if (method.Body == null)
				return 0;

			var lineSpan = tree.GetLineSpan(method.Body.Statements.Span);
			return GetLineSpanLineCount(lineSpan);
		}

		public static int GetLineSpanLineCount(FileLinePositionSpan lineSpan)
		{
			var startLine = lineSpan.StartLinePosition.Line;
			var endLine = lineSpan.EndLinePosition.Line;

			return endLine - startLine + 1;
		}

		static int GetPreviousMethodLength(ISymbol methodSymbol)
		{
			return MethodDebtAnalyzer.GetDebtMethods(methodSymbol.GetAttributes()).FirstOrDefault()?.LineCount ?? 0;
		}

		public static int GetMaxLineCount(IAssemblySymbol assembly)
		{
			return assembly.GetAttributes().Where(data => data.AttributeClass.Name == typeof(MaxMethodLength).Name && data.ConstructorArguments.Length == 1).
				Select(data => data.ConstructorArguments[0].Value as int?).FirstOrDefault() ?? DefaultMaximumMethodLength;
		}
	}
}