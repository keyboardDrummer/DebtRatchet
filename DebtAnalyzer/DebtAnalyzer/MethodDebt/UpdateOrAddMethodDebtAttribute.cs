using DebtAnalyzer.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DebtAnalyzer.MethodDebt
{
	class UpdateOrAddMethodDebtAttribute : UpdateOrAddGenericDebtAttribute
	{
		public UpdateOrAddMethodDebtAttribute(AttributeSyntax newAttribute) : base(newAttribute)
		{
		}

		public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
		{
			return VisitDeclaration(node, n => n.AttributeLists, (n,a) => n.WithAttributeLists(a));
		}

		public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
		{
			return VisitDeclaration(node, n => n.AttributeLists, (n, a) => n.WithAttributeLists(a));
		}
	}
}