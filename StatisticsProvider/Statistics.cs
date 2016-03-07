using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace StatisticsProvider
{
	public class Statistics
	{
		public static async Task<Dictionary<Project, Statistics>> GetProjectStatistics(Solution solution)
		{
			var projectStatisticsTuples = await Task.WhenAll(solution.Projects.Select(async project =>
			{
				var compilation = await project.GetCompilationAsync();
				var calculator = new LinesCalculator(compilation.Assembly);
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

		public Statistics() : this(new TypeStatistics(), new MethodStatistics())
		{

		}

		public TypeStatistics TypeStatistics { get; }

		public MethodStatistics MethodStatistics { get; }

		public Statistics Concat(Statistics other)
		{
			return new Statistics(TypeStatistics.Concat(other.TypeStatistics), MethodStatistics.Concat(other.MethodStatistics));
		}

		public string Print()
		{
			return TypeStatistics.Print() + "\n" + MethodStatistics.Print();
		}
	}
}