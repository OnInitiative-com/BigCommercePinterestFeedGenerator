using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BigCommerceAccess;
using BigCommerceAccess.Models.Configuration;
using BigCommerceAccess.Models.Product;
using BigCommerceAccess.Models.Category;
using LINQtoCSV;
using Netco.Logging;
using NUnit.Framework;
using System.Text.RegularExpressions;
using System.Text;
using System.Net;
using WebDav;
using System.Net.Http;
using System.Net.Http.Headers;

namespace OnInitiative.com_Pinterest_Feed_Generator
{
    internal class BCStoreAccess
    {

        private readonly IBigCommerceFactory BigCommerceFactory = new BigCommerceFactory();
        private BigCommerceConfig ConfigV3;

        public BCStoreAccess()
        {
            NetcoLogger.LoggerFactory = new NullLoggerFactory();

            string credentialsFilePath = Directory.GetCurrentDirectory() + "\\BigCommerceCredentials.csv";

            var cc = new CsvContext();

            var csvFileAccess = cc.Read<CSV_StoreCredentialParameters>(credentialsFilePath, new CsvFileDescription { FirstLineHasColumnNames = true, IgnoreUnknownColumns = true }).FirstOrDefault();

            if (csvFileAccess != null)
            {
                this.ConfigV3 = new BigCommerceConfig(csvFileAccess.ShortShopName, csvFileAccess.ClientId, csvFileAccess.ClientSecret, csvFileAccess.ApiKey);
            }
        }	

		public List<BigCommerceProduct> GetProductsV3()
		{
			var service = this.BigCommerceFactory.CreateProductsService(this.ConfigV3);
			List<BigCommerceProduct> products = service.GetProducts(true);

			return products;
		}

		public List<BigCommerceCategory> GetCategoriesV3()
		{
			var service = this.BigCommerceFactory.CreateCategoriesService(this.ConfigV3);
			List<BigCommerceCategory> categories = service.GetCategories();

			return categories;
		}

		/// <summary>
		/// Gets the list of Categories filled with Google product categories.
		/// </summary>
		public List<CSV_CategoriesWithGoogleClassifications> GetCategoriesWithGoogleClassification()
		{
            try
            {
				string credentialsFilePath = Directory.GetCurrentDirectory() + "\\BigcommerceCategoriesCSV.csv";

				List<CSV_CategoriesWithGoogleClassifications> catList = new List<CSV_CategoriesWithGoogleClassifications>();

				var cc = new CsvContext();

				var csvCategoriesCompleted = cc.Read<CSV_CategoriesWithGoogleClassifications>(credentialsFilePath, new CsvFileDescription { FirstLineHasColumnNames = true, IgnoreUnknownColumns = true });

				if (csvCategoriesCompleted != null)
				{
					foreach (var item in csvCategoriesCompleted)
					{
						catList.Add(new CSV_CategoriesWithGoogleClassifications
						{
							Id = item.Id,
							Name = item.Name,
							Url = item.Url,
							GoogleProductCategory = item.GoogleProductCategory
						});
					}
				}

				return catList;
			}
            catch (Exception ex)
            {
                throw ex;
            }
			
		}


		/// <summary>
		/// Compares if the created BigCommerceCategoriesCSV file is current with the 
		/// existing categories in the BigCommerce store.
		/// </summary>
		public bool IsBigCommerceWithGoogleCategoryFileLatest(List<CSV_CategoriesWithGoogleClassifications> catBCGoogleList, List<BigCommerceCategory> bcCategoriesList)
		{

			if (catBCGoogleList.Count != bcCategoriesList.Count)
				return false;

			IDictionary<int, string> BCGoogleCatDictionary = new Dictionary<int, string>();
			IDictionary<int, string> BCCatDictionary = new Dictionary<int, string>();

            foreach (var bcgitem in catBCGoogleList)
            {
			
				BCGoogleCatDictionary.Add(Int32.Parse(bcgitem.Id), bcgitem.Name);

			}

			foreach (var bcCatRecord in bcCategoriesList)
			{

				BCCatDictionary.Add((int)bcCatRecord.Id, bcCatRecord.Category_Name);

			}		

			//check keys and values for equality
			return (BCGoogleCatDictionary.Keys.SequenceEqual(BCCatDictionary.Keys) && BCGoogleCatDictionary.Keys.All(k => BCGoogleCatDictionary[k].SequenceEqual(BCCatDictionary[k])));

		}

		/// <summary>
		/// Gets the list of additional images of a given product.
		/// </summary>
		public string GetAdditionalImageLinks(List<BigCommerceImage> productNotThumbnailImagesList)
		{
			if (productNotThumbnailImagesList.Count == 0)
				return String.Empty;

			StringBuilder additionalImageList = new StringBuilder();

			var queue = new Queue<BigCommerceImage>(productNotThumbnailImagesList);

			additionalImageList.Append("\"");

			additionalImageList.Append(queue.Dequeue().UrlStandard);

			while (queue.Count !=0)
            {
				additionalImageList.Append("," + queue.Dequeue().UrlStandard);
			}
          
			additionalImageList.Append("\"");

			return additionalImageList.ToString();

		}

		/// <summary>
		/// Gets the BigCommerce store name.
		/// </summary>
		public string GetStoreName()
		{
			var service = this.BigCommerceFactory.CreateProductsService(this.ConfigV3);
			var name = service.GetStoreName();

			return name;
		}

		/// <summary>
		/// Gets the BigCommerce Store domain name.
		/// </summary>
		public string GetStoreDomain()
        {
            var service = this.BigCommerceFactory.CreateProductsService(this.ConfigV3);
            var name = service.GetStoreDomain();

            return name;
        }

		/// <summary>
		/// Gets the BigCommerce Store URL.
		/// </summary>
		public string GetStoreSafeURL()
		{
			var service = this.BigCommerceFactory.CreateProductsService(this.ConfigV3);
			var name = service.GetStoreSafeURL();

			return name;
		}

		/// <summary>
		/// Removes HTML from string with Regex.
		/// </summary>
		public string StripHTML(string source)
		{
			return Regex.Replace(source, "<.*?>", string.Empty);
		}

		/// <summary>
		/// Gets Pinterest Sale Price.
		/// </summary>
		public string GetSalePrice(decimal? sale_price, decimal? price)
		{

			string final_price = String.Empty;

			if (sale_price == price || sale_price == 0)
				final_price = "";
			else
				final_price = sale_price.ToString();

			return final_price;
		}

		/// <summary>
		/// Get Availability Pinterest parameter value.
		/// </summary>
		public string GetAvailability(string bc_availability)
		{

			string final_value = String.Empty;

			if (bc_availability == "available")
				final_value = "in stock";
			else if (bc_availability == "disabled")
				final_value = "out of stock";
			else
				final_value = "preorder";

			return final_value;
		}

		/// <summary>
		/// Format to Pinterest category standard name.
		/// </summary>
		public string formatCategoryName(string source)
		{
			TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
            string categoryNameInCaps = myTI.ToTitleCase(source);

			string catNameInCapsWithNoBeginEndSlash = categoryNameInCaps.Substring(1, categoryNameInCaps.Length - 2);

			string catNameFormatted = catNameInCapsWithNoBeginEndSlash.Replace("/", " > ");

			return catNameFormatted;
		}

		/// <summary>
		/// Uploads BigCommerce products CSV file to the store WebDav server.
		/// </summary>
		public async Task<bool> UploadBigCommerceCatalogAsync(string catalogFilePath)
		{
			try
			{
				string credentialsFilePath = Directory.GetCurrentDirectory() + "\\BigCommerceCredentials.csv";

				var cc = new CsvContext();

				var csvFileAccess = cc.Read<CSV_StoreCredentialParameters>(credentialsFilePath, new CsvFileDescription { FirstLineHasColumnNames = true, IgnoreUnknownColumns = true }).FirstOrDefault();

				if (csvFileAccess != null)
				{

					var clientParams = new WebDavClientParams
					{
						BaseAddress = new Uri(csvFileAccess.WebDavPath),
						Credentials = new NetworkCredential(csvFileAccess.WebDavUsername, csvFileAccess.WebDavPassword),
						
					};

                    using (var client = new WebDavClient(clientParams))
                    {
						// Content headers need to be set directly on HttpContent instance.
						var content = new StreamContent(File.OpenRead(catalogFilePath));
						content.Headers.ContentRange = new ContentRangeHeaderValue(0, 2);
						var result = await client.PutFile(csvFileAccess.WebDavPath + "/content/product_feed.csv", File.OpenRead(catalogFilePath), "text/csv"); // upload resource
					}
                }

				return true;
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}

		[Test]
		public async Task GetProductsV3Async()
		{
			var service = this.BigCommerceFactory.CreateProductsService(this.ConfigV3);
			var products = await service.GetProductsAsync(CancellationToken.None);			
		}	

	}
}