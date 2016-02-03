using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading;

namespace DebtAnalyzer.DebtAnnotation
{
	public class MyFixAllProvider : FixAllProvider
	{
		public override Task<CodeAction> GetFixAsync(FixAllContext fixAllContext)
		{
			return Task.FromResult(CodeAction.Create(TechnicalDebtAnnotationProvider.Title, token => FixAll(fixAllContext, token)));
		}

		static async Task<Solution> FixAll(FixAllContext fixAllContext, CancellationToken token)
		{
			var relevantProjects = GetRelevantProjects(fixAllContext);
			var solution = fixAllContext.Solution;

			foreach (var projectId in relevantProjects)
			{
				var project = solution.GetProject(projectId);
				var allDiagnostics = await fixAllContext.GetAllDiagnosticsAsync(project);
				var documentIds = allDiagnostics.ToDictionary(x => x, x => project.GetDocument(x.Location.SourceTree).Id);
				Project result = project;
				foreach (var diagnostic in allDiagnostics)
				{
					token.ThrowIfCancellationRequested();
					var document = result.GetDocument(documentIds[diagnostic]);
					var root = await document.GetSyntaxRootAsync(token);
					var methodSyntax = (BaseMethodDeclarationSyntax)root.FindNode(diagnostic.Location.SourceSpan);
					result = await TechnicalDebtAnnotationProvider.AddDebtAnnotation(document, methodSyntax, token);
				}
				solution = result.Solution;
			}
			return solution;
		}

		public override IEnumerable<FixAllScope> GetSupportedFixAllScopes()
		{
			return new [] {FixAllScope.Project, FixAllScope.Solution } ;
		}

		static IEnumerable<ProjectId> GetRelevantProjects(FixAllContext fixAllContext)
		{
			switch (fixAllContext.Scope)
			{
				case FixAllScope.Project:
					return new [] { fixAllContext.Project.Id };
				case FixAllScope.Solution:
					return fixAllContext.Solution.ProjectIds;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}