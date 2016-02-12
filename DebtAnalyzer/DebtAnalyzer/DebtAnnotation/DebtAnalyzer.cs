using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DebtAnalyzer.Attributes;
using DebtAnalyzer.MethodLength;
using DebtAnalyzer.ParameterCount;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DebtAnalyzer.DebtAnnotation
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class DebtAnalyzer : DiagnosticAnalyzer
	{
		readonly MethodLengthAnalyzer lengthAnalyzer = new MethodLengthAnalyzer();
		readonly MethodParameterCountAnalyzer parameterCountAnalyzer = new MethodParameterCountAnalyzer();

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterCompilationStartAction(startContext =>
			{
				var attributeDatas = startContext.Compilation.Assembly.GetAttributes();
				var names = GetDebtMethods(attributeDatas).Where(debtMethod => debtMethod.Target != null).GroupBy(debtMethod => debtMethod.Target).
					ToDictionary(targetGroup => targetGroup.First().Target, x => x.First());
				startContext.RegisterSyntaxNodeAction(nodeContext => lengthAnalyzer.AnalyzeSyntax(nodeContext, names), SyntaxKind.MethodDeclaration);
				startContext.RegisterSymbolAction(nodeContext => parameterCountAnalyzer.AnalyzeSymbol(nodeContext, names), SymbolKind.Method);
			});
		}

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(lengthAnalyzer.CreateDiagnosticDescriptor(DiagnosticSeverity.Warning),
			parameterCountAnalyzer.CreateDiagnosticDescriptor(DiagnosticSeverity.Warning));

		public static IEnumerable<DebtMethod> GetDebtMethods(ImmutableArray<AttributeData> attributeDatas)
		{
			return attributeDatas.Where(data => data.AttributeClass.Name == typeof(DebtMethod).Name).Select(ToDebtMethod);
		}
		
		public static string GetFullName(IMethodSymbol methodSymbol)
		{
			string parametersString = $"{string.Join(",", methodSymbol.Parameters.Select(parameter => parameter.Type.ContainingNamespace + "." + parameter.Type.Name))}";
			return $"{methodSymbol.ContainingNamespace.Name}.{methodSymbol.ContainingType.Name}.{methodSymbol.Name}({parametersString})";
		}

		static DebtMethod ToDebtMethod(AttributeData data)
		{
			var namedArguments = data.NamedArguments.ToDictionary(kv => kv.Key, kv => kv.Value);
			var result = new DebtMethod();
			if (namedArguments.ContainsKey(LineCountName))
				result.LineCount = (namedArguments[LineCountName].Value as int?) ?? 0;
			if (namedArguments.ContainsKey(ParameterCountName))
				result.ParameterCount = (namedArguments[ParameterCountName].Value as int?) ?? 0;
			if (namedArguments.ContainsKey(TargetName))
				result.Target = namedArguments[TargetName].Value as string;
			return result;
		}

		const string LineCountName = nameof(DebtMethod.LineCount);
		const string TargetName = nameof(DebtMethod.Target);
		const string ParameterCountName = nameof(DebtMethod.ParameterCount);
	}
}