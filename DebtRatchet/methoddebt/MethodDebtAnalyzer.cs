using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DebtRatchet.MethodDebt
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class MethodDebtAnalyzer : DiagnosticAnalyzer
	{
		readonly MethodLengthAnalyzer lengthAnalyzer = new MethodLengthAnalyzer();
		readonly MethodParameterCountAnalyzer parameterCountAnalyzer = new MethodParameterCountAnalyzer();

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterCompilationStartAction(startContext =>
			{
				startContext.RegisterSyntaxNodeAction(nodeContext => lengthAnalyzer.AnalyzeSyntax(nodeContext), SyntaxKind.MethodDeclaration, SyntaxKind.ConstructorDeclaration);
				startContext.RegisterSyntaxNodeAction(nodeContext => parameterCountAnalyzer.AnalyzeSymbol(nodeContext), SyntaxKind.MethodDeclaration, SyntaxKind.ConstructorDeclaration);
			});
		}

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(lengthAnalyzer.CreateDiagnosticDescriptor(DiagnosticSeverity.Warning),
			parameterCountAnalyzer.CreateDiagnosticDescriptor(DiagnosticSeverity.Warning));

		public static IEnumerable<DebtMethod> GetDebtMethods(ImmutableArray<AttributeData> attributeDatas)
		{
			return attributeDatas.Where(data => data.AttributeClass.Name == typeof(DebtMethod).Name).Select(ToDebtMethod);
		}

		static DebtMethod ToDebtMethod(AttributeData data)
		{
			var namedArguments = data.NamedArguments.ToDictionary(kv => kv.Key, kv => kv.Value);
			var result = new DebtMethod();
			if (namedArguments.ContainsKey(LineCountName))
				result.LineCount = namedArguments[LineCountName].Value as int? ?? 0;
			if (namedArguments.ContainsKey(ParameterCountName))
				result.ParameterCount = namedArguments[ParameterCountName].Value as int? ?? 0;
			return result;
		}

		const string LineCountName = nameof(DebtMethod.LineCount);
		const string ParameterCountName = nameof(DebtMethod.ParameterCount);
	}
}