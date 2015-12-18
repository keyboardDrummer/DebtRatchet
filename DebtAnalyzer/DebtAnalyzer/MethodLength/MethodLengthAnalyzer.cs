using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DebtAnalyzer
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class MethodLengthAnalyzer : DiagnosticAnalyzer
	{

		public const string DiagnosticId = "MethodLengthAnalyzer";

		public const int DefaultMaximumMethodLength = 20;


		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(new DiagnosticDescriptor(DiagnosticId, "Method is too long.", 
			"Method {0} is {1} lines long while it should be longer than {2} lines.", "Debt", DiagnosticSeverity.Warning, true));

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.MethodDeclaration);
		}

		void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
		{
			var method = (MethodDeclarationSyntax)context.Node;
			var methodSymbol = context.SemanticModel.GetDeclaredSymbol(method);
			var methodLength = GetMethodLength(method);
			var maxLineCount = GetMaxLineCount(methodSymbol);
			if (methodLength > GetPreviousMethodLength(methodSymbol) && methodLength > maxLineCount)
			{
				var severity = DebtAsErrorUtil.GetDiagnosticSeverity(methodSymbol);
				var diagnosticDescriptor = new DiagnosticDescriptor(DiagnosticId, "Method is too long.",
					"Method {0} is {1} lines long while it should not be longer than {2} lines.", "Debt", severity, true);
				var diagnostic = Diagnostic.Create(diagnosticDescriptor, method.GetLocation(), method.Identifier.Text, methodLength, maxLineCount);

				context.ReportDiagnostic(diagnostic);
			}
		}

		public static int GetMethodLength(MethodDeclarationSyntax method)
		{
			SyntaxTree tree = method.SyntaxTree;
			if (method.Body == null)
				return 0; //TODO add testcase for abstract method.

			var lineSpan = tree.GetLineSpan(method.Body.Statements.Span);
			var startLine = lineSpan.StartLinePosition.Line;
			var endLine = lineSpan.EndLinePosition.Line;

			return endLine - startLine + 1;
		}

		const string LineCountName = nameof(DebtMethod.LineCount);
		static int GetPreviousMethodLength(IMethodSymbol methodSymbol)
		{
			return methodSymbol.GetAttributes()
				.Where(data => data.AttributeClass.Name == typeof(DebtMethod).Name)
				.Select(data => data.NamedArguments.FirstOrDefault(kv => kv.Key == LineCountName).Value.Value as int?).FirstOrDefault() ?? 0;
		}

		static int GetMaxLineCount(IMethodSymbol methodSymbol)
		{
			var assembly = methodSymbol.ContainingAssembly;

			return assembly.GetAttributes().Where(data => data.AttributeClass.Name == typeof(MaxMethodLength).Name && data.ConstructorArguments.Length == 1).
				Select(data => data.ConstructorArguments[0].Value as int?).FirstOrDefault() ?? DefaultMaximumMethodLength;
		}

	}
}