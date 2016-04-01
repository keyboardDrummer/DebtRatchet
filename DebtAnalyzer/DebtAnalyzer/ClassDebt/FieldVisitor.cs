using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DebtAnalyzer.ClassDebt
{
	public class FieldVisitor : CSharpSyntaxWalker
	{
		public int FieldsFound { get; private set; }

		public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
		{
			var modifiers = node.Modifiers;
			if (IsInstanceField(modifiers))
			{
				FieldsFound++;
			}

			base.VisitFieldDeclaration(node);
		}

		static readonly ISet<SyntaxKind> forbidden = new HashSet<SyntaxKind> {SyntaxKind.StaticKeyword, SyntaxKind.ConstKeyword};
		static bool IsInstanceField(SyntaxTokenList modifiers)
		{
			return modifiers.All(token => !forbidden.Contains(token.Kind()));
		}

		public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
		{
			if (node.AccessorList == null || !node.AccessorList.Accessors.Any())
				return;

			if (node.AccessorList.Accessors.First().Body == null && IsInstanceField(node.Modifiers))
				FieldsFound++;

			base.VisitPropertyDeclaration(node);
		}
	}
}