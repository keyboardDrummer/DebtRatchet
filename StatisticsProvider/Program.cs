using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StatisticsProvider
{
	[TestClass]
	public class Program
	{
		static void Main(string[] args)
		{
			Run(args).Wait();
			Console.Read();
		}

		static async Task Run(string[] args)
		{
			var solutionPath = args[0];
			Solution solution = await GetSolution(solutionPath);
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
			var projectStatistics = projectStatisticsTuples.ToDictionary(p => p.Item1, p => p.Item2);
			var solutionStatistics = projectStatistics.Select(p => p.Value).Aggregate((a, b) => a.Concat(b));
			Console.WriteLine("Solution statistics:" + Environment.NewLine + solutionStatistics.Print());
			foreach (var project in projectStatistics.Keys)
			{
				Console.WriteLine($"Project statistics for {project.Name}:\n" + 
					$"Max method length: {projectStatistics[project].MethodStatistics.FatLineCount}\n" + 
					projectStatistics[project].Print());
			}
		}

		private static async Task<Solution> GetSolution(string path)
		{
			using (var workspace = MSBuildWorkspace.Create())
			{
				return await workspace.OpenSolutionAsync(path);
			}
		}
	}
}
