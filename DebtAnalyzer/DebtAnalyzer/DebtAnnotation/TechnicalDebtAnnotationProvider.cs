using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DebtAnalyzer.DebtAnnotation
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TechnicalDebtAnnotationProvider)), Shared]
	public class TechnicalDebtAnnotationProvider : CodeFixProvider
	{
		const string Title = "Update technical debt annotation";

		public override sealed ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
			MethodParameterCountAnalyzer.DiagnosticId, MethodLengthAnalyzer.DiagnosticId);

		public override sealed FixAllProvider GetFixAllProvider()
		{
			return WellKnownFixAllProviders.BatchFixer;
		}

		public override sealed async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			var diagnostic = context.Diagnostics.First();
			var methodSyntax = (MethodDeclarationSyntax) syntaxRoot.FindNode(context.Span);

			context.RegisterCodeFix(CodeAction.Create(Title, c => AddDebtAnnotation(context.Document, methodSyntax, c), Title), diagnostic);
		}

		async Task<Solution> AddDebtAnnotation(Document document, MethodDeclarationSyntax methodDecl, CancellationToken cancellationToken)
		{
			var syntaxRoot = (CompilationUnitSyntax) await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			var attributeType = typeof (DebtMethod);
			var methodLength = MethodLengthAnalyzer.GetMethodLength(methodDecl);
			var lineCountArgument = GetNamedAttributeArgument(nameof(DebtMethod.LineCount), methodLength);
			var parameterCountArgument = GetNamedAttributeArgument(nameof(DebtMethod.ParameterCount), methodDecl.ParameterList.Parameters.Count);
			var attribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeType.Name), SyntaxFactory.AttributeArgumentList(
				SyntaxFactory.SeparatedList(new [] { lineCountArgument, parameterCountArgument })));
			var newMethod = methodDecl.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute)));
			syntaxRoot = syntaxRoot.ReplaceNode(methodDecl, newMethod);

			var debtAnalyzerNamespace = SyntaxFactory.IdentifierName("DebtAnalyzer");
			var usingDirectiveSyntax = SyntaxFactory.UsingDirective(
				debtAnalyzerNamespace).WithUsingKeyword(SyntaxFactory.Token(SyntaxKind.UsingKeyword))
				.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

			if (syntaxRoot.Usings.All(@using => (@using.Name as IdentifierNameSyntax)?.Identifier.ValueText != debtAnalyzerNamespace.Identifier.ValueText)) //TODO really hacky solution
			{
				syntaxRoot = syntaxRoot.AddUsings(usingDirectiveSyntax);
			}
			
			var newDocument = document.WithSyntaxRoot(syntaxRoot);
			return newDocument.Project.Solution;
		}

		static AttributeArgumentSyntax GetNamedAttributeArgument(string parameterName, int parameterValue)
		{
			return SyntaxFactory.AttributeArgument(
				SyntaxFactory.LiteralExpression(
					SyntaxKind.NumericLiteralExpression,
					SyntaxFactory.Literal(
						SyntaxFactory.TriviaList(),
						parameterValue.ToString(),
						parameterValue,
						SyntaxFactory.TriviaList())))
				.WithNameEquals(
					SyntaxFactory.NameEquals(
						SyntaxFactory.IdentifierName(
							parameterName))
						.WithEqualsToken(
							SyntaxFactory.Token(
								SyntaxKind.EqualsToken)));
		}
	}
}