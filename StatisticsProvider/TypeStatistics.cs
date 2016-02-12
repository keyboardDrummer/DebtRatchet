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
			return $"Classes with more than {FatClassBoundary} lines are fat\n" +
				   $"Classes with more than {TooManyFieldsBoundary} fields have too many\n" +
				   $"# of classes = {TotalClasses}\n" +
				   $"# of lines = {TotalLines}\n" +
				   $"Average # of lines per class = {(TotalLines / (double)TotalClasses).ToString("N")}\n" +
				   $"# of fat classes = {FatClasses} ({(FatClasses / (double)TotalClasses).ToString("P")})\n" +
				   $"# of lines in fat classes = {LinesInFatClasses} ({(LinesInFatClasses / (double)TotalLines).ToString("P")})\n" +
				   $"Average # of fields per class = {(TotalFields / (double)TotalClasses).ToString("N")}\n" +
				   $"# of classes with too many fields = {ClassesWithTooManyFields} ({(ClassesWithTooManyFields / (double)TotalClasses).ToString("P")})\n" +
				   "";
		}

		public void FoundClass(string name, int classLineCount, int fieldCount)
		{
			TotalLines += classLineCount;
			TotalClasses++;

			if (classLineCount > FatClassBoundary)
			{
				LinesInFatClasses += classLineCount;
				FatClasses++;
			}

			TotalFields += fieldCount;
			if (fieldCount > TooManyFieldsBoundary)
			{
				ClassesWithTooManyFields++;
			}
		}
	}
}