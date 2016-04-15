using System.Linq;
using DebtAnalyzer.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DebtAnalyzer.ClassDebt
{
	public class FieldCountAnalyzer
	{
		public const string DiagnosticId = "MaxFieldCount";
		public static int DefaultMaximumFieldCount = 50;

		public void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
		{
			var type = (TypeDeclarationSyntax)context.Node;
			var typeSymbol = context.SemanticModel.GetDeclaredSymbol(type);
			if (RoslynUtil.IsTypeGenerated(typeSymbol))
			{
				return;
			}
			var maxFieldCount = GetMaxFieldCount(typeSymbol.ContainingAssembly);
			var previousFieldCount = GetPreviousFieldCount(typeSymbol);
			var fieldCount = GetFieldCount(type);
			if (fieldCount > previousFieldCount && fieldCount > maxFieldCount)
			{
				var severity = DebtAsErrorUtil.GetDiagnosticSeverity(typeSymbol);
				var descriptor = CreateDiagnosticDescriptor(severity);
				var diagnostic = Diagnostic.Create(descriptor, type.Identifier.GetLocation(), typeSymbol.Name, fieldCount, maxFieldCount);

				context.ReportDiagnostic(diagnostic);
			}
		}

		public static int GetFieldCount(TypeDeclarationSyntax type)
		{
			var fieldVisitor = new FieldVisitor();
			fieldVisitor.Visit(type);
			return fieldVisitor.FieldsFound;
		}

		public DiagnosticDescriptor CreateDiagnosticDescriptor(DiagnosticSeverity severity)
		{
			return new DiagnosticDescriptor(DiagnosticId, "Type has too many fields", 
				"Type {0} has {1} fields it should have more than {2}.", "Debt", severity, true);
		}

		static int GetPreviousFieldCount(INamedTypeSymbol typeSymbol)
		{
			return TypeDebtAnalyzer.GetDebtAnnotations(typeSymbol.GetAttributes()).FirstOrDefault()?.FieldCount ?? 0;
		}

		public static int GetMaxFieldCount(IAssemblySymbol assembly)
		{
			return assembly.GetAttributes().Where(data => data.AttributeClass.Name == typeof (MaxFieldCount).Name && data.ConstructorArguments.Length > 0).
				Select(data => data.ConstructorArguments[0].Value as int?).FirstOrDefault() ?? DefaultMaximumFieldCount;
		}
	}
}
