using System;
using DebtAnalyzer.DebtAnnotation;
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
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new DebtAnalyzer();
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new TechnicalDebtAnnotationProvider();
		}

		[TestMethod]
		public void TestDiagnosticWithCustomSettings()
		{
			var test = LongMethod;
			var expected = new DiagnosticResult
			{
				Id = "MethodLengthAnalyzer",
				Message = "Method MyLongMethod is 22 lines long while it should not be longer than 5 lines.",
				Severity = DiagnosticSeverity.Info,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 13, 9)
					}
			};

			VerifyCSharpDiagnostic(new [] { test, MaximumMethodLengthFive}, expected);
		}

		[TestMethod]
		public void TestDiagnostic()
		{
			var test = LongMethod;
			var expected = new DiagnosticResult
			{
				Id = "MethodLengthAnalyzer",
				Message = "Method MyLongMethod is 22 lines long while it should not be longer than 20 lines.",
				Severity = DiagnosticSeverity.Info,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 13, 9)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestDiagnosticAsError()
		{
			var test = LongMethod;
			var expected = new DiagnosticResult
			{
				Id = "MethodLengthAnalyzer",
				Message = "Method MyLongMethod is 22 lines long while it should not be longer than 20 lines.",
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 13, 9)
					}
			};

			VerifyCSharpDiagnostic(new [] { test, DebtAnalyzerTestUtil.DebtAsError} , expected);
		}

	[TestMethod]
	public void TestDiagnosticWithDebtAnnotation()
	{
		VerifyCSharpDiagnostic(new [] { DebtAnalyzerTestUtil.DebtMethodAnnotation, LongMethodWithAnnotation });
	}

	[TestMethod]
	public void TestDiagnosticWithAssemblyDebtAnnotation()
	{
		VerifyCSharpDiagnostic(new[] { AssemblyDebtAnnotation, DebtAnalyzerTestUtil.DebtMethodAnnotation, LongMethod });
	}

		static string AssemblyDebtAnnotation => @"
using DebtAnalyzer
[assembly:DebtMethod(LineCount = 30, Target = ""ConsoleApplication1.LongMethodClass.MyLongMethod"")]
";

	static string LongMethodWithAnnotation => @"
 using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using DebtAnalyzer;

    namespace ConsoleApplication1
    {
        class LongMethodClass
        {
			[DebtMethod(LineCount = 30)]
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

		static string MaximumMethodLengthFive => @"
 using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using DebtAnalyzer;

	[assembly: MaxMethodLength(5)]
    namespace DebtAnalyzer
    {

		[AttributeUsage(AttributeTargets.Assembly)]
		class MaxMethodLength : Attribute
		{
			public MaxMethodLength(int length)
			{
				Length = length;
			}

			public int Length { get; }
		}
    }";

	[TestMethod]
	public void TestExternalFixNoDoubleUsing()
	{
		VerifyCSharpFix(LongMethodWithDebtUsing, LongMethodFixed, allowNewCompilerDiagnostics: true);
	}

		[TestMethod]
	public void TestFixNoDoubleUsing()
	{
		VerifyCSharpFix(LongMethodWithDebtUsing, LongMethodFixed, allowNewCompilerDiagnostics: true);
	}

		[TestMethod]public void TestFix()
	{
		VerifyCSharpFix(LongMethod, LongMethodFixed, allowNewCompilerDiagnostics: true);
	}

		static string LongMethodFixed => @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using DebtAnalyzer;

namespace ConsoleApplication1
{
    class LongMethodClass
    {
        [DebtMethod(LineCount = 22, ParameterCount = 0)]
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
			int a22;
        }
    }
}
";
		static string LongMethodWithDebtUsing => @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using DebtAnalyzer;

namespace ConsoleApplication1
{
    class LongMethodClass
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
			int a22;
        }
    }
}
";

		static string LongMethod => @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class LongMethodClass
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
			int a22;
        }
    }
}
";
	}
}