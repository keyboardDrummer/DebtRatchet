using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using DebtAnalyzer.DebtAnnotation;
using TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DebtAnalyzer.Test
{
	[TestClass]
    public class TestMethodParameterCountAnalayzer : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void TestEmptyProgramHasNoDiagnostics()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

		[TestMethod]
		public void TestDiagnostic()
		{
			var test = TestProgramInput;
			var expected = new DiagnosticResult
			{
				Id = "DebtAnalyzer",
				Message = String.Format("Method MyBadMethod2443 has 6 parameters while it should not have more than 4."),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
							new DiagnosticResultLocation("Test0.cs", 14, 18)
						}
			};

			VerifyCSharpDiagnostic(new[] { test, DebtAnalyzerTestUtil.DebtMethodAnnotation, MaxParametersAnnotation }, expected);
		}

		[TestMethod]
        public void TestDiagnosticAsError()
        {
            var test = TestProgramInput;
            var expected = new DiagnosticResult
            {
                Id = "DebtAnalyzer",
                Message = String.Format("Method MyBadMethod2443 has 6 parameters while it should not have more than 5."),
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 14, 18)
                        }
            };

            VerifyCSharpDiagnostic(new [] { test, DebtAnalyzerTestUtil.DebtMethodAnnotation, TestMethodLengthAnalzyer.DebtAsError }, expected);
        }


		[TestMethod]
		public void TestDiagnosticAnnotation()
		{
			VerifyCSharpDiagnostic(new[] {DebtAnalyzerTestUtil.DebtMethodAnnotation, FixedProgram });
		}

		[TestMethod]
		public void TestFix()
	    {
			VerifyCSharpFix(TestProgramInput, FixedProgram, allowNewCompilerDiagnostics: true);
	    }
		
		public static string MaxParametersAnnotation => @"

using System;
using DebtAnalyzer;

[assembly: MaxParameters(4)]
namespace DebtAnalyzer
{
	[AttributeUsage(AttributeTargets.Assembly)]
	class MaxParameters : Attribute
	{
		public MaxParameters(int parameterCount)
		{
			ParameterCount = parameterCount;
		}

		public int ParameterCount { get; }
	}
}";

		static string FixedProgram => @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using DebtAnalyzer;

    namespace ConsoleApplication1
    {
        class TypeName
        {
        [DebtMethod(ParameterCount = 6)]
        void MyBadMethod2443(int a, int b, int c, int d, int e, int g)
            {

            }
        }
    }";

	    static string TestProgramInput => @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using DebtAnalyzer;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            void MyBadMethod2443(int a, int b, int c, int d, int e, int g)
            {

            }
        }
    }";

		protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new TechnicalDebtAnnotationProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new MethodParameterCountAnalyzer();
        }
    }
}