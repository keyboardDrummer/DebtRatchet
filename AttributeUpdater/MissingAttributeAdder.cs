using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DebtRatchet.ClassDebt;
using DebtRatchet.MethodDebt;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AttributeUpdater
{
	public static class MissingAttributeAdder
	{
		public static async Task<Solution> AddMissingAttributes(Solution solution)
		{
			var solutionWithMethodHasDebts = await ApplyFixToSolution(new MethodDebtAnalyzer(), new MethodDebtAnnotationProvider(), new MethodDebtFixAllProvider(), solution);
			return await ApplyFixToSolution(new TypeDebtAnalyzer(), new TypeDebtAnnotationProvider(), new TypeDebtFixAllProvider(), solutionWithMethodHasDebts);
		}

		static async Task<Solution> ApplyFixToSolution(DiagnosticAnalyzer analyzer, CodeFixProvider annotationProvider, FixAllProvider fixAllProvider, 
			Solution solution)
		{
			var result = solution;
			foreach (var projectId in solution.ProjectIds)
			{
				var project = result.GetProject(projectId);
				var compilation = await project.GetCompilationAsync();
				var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer));
				var projectDiagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(ImmutableArray.Create(analyzer), CancellationToken.None);
				var fixAllContext = new FixAllContext(project, annotationProvider, FixAllScope.Project, "",
					projectDiagnostics.Select(d => d.Id), new Provider(projectDiagnostics), CancellationToken.None);
				var fixAction = await fixAllProvider.GetFixAsync(fixAllContext);
				var operations = await fixAction.GetOperationsAsync(CancellationToken.None);
				result = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
			}
			return result;
		}

		class Provider : FixAllContext.DiagnosticProvider
		{
			readonly IEnumerable<Diagnostic> diagnostics;

			public Provider(IEnumerable<Diagnostic> diagnostics)
			{
				this.diagnostics = diagnostics;
			}

			public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken)
			{
				throw new NotImplementedException();
			}

			public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken)
			{
				return Task.FromResult(diagnostics);
			}

			public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken)
			{
				return Task.FromResult(diagnostics);
			}
		}
	}
}
