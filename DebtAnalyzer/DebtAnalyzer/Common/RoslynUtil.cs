using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DebtAnalyzer.Common
{
	class RoslynUtil
	{
		public static CompilationUnitSyntax AddUsing(CompilationUnitSyntax syntaxRoot)
		{
			var debtAnalyzerNamespace = SyntaxFactory.IdentifierName("DebtAnalyzer");
			var usingDirectiveSyntax = SyntaxFactory.UsingDirective(
				debtAnalyzerNamespace).WithUsingKeyword(SyntaxFactory.Token(SyntaxKind.UsingKeyword))
				.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

			return syntaxRoot.Usings.All(@using => (@using.Name as IdentifierNameSyntax)?.Identifier.ValueText != debtAnalyzerNamespace.Identifier.ValueText)
				? syntaxRoot.AddUsings(usingDirectiveSyntax)
				: syntaxRoot;
		}

		public static AttributeArgumentSyntax GetNamedAttributeArgument(string parameterName, object parameterValue)
		{
			return SyntaxFactory.AttributeArgument(
				SyntaxFactory.LiteralExpression(
					SyntaxKind.NumericLiteralExpression,
					SyntaxFactory.Literal(
						SyntaxFactory.TriviaList(),
						parameterValue.ToString(),
						parameterValue.ToString(),
						SyntaxFactory.TriviaList())))
				.WithNameEquals(
					SyntaxFactory.NameEquals(
						SyntaxFactory.IdentifierName(
							parameterName))
						.WithEqualsToken(
							SyntaxFactory.Token(
								SyntaxKind.EqualsToken)));
		}
	}
}
