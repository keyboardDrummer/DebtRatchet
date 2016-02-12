using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DebtAnalyzer.Attributes;
using DebtAnalyzer.MethodLength;
using DebtAnalyzer.ParameterCount;
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
		public const string Title = "Update technical debt annotation";
		const bool DefaultUseExternalAttribute = false;
		const string ExternalAnnotationsFileName = "TechDebtAnnotations.cs";

		public override sealed ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
			MethodParameterCountAnalyzer.DiagnosticId, MethodLengthAnalyzer.DiagnosticId);

		public override sealed FixAllProvider GetFixAllProvider()
		{
			return new MyFixAllProvider();
		}

		public override sealed async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			
			var diagnostic = context.Diagnostics.First();
			var methodSyntax = (BaseMethodDeclarationSyntax)syntaxRoot.FindNode(context.Span);

			context.RegisterCodeFix(CodeAction.Create(Title, async c =>
			{
				var project = await AddDebtAnnotation(context.Document, methodSyntax, c);
				return project.Solution;
			}, methodSyntax.SpanStart + ""), diagnostic);
		}

		public static async Task<Project> AddDebtAnnotation(Document document, BaseMethodDeclarationSyntax methodBaseDecl, CancellationToken cancellationToken)
		{
			var sem = await document.GetSemanticModelAsync(cancellationToken);
			var methodSymbol = sem.GetDeclaredSymbol(methodBaseDecl);
			if (GetUseExternalAttributes(methodSymbol.ContainingAssembly))
			{
				return await AddExternalDebtAnnotation(document, methodSymbol, methodBaseDecl, cancellationToken);
			}
			return await AddInlineDebtAnnotation(document, methodBaseDecl, cancellationToken);
		}

		static bool GetUseExternalAttributes(IAssemblySymbol assembly)
		{
			return assembly.GetAttributes().Where(data => data.AttributeClass.Name == typeof(GenerateExternalAttribute).Name && data.ConstructorArguments.Length > 0).
				Select(data => data.ConstructorArguments[0].Value as bool?).FirstOrDefault() ?? DefaultUseExternalAttribute;
		}

		static async Task<Project> AddExternalDebtAnnotation(Document document, IMethodSymbol symbol, BaseMethodDeclarationSyntax methodBaseDecl, CancellationToken cancellationToken)
		{
			var debtDocument = document.Project.Documents.FirstOrDefault(projectDocument => projectDocument.Name == ExternalAnnotationsFileName);
			if (debtDocument == null)
			{
				debtDocument = document.Project.AddDocument(ExternalAnnotationsFileName, "");
			}
			var syntaxRoot = (CompilationUnitSyntax)await debtDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			syntaxRoot = AddUsing(syntaxRoot);
			syntaxRoot = syntaxRoot.AddAttributeLists(GetAttributeListSyntax(symbol, methodBaseDecl));

			var newDocument = debtDocument.WithSyntaxRoot(syntaxRoot);
			return newDocument.Project;
		}

		static BaseMethodDeclarationSyntax RemoveExistingDebtAnnotations(BaseMethodDeclarationSyntax methodBaseDecl)
		{
			return (BaseMethodDeclarationSyntax)new RemoveEmptyAttributeLists().Visit(new RemoveDebtMethods().Visit(methodBaseDecl));
		}

		static async Task<Project> AddInlineDebtAnnotation(Document document, BaseMethodDeclarationSyntax methodBaseDecl, CancellationToken cancellationToken)
		{
			var syntaxRoot = (CompilationUnitSyntax) await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			var withoutDebtAnnotations = RemoveExistingDebtAnnotations(methodBaseDecl);
			syntaxRoot = syntaxRoot.ReplaceNode(methodBaseDecl, GetNewMethod(withoutDebtAnnotations));
			syntaxRoot = AddUsing(syntaxRoot);

			var newDocument = document.WithSyntaxRoot(syntaxRoot);
			return newDocument.Project;
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

		public static AttributeListSyntax GetAttributeListSyntax(IMethodSymbol symbol, BaseMethodDeclarationSyntax methodBaseDecl)
		{
			var lineCountArgument = GetLineCountArgument(methodBaseDecl);
			var parameterCountArgument = GetParameterCountArgument(methodBaseDecl);
			var targetArgument = GetNamedAttributeArgument(nameof(DebtMethod.Target), '"' + DebtAnalyzer.GetFullName(symbol) + '"');
			var attributeArgumentSyntaxs = new List<AttributeArgumentSyntax> { lineCountArgument, parameterCountArgument, targetArgument };
			var attribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(typeof(DebtMethod).Name), SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(attributeArgumentSyntaxs)));

			return SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute)).WithTarget(
				SyntaxFactory.AttributeTargetSpecifier(SyntaxFactory.Token(SyntaxKind.AssemblyKeyword)));
		}

		public static AttributeListSyntax GetAttributeListSyntax(BaseMethodDeclarationSyntax methodBaseDecl)
		{
			var attributeType = typeof (DebtMethod);
			var lineCountArgument = GetLineCountArgument(methodBaseDecl);
			var parameterCountArgument = GetParameterCountArgument(methodBaseDecl);
			var attributeArgumentSyntaxs = new List<AttributeArgumentSyntax> {lineCountArgument, parameterCountArgument};
			var attribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeType.Name), SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(attributeArgumentSyntaxs)));
			
			return SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute));
		}

		static AttributeArgumentSyntax GetParameterCountArgument(BaseMethodDeclarationSyntax methodBaseDecl)
		{
			var parameterCountArgument = GetNamedAttributeArgument(nameof(DebtMethod.ParameterCount), methodBaseDecl.ParameterList.Parameters.Count);
			return parameterCountArgument;
		}

		static AttributeArgumentSyntax GetLineCountArgument(BaseMethodDeclarationSyntax methodBaseDecl)
		{
			var methodLength = MethodLengthAnalyzer.GetMethodLength(methodBaseDecl);
			var lineCountArgument = GetNamedAttributeArgument(nameof(DebtMethod.LineCount), methodLength);
			return lineCountArgument;
		}

		static AttributeArgumentSyntax GetNamedAttributeArgument(string parameterName, object parameterValue)
		{

			return SyntaxFactory.AttributeArgument(
				SyntaxFactory.LiteralExpression(
					SyntaxKind.NumericLiteralExpression,
					SyntaxFactory.Literal(
						SyntaxFactory.TriviaList(),
						parameterValue.ToString(),
						parameterValue.ToString(),
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