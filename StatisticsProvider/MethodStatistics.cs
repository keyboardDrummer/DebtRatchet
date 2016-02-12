namespace StatisticsProvider
{
	class MethodStatistics
	{
		public int TotalLines { get; set; }
		public int LinesInFatMethods { get; set; }
		public int MethodCount { get; set; }
		public int FatMethodCount { get; set; }
		public int MethodsWithTooManyParameters { get; set;  }
		public int TotalParameters { get; set; }
		public int? FatLineCount { get; set; }
		public int? MaxParameterCount { get; set; }

		public MethodStatistics(int? fatLineCount, int? maxParameterCount, int totalLines, int linesInFatMethods, int methodCount, int fatMethodCount, 
			int methodsWithTooManyParameters, int totalParameters)
		{
			MaxParameterCount = maxParameterCount;
			FatLineCount = fatLineCount;
			TotalLines = totalLines;
			LinesInFatMethods = linesInFatMethods;
			MethodCount = methodCount;
			FatMethodCount = fatMethodCount;
			MethodsWithTooManyParameters = methodsWithTooManyParameters;
			TotalParameters = totalParameters;
		}

		public MethodStatistics()
		{
		}

		public MethodStatistics Concat(MethodStatistics other)
		{
			return new MethodStatistics(Combine(FatLineCount, other.FatLineCount), Combine(MaxParameterCount, other.MaxParameterCount), 
				TotalLines + other.TotalLines, LinesInFatMethods + other.LinesInFatMethods,
				MethodCount + other.MethodCount, FatMethodCount + other.FatMethodCount, 
				MethodsWithTooManyParameters + other.MethodsWithTooManyParameters,
				TotalParameters + other.TotalParameters);
		}

		public static int? Combine(int? first, int? second)
		{
			if (first.HasValue && second.HasValue && first.Value == second.Value)
				return first;

			return null;
		}

		public string Print()
		{
			return $"Methods with more than {FatLineCount} lines are fat.\n" +
				   $"Methods with more than {MaxParameterCount} parameters have too many.\n" +
				   $"# of methods = {MethodCount}\n" +
				   $"# of fat methods = {FatMethodCount} ({(FatMethodCount / (double)MethodCount).ToString("P")})\n" +
				   $"# lines in methods = {TotalLines}\n" +
				   $"Average # of lines per methods = {(TotalLines / (double)MethodCount).ToString("N")}\n" +
				   $"# of lines in fat methods = {LinesInFatMethods} ({(LinesInFatMethods / (double)TotalLines).ToString("P")})\n" +
				   $"Average # of parameters per method = {(TotalParameters / (double)MethodCount).ToString("N")}\n" +
				   $"# of methods with too many parameters = {MethodsWithTooManyParameters} ({(MethodsWithTooManyParameters / (double)MethodCount).ToString("P")})\n" +
				   "";
		}

		public void FoundMethod(int length, int parameterCount)
		{
			if (length > FatLineCount)
			{
				LinesInFatMethods += length;
				FatMethodCount++;
			}

			if (parameterCount > MaxParameterCount)
			{
				MethodsWithTooManyParameters++;
			}
			TotalParameters += parameterCount;

			TotalLines += length;
			MethodCount++;
		}
	}
}