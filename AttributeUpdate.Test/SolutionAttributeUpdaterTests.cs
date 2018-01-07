using System.Linq;
using DebtRatchet.MethodDebt;
using NUnit.Framework;
using CodeFixVerifier = DebtRatchet.Test.Verifiers.CodeFixVerifier;
using DiagnosticVerifier = DebtRatchet.Test.Verifiers.DiagnosticVerifier;

namespace AttributeUpdater.Test
{
	
	public class SolutionAttributeUpdaterTests
	{
		public SolutionAttributeUpdaterTests()
		{
			MethodParameterCountAnalyzer.DefaultMaximumParameterCount = 5;
		}

		[Test]
		public void TestUpdate()
		{
			var project = DiagnosticVerifier.CreateProject(new[] { OutdatedAnnotationProgram, OutdatedAnnotationProgram, ProgramWithUnnecessaryAnnotation });
			var newSolution = SolutionAttributeUpdater.UpdateAttributes(project.Solution).Result;
			var document = newSolution.Projects.SelectMany(newProject => newProject.Documents).First();
			Assert.AreEqual(FixedProgram, CodeFixVerifier.GetStringFromDocument(document));

			var document2 = newSolution.Projects.SelectMany(newProject => newProject.Documents).Skip(1).First();
			Assert.AreEqual(FixedProgram, CodeFixVerifier.GetStringFromDocument(document2));

			var document3 = newSolution.Projects.SelectMany(newProject => newProject.Documents).Skip(2).First();
			Assert.AreEqual(ProgramWithoutAnnotationNeeded, CodeFixVerifier.GetStringFromDocument(document3));
		}

		public static string ProgramWithUnnecessaryAnnotation => @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using DebtAnalyzer;

namespace ConsoleApplication1
{
    //Joo
    [TypeHasDebt(LineCount = 100, FieldCount = 8)]
    class TypeName
    {
        /// <summary>
        /// 
        /// </summary>
        [MethodHasDebt(LineCount = 1, ParameterCount = 8)]
        void MyBadMethod2443(int a, int b, int c)
        {

        }
    }
}
";

		public static string ProgramWithoutAnnotationNeeded => @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using DebtAnalyzer;

namespace ConsoleApplication1
{
    //Joo
    class TypeName
    {
        /// <summary>
        /// 
        /// </summary>
        void MyBadMethod2443(int a, int b, int c)
        {

        }
    }
}
";

		public static string OutdatedAnnotationProgram => @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using DebtAnalyzer;

namespace ConsoleApplication1
{
    //Joo
    class TypeName
    {
        /// <summary>
        /// 
        /// </summary>
        [MethodHasDebt(LineCount = 1, ParameterCount = 8)]
        void MyBadMethod2443(int a, int b, int c, int d, int e, int g)
        {

        }
    }
}
";

		public static string FixedProgram => @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using DebtAnalyzer;

namespace ConsoleApplication1
{
    //Joo
    class TypeName
    {
        /// <summary>
        /// 
        /// </summary>
        [MethodHasDebt(LineCount = 1, ParameterCount = 6)]
        void MyBadMethod2443(int a, int b, int c, int d, int e, int g)
        {

        }
    }
}
";
	}
}