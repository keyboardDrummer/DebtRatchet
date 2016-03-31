using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DebtAnalyzer.Common;
using DebtAnalyzer.DebtAnnotation;
using DebtAnalyzer.MethodDebt;
using DebtAnalyzer.ParameterCount;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ÀttributeUpdater
{
	public static class MissingAttributeAdder
	{
		public static async Task<Solution> AddMissingAttributes(Solution solution)
		{
			var analyzer = new MethodDebtAnalyzer();
			var project = solution.Projects.First();

			var compilation = await project.GetCompilationAsync();
			var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));
			var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer), CancellationToken.None);
			var fixAllContext = new FixAllContext(project, new MethodDebtAnnotationProvider(), FixAllScope.Solution, "", 
				diagnostics.Select(d => d.Id), new Provider(diagnostics), CancellationToken.None);
			var fixAction = await new GenericDebtFixAllProvider().GetFixAsync(fixAllContext);
			var operations = await fixAction.GetOperationsAsync(CancellationToken.None);
			return operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
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
