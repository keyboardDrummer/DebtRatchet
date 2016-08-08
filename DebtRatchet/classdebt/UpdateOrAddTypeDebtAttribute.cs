using DebtRatchet.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DebtRatchet.ClassDebt
{
	class UpdateOrAddTypeDebtAttribute : UpdateOrAddGenericDebtAttribute
	{
		public UpdateOrAddTypeDebtAttribute(AttributeSyntax newAttribute) : base(newAttribute)
		{
		}

		public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
		{
			return VisitDeclaration(node, n => n.AttributeLists, (n, a) => n.WithAttributeLists(a));
		}

		public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
		{
			return VisitDeclaration(node, n => node.AttributeLists, (n, a) => n.WithAttributeLists(a));
		}

		public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
		{
			return VisitDeclaration(node, n => node.AttributeLists, (n, a) => n.WithAttributeLists(a));
		}
	}
}