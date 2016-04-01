using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.MSBuild;

namespace AttributeUpdater
{
	class Program
	{
		static void Main(string[] args)
		{
			var solutionPath = args[0];
			var addAnnotations = args.Contains("-a");
			UpdateAnnotations(solutionPath, addAnnotations, true).Wait();
		}

		static async Task<bool> UpdateAnnotations(string solutionPath, bool addAnnotations, bool updateAttributes)
		{
			using (var workspace = MSBuildWorkspace.Create())
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
				return workspace.TryApplyChanges(solution);
			}
		}
	}
}
