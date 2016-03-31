using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DebtAnalyzer.Common;
using DebtAnalyzer.MethodDebt;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DebtAnalyzer.ClassDebt
{
	class TypeDebtFixAllProvider : GenericDebtFixAllProvider
	{
		protected override SyntaxNode FixRootFromDiagnostics(IEnumerable<Diagnostic> diagnostics, CompilationUnitSyntax root)
		{
			return new TypeAnnotator(diagnostics).Visit(root);
		}

		class TypeAnnotator : CSharpSyntaxRewriter
		{
			readonly ImmutableHashSet<int> spans;

			public TypeAnnotator(IEnumerable<Diagnostic> diagnostics)
			{
				spans = diagnostics.Select(diagnostic => diagnostic.Location.GetLineSpan().StartLinePosition.Line).ToImmutableHashSet();
			}

			public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
			{
				return VisitTypeDeclaration(node);
			}

			public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
			{
				return VisitTypeDeclaration(node);
			}

			public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
			{
				return VisitTypeDeclaration(node);
			}

			SyntaxNode VisitTypeDeclaration(TypeDeclarationSyntax node)
			{
				if (spans.Contains(node.Identifier.GetLocation().GetLineSpan().StartLinePosition.Line))
				{
					return TypeDebtAnnotationProvider.GetNewType(node);
				}
				return node;
			}
		}
	}
}