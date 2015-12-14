using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace DebtAnalyzer.Test
{
	[TestClass]
	public class TestMethodLengthAnalzyer : CodeFixVerifier
	{
		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return base.GetCSharpCodeFixProvider();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new MethodLengthAnalayzer();
		}

		[TestMethod]
		public void TestDiagnostic()
		{
			var test = @"
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
            void MyLongMethod()
            {
				int a1;
				int a2;
				int a3;
				int a4;
				int a5;
				int a6;
				int a7;
				int a8;
				int a9;
				int a10;
				int a11;
				int a12;
				int a13;
				int a14;
				int a15;
				int a16;
				int a17;
				int a18;
				int a19;
				int a20;
				int a21;
            }
        }
    }";
			var expected = new DiagnosticResult
			{
				Id = "MethodLengthAnalayzer",
				Message = String.Format("Method MyLongMethod is 23 lines long while it should be longer than 20 lines."),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 14, 13)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

	}
}