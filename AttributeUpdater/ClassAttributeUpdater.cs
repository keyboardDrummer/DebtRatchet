using System.Linq;
using DebtAnalyzer;
using DebtAnalyzer.DebtAnnotation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace ÀttributeUpdater
{
	class ClassAttributeUpdater : CSharpSyntaxRewriter
	{
		readonly Workspace workspace;

		public ClassAttributeUpdater(Workspace workspace) : base(false)
		{
			this.workspace = workspace;
		}

		public override SyntaxNode VisitAttribute(AttributeSyntax node)
		{
			if (node.Name.ToString() == nameof(DebtMethod))
			{
				var containingMethod = node.Ancestors().OfType<BaseMethodDeclarationSyntax>().First();
				return Formatter.Format(TechnicalDebtAnnotationProvider.GetAttribute(containingMethod), workspace); //TODO zorgen dat als het attribuut niet meer nodig is deze null returned.
			}
			return base.VisitAttribute(node);
		}
	}
}