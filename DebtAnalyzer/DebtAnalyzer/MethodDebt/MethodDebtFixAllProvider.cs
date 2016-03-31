using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DebtAnalyzer.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DebtAnalyzer.MethodDebt
{
	class MethodDebtFixAllProvider : GenericDebtFixAllProvider
	{
		protected override SyntaxNode FixRootFromDiagnostics(IEnumerable<Diagnostic> diagnostics, CompilationUnitSyntax root)
		{
			return new AnnotateMethods(diagnostics).Visit(root);
		}

		class AnnotateMethods : CSharpSyntaxRewriter
		{
			readonly ImmutableHashSet<int> spans;

			public AnnotateMethods(IEnumerable<Diagnostic> diagnostics) : base(false)
			{
				spans = diagnostics.Select(diagnostic => diagnostic.Location.GetLineSpan().StartLinePosition.Line).ToImmutableHashSet();
			}

			public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
			{
				return VisitMethodBase(node.Identifier, node);
			}

			public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
			{
				return VisitMethodBase(node.Identifier, node);
			}

			SyntaxNode VisitMethodBase(SyntaxToken identifier, BaseMethodDeclarationSyntax node)
			{
				if (spans.Contains(identifier.GetLocation().GetLineSpan().StartLinePosition.Line))
				{
					return MethodDebtAnnotationProvider.GetNewMethod(node);
				}
				return node;
			}
		}

	}
}