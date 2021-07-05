using LINQtoCSV;

namespace BigCommercePinterestFeed
{

	/// <summary>
	/// Stores BigCommerce credentials and other config parameters.
	/// </summary>
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

		[CsvColumn(Name = "FeedFileName", FieldIndex = 8)]
		public string FeedFileName { get; set; }

		[CsvColumn(Name = "MailSMTPAddress", FieldIndex = 9)]
		public string MailSMTPAddress { get; set; }

		[CsvColumn(Name = "MailPort", FieldIndex = 10)]
		public string MailPort { get; set; }

		[CsvColumn(Name = "FromEMail", FieldIndex = 11)]
		public string FromEMail { get; set; }

		[CsvColumn(Name = "MailPassword", FieldIndex = 12)]
		public string MailPassword { get; set; }

		[CsvColumn(Name = "MailToAddress", FieldIndex = 13)]
		public string MailToAddress { get; set; }
	}
}