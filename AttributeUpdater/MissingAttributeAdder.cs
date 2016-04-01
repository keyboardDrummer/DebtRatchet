using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DebtAnalyzer.ClassDebt;
using DebtAnalyzer.DebtAnnotation;
using DebtAnalyzer.MethodDebt;
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
			//var solutionWithDebtMethods = await ApplyFixToSolution(new MethodDebtAnalyzer(), new MethodDebtAnnotationProvider(), new MethodDebtFixAllProvider(), solution);
			return await ApplyFixToSolution(new TypeDebtAnalyzer(), new TypeDebtAnnotationProvider(), new TypeDebtFixAllProvider(), solution);
		}

		static async Task<Solution> ApplyFixToSolution(DiagnosticAnalyzer analyzer, CodeFixProvider annotationProvider, FixAllProvider fixAllProvider, 
			Solution solution)
		{
			var project = solution.Projects.First();
			var compilation = await project.GetCompilationAsync();
			var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer));
			var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(ImmutableArray.Create(analyzer), CancellationToken.None);
			var fixAllContext = new FixAllContext(project, annotationProvider, FixAllScope.Solution, "",
				diagnostics.Select(d => d.Id), new Provider(diagnostics), CancellationToken.None);
			var fixAction = await fixAllProvider.GetFixAsync(fixAllContext);
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
