using DebtAnalyzer.Common;
using DebtAnalyzer.MethodDebt;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DebtAnalyzer.ClassDebt
{
	class UpdateOrAddTypeDebtAttribute : UpdateOrAddGenericDebtAttribute
	{
		public UpdateOrAddTypeDebtAttribute(AttributeSyntax newAttribute) : base(newAttribute)
		{
		}

		public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
		{
			return VisitDeclaration(node, n => (ClassDeclarationSyntax)base.VisitClassDeclaration(n), (n, a) => n.AddAttributeLists(a));
		}

		public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
		{
			return VisitDeclaration(node, n => (StructDeclarationSyntax)base.VisitStructDeclaration(n), (n, a) => n.AddAttributeLists(a));
		}

		public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
		{
			return VisitDeclaration(node, n => (InterfaceDeclarationSyntax)base.VisitInterfaceDeclaration(n), (n, a) => n.AddAttributeLists(a));
		}
	}
}