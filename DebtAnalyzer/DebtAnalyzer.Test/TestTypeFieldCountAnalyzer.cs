using DebtAnalyzer.ClassDebt;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace DebtAnalyzer.Test
{
	[TestClass]
	public class TestTypeFieldCountAnalyzer : CodeFixVerifier
	{
		public TestTypeFieldCountAnalyzer()
		{
			FieldCountAnalyzer.DefaultMaximumFieldCount = 2;
		}

		static string TooManyFields => @"
using System;

namespace ConsoleApplication1
{
    class SomeClass
    {
        int a;
        int b;
        int c { get; }
    }
}";

		static string NotTooManyFields => @"
using System;

namespace ConsoleApplication1
{
    class SomeClass
    {
        int a;
        int b;
        const int c;
        static int d;
        static int e { get; }
        int f { get { return 3; } }
        int g { get { return 3; } set { d = value; } }
    }
}";

		static string TooManyFieldsFixed => @"
using System;
using DebtAnalyzer;

namespace ConsoleApplication1
{
    [DebtType(LineCount = 6, FieldCount = 3)]
    class SomeClass
    {
        int a;
        int b;
        int c { get; }
    }
}";

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new TypeDebtAnalyzer();
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new TypeDebtAnnotationProvider();
		}

		[TestMethod]
		public void TestTooManyFields()
		{
			var test = TooManyFields;
			var expected = new DiagnosticResult
			{
				Id = "MaxFieldCount",
				Message = "Type SomeClass has 3 fields it should have more than 2.",
				Severity = DiagnosticSeverity.Info,
				Locations =
					new[]
					{
						new DiagnosticResultLocation("Test0.cs", 6, 11)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestNotTooManyFields()
		{
			VerifyCSharpDiagnostic(NotTooManyFields);
		}

		[TestMethod]
		public void TestFix()
		{
			VerifyCSharpFix(TooManyFields, TooManyFieldsFixed, allowNewCompilerDiagnostics: true);
		}
	}
}
