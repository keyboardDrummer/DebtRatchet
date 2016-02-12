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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

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
				var documentsGroups = allDiagnostics.GroupBy(x => project.GetDocument(x.Location.SourceTree).Id);
				Project result = project;
				foreach (var documentGroup in documentsGroups)
				{
					var documentId = documentGroup.Key;
					var document = result.GetDocument(documentId);
					IEnumerable<Diagnostic> diagnostics = documentGroup;
					var annotator = new AnnotateMethods(diagnostics);
					token.ThrowIfCancellationRequested();
					var root = (CompilationUnitSyntax)await document.GetSyntaxRootAsync(token);
					var rootWithUsing = TechnicalDebtAnnotationProvider.AddUsing(root);
					var fixedRoot = annotator.Visit(rootWithUsing);
					result = document.WithSyntaxRoot(fixedRoot).Project;
				}
				solution = result.Solution;
			}
			return solution;
		}

		class AnnotateMethods : CSharpSyntaxRewriter
		{
			readonly ImmutableHashSet<int> spans;

			public AnnotateMethods(IEnumerable<Diagnostic> diagnostics) : base(false)
			{
				spans = diagnostics.Select(diagnostic => diagnostic.Location.GetLineSpan().StartLinePosition.Line).ToImmutableHashSet();
			}

			public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
			{
				return VisitMethodBase(node.Identifier, node);
			}

			public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
			{
				return VisitMethodBase(node.Identifier, node);
			}

			SyntaxNode VisitMethodBase(SyntaxToken identifier, BaseMethodDeclarationSyntax node)
			{
				if (spans.Contains(identifier.GetLocation().GetLineSpan().StartLinePosition.Line))
				{
					var withoutDebtAnnotations = TechnicalDebtAnnotationProvider.RemoveExistingDebtAnnotations(node);
					return TechnicalDebtAnnotationProvider.GetNewMethod(withoutDebtAnnotations);
				}
				return node;
			}
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