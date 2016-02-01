using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace DebtAnalyzer.DebtAnnotation
{
	public class MyFixAllProvider : FixAllProvider
	{
		public override async Task<CodeAction> GetFixAsync(FixAllContext fixAllContext)
		{
			var allDiagnostics = await fixAllContext.GetAllDiagnosticsAsync(fixAllContext.Project);
			var solution = fixAllContext.Solution;
			var documentIds = allDiagnostics.ToDictionary(x => x, x => solution.GetDocument(x.Location.SourceTree).Id);
			return CodeAction.Create(TechnicalDebtAnnotationProvider.Title, token =>
			{
				return Task.Run(async () =>
				{
					foreach (var diagnostic in allDiagnostics)
					{
						var document = solution.GetDocument(documentIds[diagnostic]);
						var root = await document.GetSyntaxRootAsync(token);
						var methodSyntax = (BaseMethodDeclarationSyntax) root.FindNode(diagnostic.Location.SourceSpan);
						solution = await TechnicalDebtAnnotationProvider.AddDebtAnnotation(document, methodSyntax, token);
					}
					return solution;
				}, token);
			});
		}
	}
}