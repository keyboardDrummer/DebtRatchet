namespace StatisticsProvider
{
	public class TypeStatistics
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

		public TypeStatistics(int? fatClassBoundary, int tooManyFieldsBoundary)
		{
			FatClassBoundary = fatClassBoundary;
			TooManyFieldsBoundary = tooManyFieldsBoundary;
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

		public string Print(bool onlyNumbers)
		{
			var averageNumberOfLinesPerClass = TotalLines / (double)TotalClasses;
			var averageNumberOfFieldsPerClass = TotalFields / (double)TotalClasses;
			var linesInFatClassesPercentage = (LinesInFatClasses / (double)TotalLines).ToString("P");
			var numberOfFatClassesPercentage = (FatClasses / (double)TotalClasses).ToString("P");
			var classesWithTooManyFieldsPercentage = (ClassesWithTooManyFields / (double)TotalClasses).ToString("P");
			if (onlyNumbers)
			{
				return TotalClasses + "\n" +
				       TotalLines + "\n" +
				       averageNumberOfLinesPerClass + "\n" +
					   numberOfFatClassesPercentage + "\n" +
					   linesInFatClassesPercentage + "\n" +
				       averageNumberOfFieldsPerClass + "\n" +
					   classesWithTooManyFieldsPercentage;
			}

			return $"Classes with more than {FatClassBoundary} lines are fat\n" +
				   $"Classes with more than {TooManyFieldsBoundary} fields have too many\n" +
				   $"# of classes = {TotalClasses}\n" +
				   $"# of lines = {TotalLines}\n" +
				   $"Average # of lines per class = {averageNumberOfLinesPerClass.ToString("N")}\n" +
				   $"# of fat classes = {FatClasses} ({numberOfFatClassesPercentage})\n" +
				   $"# of lines in fat classes = {LinesInFatClasses} ({linesInFatClassesPercentage})\n" +
				   $"Average # of fields per class = {averageNumberOfFieldsPerClass.ToString("N")}\n" +
				   $"# of classes with too many fields = {ClassesWithTooManyFields} ({classesWithTooManyFieldsPercentage})\n" +
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