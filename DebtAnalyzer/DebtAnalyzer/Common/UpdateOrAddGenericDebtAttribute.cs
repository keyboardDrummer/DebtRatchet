using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DebtAnalyzer.Common
{
	class UpdateOrAddGenericDebtAttribute : CSharpSyntaxRewriter
	{
		public UpdateOrAddGenericDebtAttribute(AttributeSyntax newAttribute) : base(false)
		{
			NewAttribute = newAttribute;
		}

		public AttributeSyntax NewAttribute { get; private set; }

		protected SyntaxNode VisitDeclaration<T>(T node, Func<T, T> visitBase, Func<T, AttributeListSyntax, T> addAttributeLists)
			where T : SyntaxNode
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