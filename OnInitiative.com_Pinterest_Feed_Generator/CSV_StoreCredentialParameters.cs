using LINQtoCSV;

namespace OnInitiative.com_Pinterest_Feed_Generator
{
	internal class CSV_StoreCredentialParameters
	{
		[CsvColumn(Name = "ApiKey", FieldIndex = 1)]
		public string ApiKey { get; set; }

		[CsvColumn(Name = "ShortShopName", FieldIndex = 2)]
		public string ShortShopName { get; set; }

		[CsvColumn(Name = "ClientId", FieldIndex = 3)]
		public string ClientId { get; set; }

		[CsvColumn(Name = "ClientSecret", FieldIndex = 4)]
		public string ClientSecret { get; set; }
	}
}