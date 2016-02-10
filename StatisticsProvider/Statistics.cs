namespace StatisticsProvider
{
	class Statistics
	{
		public Statistics(TypeStatistics typeStatistics, MethodStatistics methodStatistics)
		{
			TypeStatistics = typeStatistics;
			MethodStatistics = methodStatistics;
		}

		public Statistics() : this(new TypeStatistics(), new MethodStatistics())
		{

		}

		public TypeStatistics TypeStatistics { get; }

		public MethodStatistics MethodStatistics { get; }

		public Statistics Concat(Statistics other)
		{
			return new Statistics(TypeStatistics.Concat(other.TypeStatistics), MethodStatistics.Concat(other.MethodStatistics));
		}

		public string Print()
		{
			return TypeStatistics.Print() + "\n" + MethodStatistics.Print();
		}
	}
}