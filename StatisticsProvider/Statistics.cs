namespace StatisticsProvider
{
	class Statistics
	{
		readonly int fatLineCount;
		readonly int totalLines;
		readonly int linesInFatMethods;
		readonly int methodCount;
		readonly int fatMethodCount;

		public Statistics(int fatLineCount, int totalLines, int linesInFatMethods, int methodCount, int fatMethodCount)
		{
			this.fatLineCount = fatLineCount;
			this.totalLines = totalLines;
			this.linesInFatMethods = linesInFatMethods;
			this.methodCount = methodCount;
			this.fatMethodCount = fatMethodCount;
		}

		public int FatLineCount => fatLineCount;

		public Statistics Concat(Statistics other)
		{
			return new Statistics(fatLineCount, totalLines + other.totalLines, linesInFatMethods + other.linesInFatMethods,
				methodCount + other.methodCount, fatMethodCount + other.fatMethodCount);
		}

		public string Print()
		{
			var linesInFatMethodsPercentage = linesInFatMethods / (double)totalLines;
			var fatMethodCountPercentage = fatMethodCount / (double)methodCount;
			return $"methodCount = {methodCount}\n" +
				   $"fatMethodCount = {fatMethodCount} ({fatMethodCountPercentage.ToString("P")})\n" +
				   $"totalLines = {totalLines}\n" +
			       $"linesInFatMethods = {linesInFatMethods} ({linesInFatMethodsPercentage.ToString("P")})\n" +
				   "";
		}
	}
}