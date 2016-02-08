using System;
using DebtAnalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StatisticsProvider
{
	class LinesCalculator : CSharpSyntaxWalker
	{
		readonly IAssemblySymbol assembly;
		int totalLines;
		int linesInFatMethods;
		int methodCount;
		int fatMethodCount;

		public int MaxLineCount { get; }

		public LinesCalculator(IAssemblySymbol assembly) : base(SyntaxWalkerDepth.Node)
		{
			this.assembly = assembly;
			MaxLineCount = MethodLengthAnalyzer.GetMaxLineCount(assembly);
		}

		public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
		{
			var length = MethodLengthAnalyzer.GetMethodLength(node);
			if (length > MaxLineCount)
			{
				linesInFatMethods += length;
				fatMethodCount++;
			}
			totalLines += length;
			methodCount++;
			base.VisitMethodDeclaration(node);
		}

		public Statistics GetStatistics()
		{
			return new Statistics(MaxLineCount, totalLines, linesInFatMethods, methodCount, fatMethodCount);
		}
	}
}