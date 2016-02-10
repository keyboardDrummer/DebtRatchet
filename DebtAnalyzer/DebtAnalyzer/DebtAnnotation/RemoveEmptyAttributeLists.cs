using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DebtAnalyzer.DebtAnnotation
{
	class RemoveEmptyAttributeLists : CSharpSyntaxRewriter
	{
		public RemoveEmptyAttributeLists() : base(false)
		{
		}

		public override SyntaxNode VisitAttributeList(AttributeListSyntax node)
		{
			if (!node.Attributes.Any())
				return null;

			return base.VisitAttributeList(node);
		}
	}
}