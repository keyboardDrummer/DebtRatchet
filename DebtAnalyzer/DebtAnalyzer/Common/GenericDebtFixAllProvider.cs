using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DebtAnalyzer.MethodDebt;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DebtAnalyzer.Common
{
	public abstract class GenericDebtFixAllProvider : FixAllProvider
	{
		protected abstract SyntaxNode FixRootFromDiagnostics(IEnumerable<Diagnostic> diagnostics, CompilationUnitSyntax root);

		public override Task<CodeAction> GetFixAsync(FixAllContext fixAllContext)
		{
			return Task.FromResult(CodeAction.Create(MethodDebtAnnotationProvider.Title, token => FixAll(fixAllContext, token)));
		}

		async Task<Solution> FixAll(FixAllContext fixAllContext, CancellationToken token)
		{
			var relevantProjects = GetRelevantProjects(fixAllContext);
			var solution = fixAllContext.Solution;

			foreach (var projectId in relevantProjects)
			{
				var project = solution.GetProject(projectId);
				var allDiagnostics = await fixAllContext.GetAllDiagnosticsAsync(project);
				var documentsGroups = allDiagnostics.GroupBy(x => project.GetDocument(x.Location.SourceTree).Id);
				Project result = project;
				foreach (var documentGroup in documentsGroups)
				{
					var documentId = documentGroup.Key;
					var document = result.GetDocument(documentId);
					IEnumerable<Diagnostic> diagnostics = documentGroup;
					token.ThrowIfCancellationRequested();
					var root = (CompilationUnitSyntax)await document.GetSyntaxRootAsync(token);
					var rootWithUsing = RoslynUtil.AddUsing(root);
					var fixedRoot = FixRootFromDiagnostics(diagnostics, rootWithUsing);
					result = document.WithSyntaxRoot(fixedRoot).Project;
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