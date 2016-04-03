using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DebtAnalyzer.Common;
using DebtAnalyzer.MethodDebt;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DebtAnalyzer.ClassDebt
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TypeDebtAnnotationProvider)), Shared]
	public class TypeDebtAnnotationProvider : CodeFixProvider
	{
		public const string Title = "Update technical debt annotation";

		public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(FieldCountAnalyzer.DiagnosticId, TypeLengthAnalyzer.DiagnosticId);

		public sealed override FixAllProvider GetFixAllProvider()
		{
			return new TypeDebtFixAllProvider();
		}

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			
			var diagnostic = context.Diagnostics.First();
			var typeSyntax = (TypeDeclarationSyntax)syntaxRoot.FindNode(context.Span);

			context.RegisterCodeFix(CodeAction.Create(Title, async c =>
			{
				var project = await AddDebtAnnotation(context.Document, typeSyntax, c);
				return project.Solution;
			}, typeSyntax.SpanStart + ""), diagnostic);
		}

		public static async Task<Project> AddDebtAnnotation(Document document, TypeDeclarationSyntax typeSyntax, CancellationToken cancellationToken)
		{
			var syntaxRoot = (CompilationUnitSyntax) await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			syntaxRoot = syntaxRoot.ReplaceNode(typeSyntax, GetNewType(typeSyntax));
			syntaxRoot = RoslynUtil.AddUsing(syntaxRoot);

			var newDocument = document.WithSyntaxRoot(syntaxRoot);
			return newDocument.Project;
		}

		public static TypeDeclarationSyntax GetNewType(TypeDeclarationSyntax typeSyntax)
		{
			var attributeSyntax = GetAttribute(typeSyntax);
			return (TypeDeclarationSyntax)new UpdateOrAddTypeDebtAttribute(attributeSyntax).Visit(typeSyntax);
		}

		public static AttributeSyntax GetAttribute(TypeDeclarationSyntax typeSyntax)
		{
			var attributeType = typeof (DebtType);
			var lineCountArgument = GetLineCountArgument(typeSyntax);
			var parameterCountArgument = GetParameterCountArgument(typeSyntax);
			var attributeArgumentSyntaxs = new List<AttributeArgumentSyntax> {lineCountArgument, parameterCountArgument};
			return SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeType.Name), SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(attributeArgumentSyntaxs)));
		}

		static AttributeArgumentSyntax GetParameterCountArgument(TypeDeclarationSyntax typeSyntax)
		{
			return RoslynUtil.GetNamedAttributeArgument(nameof(DebtType.FieldCount), FieldCountAnalyzer.GetFieldCount(typeSyntax));
		}

		static AttributeArgumentSyntax GetLineCountArgument(TypeDeclarationSyntax typeSyntax)
		{
			return RoslynUtil.GetNamedAttributeArgument(nameof(DebtType.LineCount), TypeLengthAnalyzer.GetTypeLength(typeSyntax));
		}
	}
}