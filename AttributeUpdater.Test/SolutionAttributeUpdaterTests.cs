using Microsoft.VisualStudio.TestTools.UnitTesting;
using ÀttributeUpdater;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestHelper;

namespace AttributeUpdater.Test
{
	[TestClass]
	public class SolutionAttributeUpdaterTests
	{
		[TestMethod]
		public void TestUpdate()
		{
			var project = DiagnosticVerifier.CreateProject(new[] {OutdatedAnnotationProgram});
			var newSolution = SolutionAttributeUpdater.UpdateAttributes(project.Solution).Result;
			var document = newSolution.Projects.SelectMany(newProject => newProject.Documents).First();
			var documentText = CodeFixVerifier.GetStringFromDocument(document);
			Assert.AreEqual(FixedProgram, documentText);
		}

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
    class TypeName
    {
        /// <summary>
        /// 
        /// </summary>
        [DebtMethod(LineCount = 1, ParameterCount = 8)]
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
    class TypeName
    {
        /// <summary>
        /// 
        /// </summary>
        [DebtMethod(LineCount = 1, ParameterCount = 6)]
        void MyBadMethod2443(int a, int b, int c, int d, int e, int g)
        {

        }
    }
}
";
	}
}