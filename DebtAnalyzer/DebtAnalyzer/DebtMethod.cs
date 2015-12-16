using System;

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
}