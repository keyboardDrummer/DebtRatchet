using DebtAnalyzer.MethodDebt;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;
using NUnit.Framework;

namespace DebtAnalyzer.Test
{
	
	public class TestMethodLengthAnalzyer : CodeFixVerifier
	{
		public TestMethodLengthAnalzyer()
		{
			MethodLengthAnalyzer.DefaultMaximumMethodLength = 3;
		}

		static string StaticConstructorWithDebtAnnotation = @"
using System;
using DebtAnalyzer;

namespace ConsoleApplication1
{
	class LongMethodClass
	{
		[DebtMethod(LineCount = 4)]
		static LongMethodClass()
		{
			int a1;
			int a2;
			int a3;
			int a4;
		}
	}
}";

		static string LongMethodWithAnnotation => @"
using System;
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
        }
    }
}";

		static string LongConstructor => @"
using System;
using DebtAnalyzer;

namespace ConsoleApplication1
{
    class LongMethodClass
    {

        void LongMethodClass()
        {
			int a1;
			int a2;
			int a3;
			int a4;
        }
    }
}
";

		static string LongConstructorFixed => @"
using System;
using DebtAnalyzer;

namespace ConsoleApplication1
{
    class LongMethodClass
    {

        [DebtMethod(LineCount = 4, ParameterCount = 0)]
        void LongMethodClass()
        {
			int a1;
			int a2;
			int a3;
			int a4;
        }
    }
}
";

		static string MaximumMethodLengthFive => @"
using System;
using DebtAnalyzer;

[assembly: MaxMethodLength(2)]
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

		static string LongMethodFixed => @"
using System;
using DebtAnalyzer;

namespace ConsoleApplication1
{
    class LongMethodClass
    {

        [DebtMethod(LineCount = 4, ParameterCount = 0)]
        void MyLongMethod()
        {
			int a1;
			int a2;
			int a3;
			int a4;
        }
    }
}
";
		static string LongMethodWithDebtUsing => @"
using System;
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
        }
    }
}
";

		static string LongMethodWithOutdatedAnnotation => @"
using System;

namespace ConsoleApplication1
{
    class LongMethodClass
    {

        [DebtMethod(LineCount = 5, ParameterCount = 0)]
        void MyLongMethod()
        {
			int a1;
			int a2;
			int a3;
			int a4;
        }
    }
}
";
		static string AbstractMethod => @"
namespace ConsoleApplication1
{
    class AbstractMethodClass
    {
          protected abstract void AbstractMethod();
    }
}
";
		static string LongMethod => @"
using System;

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
        }
    }
}
";

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new MethodDebtAnalyzer();
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new MethodDebtAnnotationProvider();
		}

		[Test]
		public void TestDiagnosticWithCustomSettings()
		{
			var test = LongMethod;
			var expected = new DiagnosticResult
			{
				Id = "MethodLengthAnalyzer",
				Message = "Method MyLongMethod is 4 lines long while it should not be longer than 2 lines.",
				Severity = DiagnosticSeverity.Info,
				Locations =
					new[]
					{
						new DiagnosticResultLocation("Test0.cs", 9, 14)
					}
			};

			VerifyCSharpDiagnostic(new[] {test, MaximumMethodLengthFive}, expected);
		}

		[Test]
		public void TestDebtAnnotationForStaticConstructor()
		{
			VerifyCSharpDiagnostic(new[] { DebtAnalyzerTestUtil.DebtMethodAnnotation, StaticConstructorWithDebtAnnotation });
		}

		[Test]
		public void TestDiagnosticForAbstractMethod()
		{
			VerifyCSharpDiagnostic(new[] {AbstractMethod});
		}

		[Test]
		public void TestDiagnostic()
		{
			var test = LongMethod;
			var expected = new DiagnosticResult
			{
				Id = "MethodLengthAnalyzer",
				Message = "Method MyLongMethod is 4 lines long while it should not be longer than 3 lines.",
				Severity = DiagnosticSeverity.Info,
				Locations =
					new[]
					{
						new DiagnosticResultLocation("Test0.cs", 9, 14)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[Test]
		public void TestDiagnosticAsError()
		{
			var test = LongMethod;
			var expected = new DiagnosticResult
			{
				Id = "MethodLengthAnalyzer",
				Message = "Method MyLongMethod is 4 lines long while it should not be longer than 3 lines.",
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[]
					{
						new DiagnosticResultLocation("Test0.cs", 9, 14)
					}
			};

			VerifyCSharpDiagnostic(new[] {test, DebtAnalyzerTestUtil.DebtAsError}, expected);
		}

		[Test]
		public void TestDiagnosticWithDebtAnnotation()
		{
			VerifyCSharpDiagnostic(new[] {DebtAnalyzerTestUtil.DebtMethodAnnotation, LongMethodWithAnnotation});
		}

		[Test]
		public void TestExternalFixNoDoubleUsing()
		{
			VerifyCSharpFix(LongMethodWithDebtUsing, LongMethodFixed, allowNewCompilerDiagnostics: true);
		}

		[Test]
		public void TestFixNoDoubleUsing()
		{
			VerifyCSharpFix(LongMethodWithDebtUsing, LongMethodFixed, allowNewCompilerDiagnostics: true);
		}

		[Test]
		public void TestConstructorFix()
		{
			VerifyCSharpFix(LongConstructor, LongConstructorFixed, allowNewCompilerDiagnostics: true);
		}

		[Test]
		public void TestFix()
		{
			VerifyCSharpFix(LongMethod, LongMethodFixed, allowNewCompilerDiagnostics: true);
		}

		[Test]
		public void TestOverwriteFix()
		{
			VerifyCSharpFix(LongMethodWithOutdatedAnnotation, LongMethodFixed, allowNewCompilerDiagnostics: true);
		}
	}
}