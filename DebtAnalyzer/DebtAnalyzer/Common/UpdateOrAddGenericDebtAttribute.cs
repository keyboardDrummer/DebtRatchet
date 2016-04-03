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

		protected SyntaxNode VisitDeclaration<T>(T node, Func<T, SyntaxList<AttributeListSyntax>> getAttributeLists, Func<T, SyntaxList<AttributeListSyntax>, T> withAttributeLists)
			where T : SyntaxNode
		{
			var newAttributeLists = VisitList(getAttributeLists(node));
			T withAttribute;
			if (NewAttribute == null)
			{
				withAttribute = withAttributeLists(node, newAttributeLists);
			}
			else
			{
				newAttributeLists = newAttributeLists.Add(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(NewAttribute)));
				NewAttribute = null;
				withAttribute = withAttributeLists(node.WithoutTrivia(), newAttributeLists);
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
			if (Equals(node.Name.ToString(), NewAttribute?.Name.ToString()))
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