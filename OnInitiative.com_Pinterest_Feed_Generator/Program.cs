using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BigCommerceAccess;
using BigCommerceAccess.Misc;
using BigCommerceAccess.Models.Configuration;
using BigCommerceAccess.Models.Product;
using BigCommerceAccess.Models.Category;
using LINQtoCSV;
using Netco.Logging;
using NUnit.Framework;
using System.IO;
using System.Dynamic;
using CsvHelper;
using System.Globalization;
using static System.Net.Mime.MediaTypeNames;
using CsvContext = CsvHelper.CsvContext;
using CsvHelper.Configuration;

namespace OnInitiative.com_Pinterest_Feed_Generator
{
    class Program
    {

        static void Main(string[] args)
        {
            StringBuilder sb = new StringBuilder();
            String pathFile = Directory.GetCurrentDirectory() + "\\PinterestCatalog_JobLog.txt";
            String dateAndTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            try
            {
                BCStoreAccess obj = new BCStoreAccess();

                var storeName = obj.GetStoreName();
                var storeDomain = obj.GetStoreDomain();
                var storeSafeURL = obj.GetStoreSafeURL();

                string bigcommerceCategoriesCSVPath = Directory.GetCurrentDirectory() + "\\BigcommerceCategoriesCSV.csv";

                bool categoryFileExists = File.Exists(bigcommerceCategoriesCSVPath);

                //Get only BigCommerce categories that are visible
                List<BigCommerceCategory> categories = obj.GetCategoriesV3().Where(x => x.IsVisible).ToList();

                if (!categoryFileExists)
                {
                    var catRecords = new List<dynamic>();

                    foreach (var item in categories)
                    {
                        dynamic category = new ExpandoObject();

                        category.Id = item.Id;
                        category.Name = item.Category_Name;
                        category.Url = item.Category_URL.Url;
                        category.GoogleProductCategory = "0";

                        catRecords.Add(category);
                    }

                    using (var writer = new StreamWriter(bigcommerceCategoriesCSVPath))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(catRecords);
                    }

                    Console.WriteLine("Store Name: " + storeName);
                    Console.WriteLine("Store URL: " + storeSafeURL);
                    Console.WriteLine("");
                    Console.WriteLine("---------------");
                    Console.WriteLine("");
                    Console.WriteLine("BigCommerce Categories file was created successfully. Open the created BigcommerceCategoriesCSV.csv file and proceed to fill out the Google Product Category column");
                    Console.WriteLine("");
                    Console.WriteLine("---------------");
                    Console.WriteLine("");
                    Console.Write("Press Enter to exit now...");
                    Console.ReadLine();
                    Environment.Exit(0); // --> Breaking the execution at this point.
                }

                //Checking integrity of BigcommerceCategoriesCSV file
                List<CSV_CategoriesWithGoogleClassifications> catBCGoogleList = obj.GetCategoriesWithGoogleClassification();

                bool isBigcommerceCategoriesCSVCurrent = obj.IsBigCommerceCategoryFileLatest(catBCGoogleList, categories);

                //If BigcommerceCategoriesCSV file is current, proceed to export BigCommerce catalog to CSV file for Pinterest ingestion.
                if (!isBigcommerceCategoriesCSVCurrent)
                    throw new Exception("BigcommerceCategoriesCSV file is not current. Please update.");

                List<BigCommerceProduct> products = obj.GetProductsV3();

                var prodRecords = new List<dynamic>();

                foreach (var item in products)
                {
                    if (item.IsVisible && item.Type == "physical")
                    {
                        dynamic product = new ExpandoObject();

                        product.id = item.Id;
                        product.title = item.Name;
                        product.description = obj.StripHTML(item.Description);
                        product.link = storeSafeURL + item.Product_URL;
                        product.price = item.RetailPrice;
                        product.sale_price = obj.GetSalePrice(item.SalePrice, item.RetailPrice);
                        product.availability = obj.GetAvailability(item.Availability);
                        product.brand = item.BrandName;
                        product.condition = item.Condition.ToLower();
                        product.product_type = obj.formatCategoryName(categories.Where(n => n.Id == item.Categories[0]).FirstOrDefault().Category_URL.Url);
                        product.google_product_category = catBCGoogleList.Where(x => Int32.Parse(x.Id) == item.Categories[0]).FirstOrDefault().GoogleProductCategory;

                        product.GoogleProductCategory = "0";
                        prodRecords.Add(product);
                    }
                }

                using (var writer = new StreamWriter(bigcommerceCategoriesCSVPath))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(prodRecords);
                }

                Console.Read();

            }
            catch (Exception ex)
            {
                sb.Append(dateAndTime + " ERROR: " + ex.Message);
                File.AppendAllText(pathFile, sb.ToString());
            }

        }
    }
}
