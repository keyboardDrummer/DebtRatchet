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
			ProvideStatistics(args[0]).Wait();
		}

		static async Task ProvideStatistics(string solutionPath)
		{
			Solution solution = await GetSolution(solutionPath);
			var projectStatistics = await Statistics.GetProjectStatistics(solution);
			var solutionStatistics = projectStatistics.Select(p => p.Value).Aggregate((a, b) => a.Concat(b));
			Console.WriteLine("Solution statistics:" + Environment.NewLine + solutionStatistics.Print());
			foreach (var project in projectStatistics.Keys)
			{
				Console.WriteLine($"Project statistics for {project.Name}:\n" + projectStatistics[project].Print());
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
