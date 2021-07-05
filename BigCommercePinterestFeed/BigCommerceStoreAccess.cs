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
using System.Text.RegularExpressions;
using System.Text;
using System.Net;
using WebDav;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Dynamic;
using CsvHelper;
using System.Web;

namespace BigCommercePinterestFeed
{
    /// <summary>
    /// Represents an instance of the BigCommerce store.
    /// </summary>
    public class BigCommerceStoreAccess : IBigCommerceStoreAccess
    {

        private readonly IBigCommerceFactory BigCommerceFactory = new BigCommerceFactory();

        private BigCommerceConfig ConfigV3;
        private string credentialsFilePath;

        EMailNotification mailObj;

        /// <summary>
        /// Gets or Sets the BigCommerce categories file path.
        /// </summary>
        public string CategoriesCSVPath { get; set; }

        /// <summary>
        /// Gets or Sets the Pinterest's catalog file path.
        /// </summary>
        public string PinterestCatalogCSVPath { get; set; }

        /// <summary>
        /// Gets or Sets the Pinterest's feed file name.
        /// </summary>
        public string FeedFileName { get; set; }

        /// <summary>
        /// Gets or Sets EMailNotification created object instance.
        /// </summary>
        public EMailNotification MailObj { get => mailObj; set => mailObj = value; }

        /// <summary>
        /// Initializes a new instance of the BigCommerceStoreAccess class. 
        /// Establishes the connection to the store using the credentials 
        /// provided within the CSV configuration file.
        /// </summary>
        public BigCommerceStoreAccess(string execPath)
        {
            try
            {

                string credentialsFilePath = execPath + "\\BigCommerceCredentials.csv";

                var cc = new LINQtoCSV.CsvContext();

                var csvFileAccess = cc.Read<CSV_StoreCredentialParameters>(credentialsFilePath, new CsvFileDescription { FirstLineHasColumnNames = true, IgnoreUnknownColumns = true }).FirstOrDefault();

                if (csvFileAccess != null)
                {
                    this.ConfigV3 = new BigCommerceConfig(csvFileAccess.ShortShopName, csvFileAccess.ClientId, csvFileAccess.ClientSecret, csvFileAccess.ApiKey);
                    this.credentialsFilePath = credentialsFilePath;
                    this.FeedFileName = csvFileAccess.FeedFileName;

                    this.MailObj = new EMailNotification(csvFileAccess.MailSMTPAddress, Convert.ToInt32(csvFileAccess.MailPort), csvFileAccess.FromEMail,
                                                    csvFileAccess.MailPassword, csvFileAccess.MailToAddress);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
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

                var cc = new LINQtoCSV.CsvContext();

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
        static string GetAdditionalImageLinks(List<BigCommerceImage> productNotThumbnailImagesList)
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
        /// Returns string in UTF-8 format.
        /// </summary>
        static string ToUTF8(string text)
        {
            return Encoding.UTF8.GetString(Encoding.Default.GetBytes(text));
        }

        /// <summary>
        /// Removes HTML from string with Regex.
        /// </summary>
        static string StripHTML(string source)
        {
            return HttpUtility.HtmlDecode(Regex.Replace(source, @"<[^>]+>|\n", "").Trim());
        }

        /// <summary>
        /// Gets Pinterest Sale Price.
        /// </summary>
        static string GetSalePrice(decimal? sale_price, decimal? price)
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
        static string GetAvailability(string bc_availability)
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
        static string formatCategoryName(string source)
        {
            TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
            string categoryNameInCaps = myTI.ToTitleCase(source);

            string catNameInCapsWithNoBeginEndSlash = categoryNameInCaps.Substring(1, categoryNameInCaps.Length - 2);

            string catNameFormatted = catNameInCapsWithNoBeginEndSlash.Replace("/", " > ");

            return catNameFormatted;
        }

        /// <summary>
        /// Saves Pinterest product's feed to local file.
        /// </summary>
        public void SaveProducts(List<BigCommerceProduct> productsList, List<BigCommerceCategory> bigCommerceCategories, List<CSV_CategoriesWithGoogleClassifications> catBCGoogleList, string pinterestCatalogCSVPath)
        {
            try
            {

                if (productsList is null)
                {
                    throw new ArgumentNullException(nameof(productsList));
                }

                if (bigCommerceCategories is null)
                {
                    throw new ArgumentNullException(nameof(bigCommerceCategories));
                }

                if (catBCGoogleList is null)
                {
                    throw new ArgumentNullException(nameof(catBCGoogleList));
                }

                if (string.IsNullOrEmpty(pinterestCatalogCSVPath))
                {
                    throw new ArgumentException($"'{nameof(pinterestCatalogCSVPath)}' cannot be null or empty.", nameof(pinterestCatalogCSVPath));
                }
                //Select only "physical" products that are visible 
                List<BigCommerceProduct> products = productsList.Where(x => x.ProductType == "physical" && x.IsProductVisible).ToList();

                string storeURL = GetStoreSafeURL();

                var prodRecords = new List<dynamic>();

                foreach (var item in products)
                {
                    dynamic product = new ExpandoObject();

                    product.id = item.Id;
                    product.title = item.Name;
                    product.description = ToUTF8(StripHTML(item.Description));
                    product.link = storeURL + item.Product_URL;
                    product.price = item.Price;
                    product.sale_price = GetSalePrice(item.SalePrice, item.Price);
                    product.availability = GetAvailability(item.Availability);
                    product.brand = item.BrandName;
                    product.condition = item.Condition.ToLower();
                    product.product_type = formatCategoryName(bigCommerceCategories.Where(n => n.Id == item.Categories[0]).FirstOrDefault().Category_URL.Url);
                    product.google_product_category = catBCGoogleList.Where(x => Int32.Parse(x.Id) == item.Categories[0]).FirstOrDefault().GoogleProductCategory;
                    product.image_link = item.ThumbnailImageURL.StandardUrl;
                    product.additional_image_link = GetAdditionalImageLinks(item.Main_Images.Where(x => (!x.IsThumbnail)).ToList());

                    prodRecords.Add(product);
                }

                using (var writer = new StreamWriter(pinterestCatalogCSVPath))
                using (var csv = new CsvWriter(writer, CultureInfo.CurrentCulture = new CultureInfo("en-US")))
                {
                    csv.WriteRecords(prodRecords);
                    csv.Flush();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Uploads BigCommerce products CSV file to the store's WebDav server.
        /// </summary>
        public async Task<bool> UploadBigCommerceCatalogAsync(string catalogFilePath)
        {
            try
            {
                var cc = new LINQtoCSV.CsvContext();

                var csvFileAccess = cc.Read<CSV_StoreCredentialParameters>(this.credentialsFilePath, new CsvFileDescription { FirstLineHasColumnNames = true, IgnoreUnknownColumns = true }).FirstOrDefault();

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
                        WebDavResponse result = await client.PutFile(csvFileAccess.WebDavPath + "/content/" + csvFileAccess.FeedFileName, File.OpenRead(catalogFilePath), "text/csv"); // upload resource

                        if (result.IsSuccessful)
                        {
                            return true;
                        }
                        else
                            throw new Exception("UploadBigCommerceCatalogAsync -> " + result.Description);
                    }
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