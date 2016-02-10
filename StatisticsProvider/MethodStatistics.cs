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

		public int FatLineCount { get; set; }

		public MethodStatistics(int fatLineCount, int totalLines, int linesInFatMethods, int methodCount, int fatMethodCount, 
			int methodsWithTooManyParameters, int totalParameters)
		{
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
			return new MethodStatistics(FatLineCount, TotalLines + other.TotalLines, LinesInFatMethods + other.LinesInFatMethods,
				MethodCount + other.MethodCount, FatMethodCount + other.FatMethodCount, 
				MethodsWithTooManyParameters + other.MethodsWithTooManyParameters,
				TotalParameters + other.TotalParameters);
		}

		public string Print()
		{
			return $"methodCount = {MethodCount}\n" +
				   $"fatMethodCount = {FatMethodCount} ({(FatMethodCount / (double)MethodCount).ToString("P")})\n" +
				   $"totalLinesInMethods = {TotalLines}\n" +
				   $"averageLinesPerMethod = {(TotalLines / (double)MethodCount).ToString("N")}\n" +
				   $"linesInFatMethods = {LinesInFatMethods} ({(LinesInFatMethods / (double)TotalLines).ToString("P")})\n" +
				   $"averageMethodParameterCount = {(TotalParameters / (double)MethodCount).ToString("N")}\n" +
				   $"methodsWithTooManyParameters = {MethodsWithTooManyParameters} ({(MethodsWithTooManyParameters / (double)MethodCount).ToString("P")})\n" +
				   "";
		}
	}
}