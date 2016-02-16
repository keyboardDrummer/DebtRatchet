using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DebtAnalyzer.DebtAnnotation
{
	class RemoveDebtMethods : CSharpSyntaxRewriter
	{
		public RemoveDebtMethods(AttributeSyntax newAttribute) : base(false)
		{
			NewAttribute = newAttribute;
		}

		public AttributeSyntax NewAttribute { get; private set; }

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