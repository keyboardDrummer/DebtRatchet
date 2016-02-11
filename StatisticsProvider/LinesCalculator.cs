using System;
using DebtAnalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StatisticsProvider
{
	class FieldVisitor : CSharpSyntaxWalker
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

	class LinesCalculator : CSharpSyntaxWalker
	{
		readonly IAssemblySymbol assembly;
		readonly Statistics statistics = new Statistics();
		MethodStatistics MethodStatistics => statistics.MethodStatistics;
		TypeStatistics TypeStatistics => statistics.TypeStatistics;

		public int MaxLineCount { get; }
		public int MaxParameterCount { get; }

		public LinesCalculator(IAssemblySymbol assembly) : base(SyntaxWalkerDepth.Node)
		{
			this.assembly = assembly;
			MaxLineCount = MethodLengthAnalyzer.GetMaxLineCount(assembly);
			MaxParameterCount = MethodParameterCountAnalyzer.GetMaxParameterCount(assembly);
		}

		public override void VisitClassDeclaration(ClassDeclarationSyntax node)
		{
			var lineSpan = node.SyntaxTree.GetLineSpan(node.FullSpan);
			var classLineCount = MethodLengthAnalyzer.GetLineSpanLineCount(lineSpan);
			TypeStatistics.TotalLines += classLineCount;
			TypeStatistics.TotalClasses++;

			if (classLineCount > 500)
			{
				TypeStatistics.LinesInFatClasses += classLineCount;
				TypeStatistics.FatClasses++;
			}

			var fieldVisitor = new FieldVisitor();
			fieldVisitor.Visit(node);
			var fieldCount = fieldVisitor.FieldsFound;

			TypeStatistics.TotalFields += fieldCount;
			if (fieldCount > 10)
			{
				TypeStatistics.ClassesWithTooManyFields++;
			}
			base.VisitClassDeclaration(node);
		}

		public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
		{
			var length = MethodLengthAnalyzer.GetMethodLength(node);
			if (length > MaxLineCount)
			{
				MethodStatistics.LinesInFatMethods += length;
				MethodStatistics.FatMethodCount++;
			}

			var parameterCount = node.ParameterList.Parameters.Count;
			if (parameterCount > MaxParameterCount)
			{
				MethodStatistics.MethodsWithTooManyParameters++;
			}
			MethodStatistics.TotalParameters += parameterCount;

			MethodStatistics.TotalLines += length;
			MethodStatistics.MethodCount++;
			base.VisitMethodDeclaration(node);
		}

		public Statistics GetStatistics()
		{
			return statistics;
		}
	}
}