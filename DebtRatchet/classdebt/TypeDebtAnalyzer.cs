using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DebtAnalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DebtRatchet.ClassDebt
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class TypeDebtAnalyzer : DiagnosticAnalyzer
	{
		readonly TypeLengthAnalyzer lengthAnalyzer = new TypeLengthAnalyzer();
		readonly FieldCountAnalyzer parameterCountAnalyzer = new FieldCountAnalyzer();

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterCompilationStartAction(startContext =>
			{
				var syntaxes = new List<SyntaxKind> {SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration, SyntaxKind.InterfaceDeclaration}.ToImmutableArray();
				startContext.RegisterSyntaxNodeAction(nodeContext => lengthAnalyzer.AnalyzeSyntax(nodeContext), syntaxes);
				startContext.RegisterSyntaxNodeAction(nodeContext => parameterCountAnalyzer.AnalyzeSyntax(nodeContext), syntaxes);
			});
		}

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(lengthAnalyzer.CreateDiagnosticDescriptor(DiagnosticSeverity.Warning),
			parameterCountAnalyzer.CreateDiagnosticDescriptor(DiagnosticSeverity.Warning));

		public static IEnumerable<DebtType> GetDebtAnnotations(ImmutableArray<AttributeData> attributeDatas)
		{
			return attributeDatas.Where(data => data.AttributeClass.Name == typeof(DebtType).Name).Select(ToDebtType);
		}

		static DebtType ToDebtType(AttributeData data)
		{
			var namedArguments = data.NamedArguments.ToDictionary(kv => kv.Key, kv => kv.Value);
			var result = new DebtType();
			if (namedArguments.ContainsKey(LineCountName))
				result.LineCount = (namedArguments[LineCountName].Value as int?) ?? 0;
			if (namedArguments.ContainsKey(FieldCountName))
				result.FieldCount = (namedArguments[FieldCountName].Value as int?) ?? 0;
			return result;
		}

		const string LineCountName = nameof(DebtType.LineCount);
		const string FieldCountName = nameof(DebtType.FieldCount);
	}
}