using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace AttributeUpdater
{
	class Program
	{
		static void Main(string[] args)
		{
			var solutionPath = args[0];
			UpdateAnnotations(solutionPath).Wait();
		}

		static async Task<bool> UpdateAnnotations(string solutionPath)
		{
			using (var workspace = MSBuildWorkspace.Create())
			{
				var solution = await workspace.OpenSolutionAsync(solutionPath);
				var newSolution = await SolutionAttributeUpdater.UpdateAttributes(solution);
				return workspace.TryApplyChanges(newSolution);
			}
		}
	}
}
