using System;
using DebtAnalyzer;
using DebtAnalyzer.MethodLength;
using DebtAnalyzer.ParameterCount;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StatisticsProvider
{
	class LinesCalculator : CSharpSyntaxWalker
	{
		readonly IAssemblySymbol assembly;
		readonly Statistics statistics = new Statistics();
		MethodStatistics MethodStatistics => statistics.MethodStatistics;
		TypeStatistics TypeStatistics => statistics.TypeStatistics;

		public LinesCalculator(IAssemblySymbol assembly) : base(SyntaxWalkerDepth.Node)
		{
			this.assembly = assembly;
			MethodStatistics.FatLineCount = MethodLengthAnalyzer.GetMaxLineCount(assembly);
			MethodStatistics.MaxParameterCount = MethodParameterCountAnalyzer.GetMaxParameterCount(assembly);

			TypeStatistics.FatClassBoundary = 1000;
			TypeStatistics.TooManyFieldsBoundary = 8;
		}

		public override void VisitClassDeclaration(ClassDeclarationSyntax node)
		{
			var lineSpan = node.SyntaxTree.GetLineSpan(node.FullSpan);
			var classLineCount = MethodLengthAnalyzer.GetLineSpanLineCount(lineSpan);
			var typeStatistics = TypeStatistics;

			var fieldVisitor = new FieldVisitor();
			fieldVisitor.Visit(node);
			var fieldCount = fieldVisitor.FieldsFound;

			typeStatistics.FoundClass(node.Identifier.ToString(), classLineCount, fieldCount);
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