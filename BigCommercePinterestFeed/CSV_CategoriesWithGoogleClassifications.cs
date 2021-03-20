using LINQtoCSV;

namespace BigCommercePinterestFeed
{	
	/// <summary>
	/// Represents an instance of the category columns with the Google classification.
	/// </summary>
	public class CSV_CategoriesWithGoogleClassifications
    {
		/// <summary>
		/// Gets or Sets the ID property.
		/// </summary>
		[CsvColumn(Name = "Id", FieldIndex = 1)]       
        public string Id { get; set; }

		/// <summary>
		/// Gets or Sets the Name property.
		/// </summary>
		[CsvColumn(Name = "Name", FieldIndex = 2)]
		public string Name { get; set; }

		/// <summary>
		/// Gets or Sets the Url property.
		/// </summary>
		[CsvColumn(Name = "Url", FieldIndex = 3)]
		public string Url { get; set; }

		/// <summary>
		/// Gets or Sets the GoogleProductCategory property.
		/// </summary>
		[CsvColumn(Name = "GoogleProductCategory", FieldIndex = 4)]
		public string GoogleProductCategory { get; set; }
	}
}