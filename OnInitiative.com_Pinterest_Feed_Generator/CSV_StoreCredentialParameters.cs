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

		[CsvColumn(Name = "WebDavPath", FieldIndex = 5)]
		public string WebDavPath { get; set; }

		[CsvColumn(Name = "WebDavUsername", FieldIndex = 6)]
		public string WebDavUsername { get; set; }

		[CsvColumn(Name = "WebDavPassword", FieldIndex = 7)]
		public string WebDavPassword { get; set; }
	}
}