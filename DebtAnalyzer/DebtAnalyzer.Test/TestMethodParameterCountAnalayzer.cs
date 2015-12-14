using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
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
                Message = String.Format("Method MyBadMethod2443 has 6 parameters while it should not have more than 5."),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 14, 18)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }


		[TestMethod]
		public void TestDiagnosticAnnotation()
		{
			VerifyCSharpDiagnostic(new[] { Annotation, FixedProgram });
		}

		[TestMethod]
		public void TestFix()
	    {
			VerifyCSharpFix(TestProgramInput, FixedProgram, allowNewCompilerDiagnostics: true);
	    }

		static string Annotation => @"using System;

namespace DebtAnalyzer
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DebtMethod : Attribute
    {
        public DebtMethod(int parameterCount)
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
        [DebtMethod(6)]
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