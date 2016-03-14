using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StatisticsProvider
{
	// ReSharper disable LocalizableElement
	[TestClass]
	public class Program
	{
		static void Main(string[] args)
		{
			if (!args.Any())
			{
				Console.WriteLine("First argument should be a path to the solution file.");
				Console.WriteLine("Add -n after the first argument to output only the raw numbers without descriptions.");
			}
			else
			{
				var onlyNumbers = args.Contains("-n");
				ProvideStatistics(args[0], onlyNumbers).Wait();
			}
		}

		static async Task ProvideStatistics(string solutionPath, bool onlyNumbers)
		{
			Solution solution = await GetSolution(solutionPath);
			var projectStatistics = await Statistics.GetProjectStatistics(solution);
			var solutionStatistics = projectStatistics.Select(p => p.Value).Aggregate((a, b) => a.Concat(b));
			Console.WriteLine("Solution statistics:" + Environment.NewLine + solutionStatistics.Print(onlyNumbers));
			foreach (var project in projectStatistics.Keys)
			{
				Console.WriteLine($"Project statistics for {project.Name}:\n" + projectStatistics[project].Print(onlyNumbers));
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
