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
			var methodSyntax = (BaseMethodDeclarationSyntax)syntaxRoot.FindNode(context.Span);

			context.RegisterCodeFix(CodeAction.Create(Title, c => AddDebtAnnotation(context.Document, methodSyntax, c), Title), diagnostic);
		}

		Task<Solution> AddDebtAnnotation(Document document, BaseMethodDeclarationSyntax methodBaseDecl, CancellationToken cancellationToken)
		{
			return AddInlineDebtAnnotation(document, methodBaseDecl, cancellationToken);
		}

		static async Task<Solution> AddExternalDebtAnnotation(Document document, BaseMethodDeclarationSyntax methodBaseDecl, CancellationToken cancellationToken)
		{
			var name = "TechDebtAnnotations.cs";
			var debtDocument = document.Project.Documents.FirstOrDefault(projectDocument => projectDocument.Name == name);
			if (debtDocument == null)
			{
				debtDocument = document.Project.AddDocument(name, "");
			}
			var syntaxRoot = (CompilationUnitSyntax)await debtDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			syntaxRoot = AddUsing(syntaxRoot);
			syntaxRoot.AddAttributeLists(GetAttributeListSyntax(methodBaseDecl));

			var newDocument = document.WithSyntaxRoot(syntaxRoot);
			return newDocument.Project.Solution;
		}

		static async Task<Solution> AddInlineDebtAnnotation(Document document, BaseMethodDeclarationSyntax methodBaseDecl, CancellationToken cancellationToken)
		{
			var syntaxRoot = (CompilationUnitSyntax) await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			syntaxRoot = syntaxRoot.ReplaceNode(methodBaseDecl, GetNewMethod(methodBaseDecl));
			syntaxRoot = AddUsing(syntaxRoot);

			var newDocument = document.WithSyntaxRoot(syntaxRoot);
			return newDocument.Project.Solution;
		}

		static CompilationUnitSyntax AddUsing(CompilationUnitSyntax syntaxRoot)
		{
			var debtAnalyzerNamespace = SyntaxFactory.IdentifierName("DebtAnalyzer");
			var usingDirectiveSyntax = SyntaxFactory.UsingDirective(
				debtAnalyzerNamespace).WithUsingKeyword(SyntaxFactory.Token(SyntaxKind.UsingKeyword))
				.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

			var withUsing = syntaxRoot.Usings.All(@using => (@using.Name as IdentifierNameSyntax)?.Identifier.ValueText != debtAnalyzerNamespace.Identifier.ValueText) 
				? syntaxRoot.AddUsings(usingDirectiveSyntax) 
				: syntaxRoot;
			return withUsing;
		}

		static BaseMethodDeclarationSyntax GetNewMethod(BaseMethodDeclarationSyntax methodBaseDecl)
		{
			var attributeListSyntax = GetAttributeListSyntax(methodBaseDecl);

			var methodDecl = methodBaseDecl as MethodDeclarationSyntax;
			if (methodDecl != null)
			{
				return methodDecl.WithoutTrivia().
					AddAttributeLists(attributeListSyntax).
					WithTriviaFrom(methodBaseDecl);
			}
			return ((ConstructorDeclarationSyntax) methodBaseDecl).WithoutTrivia().
				AddAttributeLists(attributeListSyntax).
				WithTriviaFrom(methodBaseDecl);
		}

		public static AttributeListSyntax GetAttributeListSyntax(BaseMethodDeclarationSyntax methodBaseDecl)
		{
			var attributeType = typeof (DebtMethod);
			var methodLength = MethodLengthAnalyzer.GetMethodLength(methodBaseDecl);
			var lineCountArgument = GetNamedAttributeArgument(nameof(DebtMethod.LineCount), methodLength);
			var parameterCountArgument = GetNamedAttributeArgument(nameof(DebtMethod.ParameterCount), methodBaseDecl.ParameterList.Parameters.Count);
			var attribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeType.Name), SyntaxFactory.AttributeArgumentList(
				SyntaxFactory.SeparatedList(new[] {lineCountArgument, parameterCountArgument})));

			return SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute));
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