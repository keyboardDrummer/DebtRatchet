using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
				foreach (var documentId in documentIds)
				{
					var document = project.GetDocument(documentId);
					var newDocument = await UpdateAttributes(document);
					project = newDocument.Project;
				}
				result = project.Solution;
			}
			return result;
		}

		static async Task<Document> UpdateAttributes(Document document)
		{
			var root = await document.GetSyntaxRootAsync();
			var newRoot = new ClassAttributeUpdater(document.Project.Solution.Workspace).Visit(root);
			return document.WithSyntaxRoot(newRoot);
		}
	}
}