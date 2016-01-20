using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DebtAnalyzer
{
	public class MethodLengthAnalyzer
	{
		public const string DiagnosticId = "MethodLengthAnalyzer";

		public const int DefaultMaximumMethodLength = 20;

		public void AnalyzeSyntax(SyntaxNodeAnalysisContext context, Dictionary<string, DebtMethod> names)
		{
			var method = (MethodDeclarationSyntax)context.Node;
			var methodSymbol = context.SemanticModel.GetDeclaredSymbol(method);
			var methodLength = GetMethodLength(method);
			var maxLineCount = GetMaxLineCount(methodSymbol);
			if (methodLength > GetPreviousMethodLength(names, methodSymbol) && methodLength > maxLineCount)
			{
				var severity = DebtAsErrorUtil.GetDiagnosticSeverity(methodSymbol);
				var diagnosticDescriptor = CreateDiagnosticDescriptor(severity);
				var diagnostic = Diagnostic.Create(diagnosticDescriptor, method.GetLocation(), method.Identifier.Text, methodLength, maxLineCount);

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
				return 0; //TODO add testcase for abstract method.

			var lineSpan = tree.GetLineSpan(method.Body.Statements.Span);
			var startLine = lineSpan.StartLinePosition.Line;
			var endLine = lineSpan.EndLinePosition.Line;

			return endLine - startLine + 1;
		}

		static int GetPreviousMethodLength(IReadOnlyDictionary<string, DebtMethod> assemblyAnnotations, IMethodSymbol methodSymbol)
		{
			var fromDirectAttribute = DebtAnalyzer.GetDebtMethods(methodSymbol.GetAttributes()).FirstOrDefault();
			var fullName = DebtAnalyzer.GetFullName(methodSymbol);
			return (fromDirectAttribute ?? assemblyAnnotations.Get(fullName, () => null))?.LineCount ?? 0;
		}

		static int GetMaxLineCount(IMethodSymbol methodSymbol)
		{
			var assembly = methodSymbol.ContainingAssembly;

			return assembly.GetAttributes().Where(data => data.AttributeClass.Name == typeof(MaxMethodLength).Name && data.ConstructorArguments.Length == 1).
				Select(data => data.ConstructorArguments[0].Value as int?).FirstOrDefault() ?? DefaultMaximumMethodLength;
		}
	}
}