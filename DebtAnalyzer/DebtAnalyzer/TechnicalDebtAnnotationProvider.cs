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
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Rename;

namespace DebtAnalyzer
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TechnicalDebtAnnotationProvider)), Shared]
	public class TechnicalDebtAnnotationProvider : CodeFixProvider
	{
		private const string title = "Update technical debt annotation";

		public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DebtDiagnosticAnalyzer.DiagnosticId);

		public sealed override FixAllProvider GetFixAllProvider()
		{
			return null;
		}

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			var diagnostic = context.Diagnostics.First();
			var methodSyntax = (MethodDeclarationSyntax)syntaxRoot.FindNode(context.Span);

			// Register a code action that will invoke the fix.
			context.RegisterCodeFix(
				CodeAction.Create(
					title: title,
					createChangedSolution: c => AddDebtAnnotation(context.Document, methodSyntax, c),
					equivalenceKey: title),
				diagnostic);
		}

		private async Task<Solution> AddDebtAnnotation(Document document, MethodDeclarationSyntax methodDecl, CancellationToken cancellationToken)
		{
			var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			var attributeType = typeof(DebtMethod);
			var parameterCount = methodDecl.ParameterList.Parameters.Count;
			var attribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeType.Name), SyntaxFactory.AttributeArgumentList(
								SyntaxFactory.SingletonSeparatedList(
									SyntaxFactory.AttributeArgument(
										SyntaxFactory.LiteralExpression(
											SyntaxKind.NumericLiteralExpression,
											SyntaxFactory.Literal(
												SyntaxFactory.TriviaList(),
												parameterCount.ToString(),
												parameterCount,
												SyntaxFactory.TriviaList()))))));
			var newMethod = methodDecl.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute)));
			var newRoot = syntaxRoot.ReplaceNode(methodDecl, newMethod);

			var newDocument = document.WithSyntaxRoot(newRoot);
			return newDocument.Project.Solution;
		}
	}
}