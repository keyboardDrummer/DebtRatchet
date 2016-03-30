using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DebtAnalyzer.ClassDebt
{
	public class FieldVisitor : CSharpSyntaxWalker
	{
		public int FieldsFound { get; private set; }

		public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
		{
			FieldsFound++;
			base.VisitFieldDeclaration(node);
		}

		public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
		{
			if (node.AccessorList == null || !node.AccessorList.Accessors.Any())
				return;

			if (node.AccessorList.Accessors.First().Body == null)
				FieldsFound++;

			base.VisitPropertyDeclaration(node);
		}
	}
}