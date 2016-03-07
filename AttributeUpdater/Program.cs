using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace ÀttributeUpdater
{
	class Program
	{
		static void Main(string[] args)
		{
			var solutionPath = args[0];
			UpdateAnnotations(solutionPath).Wait();
		}

		static async Task UpdateAnnotations(string solutionPath)
		{
			var solution = await GetSolution(solutionPath);
			await MissingAttributeAdder.AddMissingAttributes(solution);
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
