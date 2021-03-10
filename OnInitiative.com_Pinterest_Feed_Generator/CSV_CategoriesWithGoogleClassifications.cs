using LINQtoCSV;

namespace OnInitiative.com_Pinterest_Feed_Generator
{
	internal class CSV_CategoriesWithGoogleClassifications
	{
		[CsvColumn(Name = "Id", FieldIndex = 1)]
		public string Id { get; set; }

		[CsvColumn(Name = "Name", FieldIndex = 2)]
		public string Name { get; set; }

		[CsvColumn(Name = "Url", FieldIndex = 3)]
		public string Url { get; set; }

		[CsvColumn(Name = "GoogleProductCategory", FieldIndex = 4)]
		public string GoogleProductCategory { get; set; }
	}
}