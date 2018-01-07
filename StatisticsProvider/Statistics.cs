using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace StatisticsProvider
{
	public class Statistics
	{
		public static async Task<Dictionary<Project, Statistics>> GetProjectStatistics(Solution solution, Statistics emptyStatistics = null)
		{
			var projectStatisticsTuples = await Task.WhenAll(solution.Projects.Select(async project =>
			{
				var compilation = await project.GetCompilationAsync();
				var calculator = emptyStatistics == null ? new LinesCalculator(compilation.Assembly) : new LinesCalculator(emptyStatistics);
				await Task.WhenAll(project.Documents.Select(async document =>
				{
					var root = await document.GetSyntaxRootAsync();
					calculator.Visit(root);
				}));
				return Tuple.Create(project, calculator.GetStatistics());
			}));
			return projectStatisticsTuples.ToDictionary(p => p.Item1, p => p.Item2);
		}

		public Statistics(TypeStatistics typeStatistics, MethodStatistics methodStatistics)
		{
			TypeStatistics = typeStatistics;
			MethodStatistics = methodStatistics;
		}
		
		public TypeStatistics TypeStatistics { get; }

		public MethodStatistics MethodStatistics { get; }

		public Statistics Concat(Statistics other)
		{
			return new Statistics(TypeStatistics.Concat(other.TypeStatistics), MethodStatistics.Concat(other.MethodStatistics));
		}

		public string Print(bool onlyNumbers)
		{
			return TypeStatistics.Print(onlyNumbers) + "\n" + MethodStatistics.Print(onlyNumbers);
		}
	}
}