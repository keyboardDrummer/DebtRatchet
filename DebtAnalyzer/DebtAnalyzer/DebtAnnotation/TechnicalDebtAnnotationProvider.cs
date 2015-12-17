using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DebtAnalyzer
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TechnicalDebtAnnotationProvider)), Shared]
	public class TechnicalDebtAnnotationProvider : CodeFixProvider
	{
		const string title = "Update technical debt annotation";

		public override sealed ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MethodParameterCountAnalyzer.DiagnosticId);

		public override sealed FixAllProvider GetFixAllProvider()
		{
			return WellKnownFixAllProviders.BatchFixer;
		}

		public override sealed async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			var diagnostic = context.Diagnostics.First();
			var methodSyntax = (MethodDeclarationSyntax) syntaxRoot.FindNode(context.Span);

			// Register a code action that will invoke the fix.
			context.RegisterCodeFix(
				CodeAction.Create(title, c => AddDebtAnnotation(context.Document, methodSyntax, c), title),
				diagnostic);
		}

		async Task<Solution> AddDebtAnnotation(Document document, MethodDeclarationSyntax methodDecl, CancellationToken cancellationToken)
		{
			var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			var attributeType = typeof (DebtMethod);

			var parameterCount = methodDecl.ParameterList.Parameters.Count;
			var attributeArgument = SyntaxFactory.AttributeArgument(
				SyntaxFactory.LiteralExpression(
					SyntaxKind.NumericLiteralExpression,
					SyntaxFactory.Literal(
						SyntaxFactory.TriviaList(),
						parameterCount.ToString(),
						parameterCount,
						SyntaxFactory.TriviaList())))
				.WithNameEquals(
					SyntaxFactory.NameEquals(
						SyntaxFactory.IdentifierName(
							@"ParameterCount"))
						.WithEqualsToken(
							SyntaxFactory.Token(
								SyntaxKind.EqualsToken)));
			var attribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeType.Name), SyntaxFactory.AttributeArgumentList(
				SyntaxFactory.SingletonSeparatedList(
					attributeArgument)));
			var newMethod = methodDecl.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute)));
			var newRoot = syntaxRoot.ReplaceNode(methodDecl, newMethod);

			var newDocument = document.WithSyntaxRoot(newRoot);
			return newDocument.Project.Solution;
		}
	}
}