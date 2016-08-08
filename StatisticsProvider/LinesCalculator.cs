using DebtRatchet.ClassDebt;
using DebtRatchet.MethodDebt;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StatisticsProvider
{
	class LinesCalculator : CSharpSyntaxWalker
	{
		readonly IAssemblySymbol assembly;
		readonly Statistics statistics;
		MethodStatistics MethodStatistics => statistics.MethodStatistics;
		TypeStatistics TypeStatistics => statistics.TypeStatistics;

		public LinesCalculator(IAssemblySymbol assembly) : base(SyntaxWalkerDepth.Node)
		{
			this.assembly = assembly;
			var methodStatistics = new MethodStatistics(MethodLengthAnalyzer.GetMaxLineCount(assembly), MethodParameterCountAnalyzer.GetMaxParameterCount(assembly));
			var typeStatistics = new TypeStatistics(1000, 8);
			statistics = new Statistics(typeStatistics, methodStatistics);
		}

		public LinesCalculator(Statistics statistics) : base(SyntaxWalkerDepth.Node)
		{
			this.statistics = statistics;
		}

		public override void VisitClassDeclaration(ClassDeclarationSyntax node)
		{
			var classLineCount = TypeLengthAnalyzer.GetTypeLength(node);
			var fieldCount = FieldCountAnalyzer.GetFieldCount(node);

			TypeStatistics.FoundClass(node.Identifier.ToString(), classLineCount, fieldCount);
			base.VisitClassDeclaration(node);
		}

		public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
		{
			var length = MethodLengthAnalyzer.GetMethodLength(node);
			var parameterCount = node.ParameterList.Parameters.Count;
			MethodStatistics.FoundMethod(length, parameterCount);
			base.VisitMethodDeclaration(node);
		}

		public Statistics GetStatistics()
		{
			return statistics;
		}
	}
}