using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DebtAnalyzer.DebtAnnotation
{
	class UpdateOrAddDebtAttribute : CSharpSyntaxRewriter
	{
		public UpdateOrAddDebtAttribute(AttributeSyntax newAttribute) : base(false)
		{
			NewAttribute = newAttribute;
		}

		public AttributeSyntax NewAttribute { get; private set; }

		public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
		{
			return VisitMethodBaseDeclaration(node, n => (MethodDeclarationSyntax)base.VisitMethodDeclaration(n), (n,a) => n.AddAttributeLists(a));
		}

		public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
		{
			return VisitMethodBaseDeclaration(node, n => (ConstructorDeclarationSyntax)base.VisitConstructorDeclaration(n), (n, a) => n.AddAttributeLists(a));
		}

		private SyntaxNode VisitMethodBaseDeclaration<T>(T node, Func<T, T> visitBase, Func<T, AttributeListSyntax, T> addAttributeLists)
			where T : BaseMethodDeclarationSyntax
		{
			T basee = visitBase(node);
			T withAttribute;
			if (NewAttribute == null)
			{
				withAttribute = basee;
			}
			else
			{
				var attributeListSyntax = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(NewAttribute));
				NewAttribute = null;
				withAttribute = addAttributeLists(basee.WithoutTrivia(), attributeListSyntax);
			}
			return withAttribute.WithTriviaFrom(node);
		}

		public override SyntaxNode VisitAttributeList(AttributeListSyntax node)
		{
			if (!node.Attributes.Any())
				return null;

			return base.VisitAttributeList(node);
		}

		public override SyntaxNode VisitAttribute(AttributeSyntax node)
		{
			if (node.Name.ToString() == nameof(DebtMethod))
			{
				if (NewAttribute == null) return null;
				var value = NewAttribute;
				NewAttribute = null;
				return value;
			}

			return base.VisitAttribute(node);
		}
	}
}