using System.Linq;
using DebtAnalyzer.Common;
using DebtAnalyzer.MethodDebt;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DebtAnalyzer.ClassDebt
{
	public class TypeLengthAnalyzer
	{
		public const string DiagnosticId = "TypeLengthAnalyzer";

		public static int DefaultMaximumTypeLength = 1000;

		public void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
		{
			var type = (TypeDeclarationSyntax)context.Node;
			var typeSymbol = context.SemanticModel.GetDeclaredSymbol(type);
			var typeLength = GetTypeLength(type);
			var maxLineCount = GetMaxLineCount(typeSymbol.ContainingAssembly);
			var previousTypeLength = GetPreviousTypeLength(typeSymbol);
			if (typeLength > previousTypeLength && typeLength > maxLineCount)
			{
				var severity = DebtAsErrorUtil.GetDiagnosticSeverity(typeSymbol);
				var diagnosticDescriptor = CreateDiagnosticDescriptor(severity);
				var diagnostic = Diagnostic.Create(diagnosticDescriptor, type.GetLocation(), type.Identifier.Text, typeLength, maxLineCount);

				context.ReportDiagnostic(diagnostic);
			}
		}

		public DiagnosticDescriptor CreateDiagnosticDescriptor(DiagnosticSeverity severity)
		{
			return new DiagnosticDescriptor(DiagnosticId, "Type is too long.",
				"Type {0} is {1} lines long while it should not be longer than {2} lines.", "Debt", severity, true);
		}

		public static int GetTypeLength(TypeDeclarationSyntax type)
		{
			SyntaxTree tree = type.SyntaxTree;
			var lineSpan = tree.GetLineSpan(type.Span);
			return MethodLengthAnalyzer.GetLineSpanLineCount(lineSpan);
		}

		static int GetPreviousTypeLength(INamedTypeSymbol typeSymbol)
		{
			return TypeDebtAnalyzer.GetDebtAnnotations(typeSymbol.GetAttributes()).FirstOrDefault()?.LineCount ?? 0;
		}

		public static int GetMaxLineCount(IAssemblySymbol assembly)
		{
			return assembly.GetAttributes().Where(data => data.AttributeClass.Name == typeof(MaxTypeLength).Name && data.ConstructorArguments.Length == 1).
				Select(data => data.ConstructorArguments[0].Value as int?).FirstOrDefault() ?? DefaultMaximumTypeLength;
		}
	}
}