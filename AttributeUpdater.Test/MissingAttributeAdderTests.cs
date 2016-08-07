using System.Linq;
using DebtAnalyzer.MethodDebt;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using CodeFixVerifier = DebtRatchet.Test.Verifiers.CodeFixVerifier;
using DiagnosticVerifier = DebtRatchet.Test.Verifiers.DiagnosticVerifier;

namespace AttributeUpdater.Test
{
	
	public class MissingAttributeAdderTest
	{
		public MissingAttributeAdderTest()
		{
			MethodParameterCountAnalyzer.DefaultMaximumParameterCount = 5;
		}

		[Test]
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