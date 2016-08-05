using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StatisticsProvider
{
	// ReSharper disable LocalizableElement
	
	public class Program
	{
		const string ConfigurationArgument = "-c";

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
				ProvideStatistics(args[0], onlyNumbers, GetEmptyStatistics(args));
			}
		}

		static Statistics GetEmptyStatistics(string[] args)
		{
			if (!args.Contains(ConfigurationArgument)) return null;

			var defaults = args.SkipWhile(s => s != ConfigurationArgument).Skip(1).Take(4).Select(int.Parse).ToList();
			var methodStatistics = new MethodStatistics(defaults[0], defaults[1]);
			var typeStatistics = new TypeStatistics(defaults[2], defaults[3]);
			return new Statistics(typeStatistics, methodStatistics);
		}

		static async void ProvideStatistics(string solutionPath, bool onlyNumbers, Statistics statistics)
		{
			try
			{
				var solution = await GetSolution(solutionPath);
				var projectStatistics = await Statistics.GetProjectStatistics(solution, statistics);
				var solutionStatistics = projectStatistics.Select(p => p.Value).Aggregate((a, b) => a.Concat(b));
				Console.WriteLine("Solution statistics:" + Environment.NewLine + solutionStatistics.Print(onlyNumbers));
				foreach (var project in projectStatistics.Keys)
				{
					Console.WriteLine($"Project statistics for {project.Name}:\n" + projectStatistics[project].Print(onlyNumbers));
				}
			}
			catch (FileNotFoundException e)
			{
				Console.Write(e.Message);
			}
		}

		static async Task<Solution> GetSolution(string path)
		{
			using (var workspace = MSBuildWorkspace.Create())
			{
					return await workspace.OpenSolutionAsync(path);
			}
		}
	}
}
