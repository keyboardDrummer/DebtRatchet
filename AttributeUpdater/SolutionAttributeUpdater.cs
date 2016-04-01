using System.Linq;
using System.Threading.Tasks;
using DebtAnalyzer.MethodDebt;
using DebtAnalyzer.ParameterCount;
using Microsoft.CodeAnalysis;

namespace AttributeUpdater
{
	public static class SolutionAttributeUpdater
	{
		public static async Task<Solution> UpdateAttributes(Solution solution)
		{
			var result = solution;
			foreach (var projectId in solution.ProjectIds)
			{
				var project = result.GetProject(projectId);
				var documentIds = project.DocumentIds.ToList();
				var compilation = await project.GetCompilationAsync();
				var maxParameters = MethodParameterCountAnalyzer.GetMaxParameterCount(compilation.Assembly);
				var maxMethodLength = MethodLengthAnalyzer.GetMaxLineCount(compilation.Assembly);
				foreach (var documentId in documentIds)
				{
					var document = project.GetDocument(documentId);
					var newDocument = await UpdateAttributes(document, maxParameters, maxMethodLength);
					project = newDocument.Project;
				}
				result = project.Solution;
			}
			return result;
		}

		static async Task<Document> UpdateAttributes(Document document, int maxParameters, int maxMethodLength)
		{
			var root = await document.GetSyntaxRootAsync();
			var newRoot = new ClassAttributeUpdater(document.Project.Solution.Workspace, maxParameters, maxMethodLength).Visit(root);
			return document.WithSyntaxRoot(newRoot);
		}
	}
}