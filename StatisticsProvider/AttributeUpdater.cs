using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DebtAnalyzer.DebtAnnotation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace StatisticsProvider
{
	public static class AttributeUpdater
	{
		public static async Task UpdateAttributes(Solution solution)
		{
			var analyzer = new MethodDebtAnalyzer();
			var project = solution.Projects.First();

			//TODO dit gaat niet werken omdat hij bestaande attributes niet update. Hij voegt alleen missende toe.
			var compilationWithAnalyzers = project.GetCompilationAsync().Result.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));
			var diagnostics = await compilationWithAnalyzers.GetAllDiagnosticsAsync();
			var fixer = new MyFixAllProvider();
			IEnumerable<string> diagnosticIds = diagnostics.Select(d => d.Id);
			var fixAllContext = new FixAllContext(project, new MethodDebtAnnotationProvider(), FixAllScope.Solution, "", diagnosticIds, new Provider(diagnostics), CancellationToken.None);
			var fixAction = await fixer.GetFixAsync(fixAllContext);
			var operations = fixAction.GetOperationsAsync(CancellationToken.None).Result;
			var newSolution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
			newSolution.Workspace.TryApplyChanges(newSolution);
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
