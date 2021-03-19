using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BigCommerceAccess;
using BigCommerceAccess.Models.Configuration;
using BigCommerceAccess.Models.Product;
using BigCommerceAccess.Models.Category;
using LINQtoCSV;
using Netco.Logging;
using System.Text.RegularExpressions;
using System.Text;
using System.Net;
using WebDav;
using System.Net.Http;
using System.Net.Http.Headers;

namespace BigCommercePinterestFeed
{
    public class BigCommerceStoreAccess : IBigCommerceStoreAccess
    {

        private readonly IBigCommerceFactory BigCommerceFactory = new BigCommerceFactory();
        private BigCommerceConfig ConfigV3;

        /// <summary>
        /// Gets or Sets the BigCommerce crendentials file path.
        /// </summary>
        public string CredentialsFilePath { get; set; }

        /// <summary>
        /// Gets or Sets the BigCommerce categories file path.
        /// </summary>
        public string CategoriesCSVPath { get; set; }

        /// <summary>
        /// Gets or Sets the Pinterest's catalog file path.
        /// </summary>
        public string PinterestCatalogCSVPath { get; set; }

        public BigCommerceStoreAccess()
        {
            NetcoLogger.LoggerFactory = new NullLoggerFactory();

            var cc = new CsvContext();

            var csvFileAccess = cc.Read<CSV_StoreCredentialParameters>(this.CredentialsFilePath, new CsvFileDescription { FirstLineHasColumnNames = true, IgnoreUnknownColumns = true }).FirstOrDefault();

            if (csvFileAccess != null)
            {
                this.ConfigV3 = new BigCommerceConfig(csvFileAccess.ShortShopName, csvFileAccess.ClientId, csvFileAccess.ClientSecret, csvFileAccess.ApiKey);
            }
        }

        /// <summary>
        /// Gets the list of Products from the BigCommerce store.
        /// </summary>
        public List<BigCommerceProduct> GetProducts()
        {
            var service = this.BigCommerceFactory.CreateProductsService(this.ConfigV3);
            List<BigCommerceProduct> products = service.GetProducts(true);

            return products;
        }

        /// <summary>
        /// Gets the list of Categories from the BigCommerce store.
        /// </summary>
        public List<BigCommerceCategory> GetCategories()
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

                List<CSV_CategoriesWithGoogleClassifications> catList = new List<CSV_CategoriesWithGoogleClassifications>();

                var cc = new CsvContext();

                var csvCategoriesCompleted = cc.Read<CSV_CategoriesWithGoogleClassifications>(this.CategoriesCSVPath, new CsvFileDescription { FirstLineHasColumnNames = true, IgnoreUnknownColumns = true });

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
                throw new Exception(ex.Message);
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

            while (queue.Count != 0)
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
            var storeDomain = service.GetStoreDomain();

            return storeDomain;
        }

        /// <summary>
        /// Gets the BigCommerce Store URL.
        /// </summary>
        public string GetStoreSafeURL()
        {
            var service = this.BigCommerceFactory.CreateProductsService(this.ConfigV3);
            var storeSafeURL = service.GetStoreSafeURL();

            return storeSafeURL;
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
        /// Get Availability's Pinterest parameter value.
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
        /// Uploads BigCommerce products CSV file to the store's WebDav server.
        /// </summary>
        public async Task<bool> UploadBigCommerceCatalogAsync(string catalogFilePath)
        {
            try
            {
                var cc = new CsvContext();

                var csvFileAccess = cc.Read<CSV_StoreCredentialParameters>(this.CredentialsFilePath, new CsvFileDescription { FirstLineHasColumnNames = true, IgnoreUnknownColumns = true }).FirstOrDefault();

                if (csvFileAccess != null)
                {

                    var clientParams = new WebDavClientParams
                    {
                        BaseAddress = new Uri(csvFileAccess.WebDavPath),
                        Credentials = new NetworkCredential(csvFileAccess.WebDavUsername, csvFileAccess.WebDavPassword)
                    };

                    using (var client = new WebDavClient(clientParams))
                    {
                        // Content headers need to be set directly on HttpContent instance.
                        var content = new StreamContent(File.OpenRead(catalogFilePath));
                        content.Headers.ContentRange = new ContentRangeHeaderValue(0, 2);
                        var result = await client.PutFile(csvFileAccess.WebDavPath + "/content/" + csvFileAccess.PinterestFeedFileName, File.OpenRead(catalogFilePath), "text/csv"); // upload resource
                    }

                    return true;
                }

                throw new Exception("UploadBigCommerceCatalogAsync -> Could not read the authentication file.");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

    }
}