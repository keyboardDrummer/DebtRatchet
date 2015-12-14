using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DebtAnalyzer
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class MethodLengthAnalayzer : DiagnosticAnalyzer
	{

		public const string DiagnosticId = "MethodLengthAnalayzer";
		
		public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, "Method is too long.", 
			"Method {0} is {1} lines long while it should be longer than {2} lines.", "Debt", DiagnosticSeverity.Warning, true);

		public const int MaximumMethodLength = 20;
		
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.MethodDeclaration);
		}

		void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
		{
			var method = (MethodDeclarationSyntax)context.Node;
			SyntaxTree tree = method.SyntaxTree;
			var lineSpan = tree.GetLineSpan(method.Span);
			var startLine = lineSpan.StartLinePosition.Line;
			var endLine = lineSpan.EndLinePosition.Line;

			var methodLength = endLine - startLine;
			if (methodLength > MaximumMethodLength)
			{
				var diagnostic = Diagnostic.Create(Rule, method.GetLocation(), method.Identifier.Text, methodLength, MaximumMethodLength);

				context.ReportDiagnostic(diagnostic);
			}
		}

	}
}