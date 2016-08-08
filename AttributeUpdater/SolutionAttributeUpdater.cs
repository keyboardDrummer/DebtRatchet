using System.Linq;
using System.Threading.Tasks;
using DebtRatchet.ClassDebt;
using DebtRatchet.MethodDebt;
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
				var updater = await GetClassUpdater(project);
				foreach (var documentId in documentIds)
				{
					var document = project.GetDocument(documentId);
					var newDocument = await UpdateAttributes(document, updater);
					project = newDocument.Project;
				}
				result = project.Solution;
			}
			return result;
		}

		static async Task<ClassAttributeUpdater> GetClassUpdater(Project project)
		{
			var compilation = await project.GetCompilationAsync();
			var maxParameters = MethodParameterCountAnalyzer.GetMaxParameterCount(compilation.Assembly);
			var maxMethodLength = MethodLengthAnalyzer.GetMaxLineCount(compilation.Assembly);
			var maxTypeLength = TypeLengthAnalyzer.GetMaxLineCount(compilation.Assembly);
			var maxFieldCount = FieldCountAnalyzer.GetMaxFieldCount(compilation.Assembly);
			return new ClassAttributeUpdater(project.Solution.Workspace, maxParameters, maxMethodLength, maxTypeLength, maxFieldCount);
		}

		static async Task<Document> UpdateAttributes(Document document, ClassAttributeUpdater updater)
		{
			var root = await document.GetSyntaxRootAsync();
			var newRoot = updater.Visit(root);
			return document.WithSyntaxRoot(newRoot);
		}
	}
}