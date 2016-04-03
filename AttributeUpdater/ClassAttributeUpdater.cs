using System.Linq;
using DebtAnalyzer;
using DebtAnalyzer.ClassDebt;
using DebtAnalyzer.DebtAnnotation;
using DebtAnalyzer.MethodDebt;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace AttributeUpdater
{
	class ClassAttributeUpdater : CSharpSyntaxRewriter
	{
		readonly int maxMethodLength;
		readonly int maxParameters;
		readonly int maxTypeLength;
		readonly int maxFieldCount;
		readonly Workspace workspace;

		public ClassAttributeUpdater(Workspace workspace, int maxParameters, int maxMethodLength, int maxTypeLength, int maxFieldCount) : base(false)
		{
			this.workspace = workspace;
			this.maxParameters = maxParameters;
			this.maxMethodLength = maxMethodLength;
			this.maxTypeLength = maxTypeLength;
			this.maxFieldCount = maxFieldCount;
		}

		public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
		{
			return base.VisitClassDeclaration(node).WithLeadingTrivia(node.GetLeadingTrivia());
		}

		public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
		{
			return base.VisitInterfaceDeclaration(node).WithLeadingTrivia(node.GetLeadingTrivia());
		}

		public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
		{
			return base.VisitStructDeclaration(node).WithLeadingTrivia(node.GetLeadingTrivia());
		}

		public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
		{
			return base.VisitConstructorDeclaration(node).WithLeadingTrivia(node.GetLeadingTrivia());
		}

		public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
		{
			return base.VisitMethodDeclaration(node).WithLeadingTrivia(node.GetLeadingTrivia());
		}

		public override SyntaxNode VisitAttributeList(AttributeListSyntax node)
		{
			var result = (AttributeListSyntax) base.VisitAttributeList(node);
			return result.Attributes.Any() ? result : null;
		}

		public override SyntaxNode VisitAttribute(AttributeSyntax node)
		{
			if (node.Name.ToString() == nameof(DebtMethod))
			{
				var containingMethod = node.Ancestors().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault();
				if (containingMethod == null)
				{
					return node;
				}

				if (containingMethod.ParameterList.Parameters.Count <= maxParameters && MethodLengthAnalyzer.GetMethodLength(containingMethod) < maxMethodLength)
				{
					return null;
				}
				return Formatter.Format(MethodDebtAnnotationProvider.GetAttribute(containingMethod), workspace);
			}

			if (node.Name.ToString() == nameof(DebtType))
			{
				var containingType = node.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
				if (containingType == null)
				{
					return node;
				}
				if (TypeLengthAnalyzer.GetTypeLength(containingType) < maxTypeLength && 
					FieldCountAnalyzer.GetFieldCount(containingType) < maxFieldCount)
				{
					return null;
				}
				return Formatter.Format(TypeDebtAnnotationProvider.GetAttribute(containingType), workspace);
			}
			return base.VisitAttribute(node);
		}
	}
}