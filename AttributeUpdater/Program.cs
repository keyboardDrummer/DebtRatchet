using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.MSBuild;

namespace AttributeUpdater
{
	class Program
	{
		static void Main(string[] args)
		{
			if (!args.Any())
			{
				Console.WriteLine("Run this tool on your solution to update your debt annotations.");
				Console.WriteLine("Existing annotations will be narrowed when possible.");
				Console.WriteLine("The first argument should be a path to the solution file.");
				Console.WriteLine("Add -a after the first argument to add missing annotations.");
			}
			else
			{
				var solutionPath = args[0];
				var addAnnotations = args.Contains("-a");
                UpdateAnnotations(solutionPath, addAnnotations, true).GetAwaiter().GetResult();
                Console.WriteLine("Updated annotations.");
            }
		}

		static async Task UpdateAnnotations(string solutionPath, bool addAnnotations, bool updateAttributes)
		{
			using (var workspace = MSBuildWorkspace.Create())
			{
				try
				{
					var solution = await workspace.OpenSolutionAsync(solutionPath);
					if (addAnnotations)
					{
						solution = await MissingAttributeAdder.AddMissingAttributes(solution);
					}
					if (updateAttributes)
					{
						solution = await SolutionAttributeUpdater.UpdateAttributes(solution);
					}
					workspace.TryApplyChanges(solution);
				}
				catch (FileNotFoundException e)
				{
					Console.Write(e.Message);
				}
			}
		}
	}
}
