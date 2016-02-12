namespace StatisticsProvider
{
	class TypeStatistics
	{
		public int? FatClassBoundary { get; set; }
		public int? TooManyFieldsBoundary { get; set; }
		public int TotalLines { get; set; }
		public int TotalClasses { get; set; }
		public int TotalFields { get; set; }
		public int FatClasses { get; set; }
		public int LinesInFatClasses { get; set; }
		public int ClassesWithTooManyFields { get; set; }

		public TypeStatistics(int? fatClassBoundary, int? tooManyFieldsBoundary, int totalLines, int totalClasses, int totalFields, int fatClasses, int linesInFatClasses, int classesWithTooManyFields)
		{
			FatClassBoundary = fatClassBoundary;
			TooManyFieldsBoundary = tooManyFieldsBoundary;
			TotalLines = totalLines;
			TotalClasses = totalClasses;
			TotalFields = totalFields;
			FatClasses = fatClasses;
			LinesInFatClasses = linesInFatClasses;
			ClassesWithTooManyFields = classesWithTooManyFields;
		}

		public TypeStatistics()
		{
		}

		public TypeStatistics Concat(TypeStatistics other)
		{
			return new TypeStatistics(MethodStatistics.Combine(FatClassBoundary, other.FatClassBoundary),
				MethodStatistics.Combine(TooManyFieldsBoundary, other.TooManyFieldsBoundary),
				TotalLines + other.TotalLines, 
				TotalClasses + other.TotalClasses, 
				TotalFields + other.TotalFields, 
				FatClasses + other.FatClasses,
				LinesInFatClasses + other.LinesInFatClasses,
				ClassesWithTooManyFields + other.ClassesWithTooManyFields);
		}

		public string Print()
		{
			return $"classCount = {TotalClasses}\n" +
				   $"totalLines = {TotalLines}\n" +
				   $"averageLinesPerClass = {(TotalLines / (double)TotalClasses).ToString("N")}\n" +
				   $"fatClassCount = {FatClasses} ({(FatClasses / (double)TotalClasses).ToString("P")})\n" +
				   $"linesInFatClasses = {LinesInFatClasses} ({(LinesInFatClasses / (double)TotalLines).ToString("P")})\n" +
				   $"averageClassFieldCount = {(TotalFields / (double)TotalClasses).ToString("N")}\n" +
				   $"classesWithTooManyFields = {ClassesWithTooManyFields} ({(ClassesWithTooManyFields / (double)TotalClasses).ToString("P")})\n" +
				   "";
		}
	}
}