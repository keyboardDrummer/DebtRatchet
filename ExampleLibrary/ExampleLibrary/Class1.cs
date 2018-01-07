using DebtRatchet;
using ExampleLibrary;
using System;

[assembly: MaxMethodLength(2)]

namespace ExampleLibrary
{   

#pragma warning disable CS0168 // Variable is declared but never used
    public class Class1
    {
        public void TooLongMethod()
        {
            int a;
            int b;
            int c;
            int d;
        }
    }
#pragma warning restore CS0168 // Variable is declared but never used
}
