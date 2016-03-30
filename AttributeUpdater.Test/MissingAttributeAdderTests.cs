using Microsoft.VisualStudio.TestTools.UnitTesting;
using ÀttributeUpdater;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DebtAnalyzer.ParameterCount;
using TestHelper;

namespace ÀttributeUpdater.Test
{
	[TestClass]
	public class MissingAttributeAdderTest
	{
		public MissingAttributeAdderTest()
		{
			MethodParameterCountAnalyzer.DefaultMaximumParameterCount = 5;
		}

		[TestMethod]
		public void TestAddMissingAttribute()
		{
			var project = DiagnosticVerifier.CreateProject(new[] { OutdatedAnnotationProgram });
			var newSolution = MissingAttributeAdder.AddMissingAttributes(project.Solution).Result;
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