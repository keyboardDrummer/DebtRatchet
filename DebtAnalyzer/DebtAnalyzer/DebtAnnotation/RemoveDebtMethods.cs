using DebtAnalyzer.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DebtAnalyzer.DebtAnnotation
{
	class RemoveDebtMethods : CSharpSyntaxRewriter
	{
		public RemoveDebtMethods() : base(false)
		{
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
				return null;

			return base.VisitAttribute(node);
		}
	}
}