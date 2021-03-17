using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BigCommerceAccess.Models.Product;
using BigCommerceAccess.Models.Category;
using System.IO;
using System.Dynamic;
using CsvHelper;
using System.Globalization;

namespace OnInitiative.com_Pinterest_Feed_Generator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            StringBuilder sb = new StringBuilder();
            String pathFile = Directory.GetCurrentDirectory() + "\\PinterestCatalog_JobLog.txt";
            String dateAndTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            try
            {
                BCStoreAccess BCAccess = new BCStoreAccess();

                var storeName = BCAccess.GetStoreName();
                var storeDomain = BCAccess.GetStoreDomain();
                var storeSafeURL = BCAccess.GetStoreSafeURL();

                string bigcommerceCategoriesCSVPath = Directory.GetCurrentDirectory() + "\\BigcommerceCategoriesCSV.csv";
                string pinterestCatalogCSVPath = Directory.GetCurrentDirectory() + "\\products.csv";

                bool categoryFileExists = File.Exists(bigcommerceCategoriesCSVPath);

                //Get only BigCommerce categories that are visible
                List<BigCommerceCategory> categories = BCAccess.GetCategoriesV3().Where(x => x.IsVisible).ToList();

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

                //Checking integrity of BigCommerceCategoriesCSV file
                List<CSV_CategoriesWithGoogleClassifications> catBCGoogleList = BCAccess.GetCategoriesWithGoogleClassification();

                bool isBigcommerceCategoriesCSVCurrent = BCAccess.IsBigCommerceWithGoogleCategoryFileLatest(catBCGoogleList, categories);
                                
                if (!isBigcommerceCategoriesCSVCurrent)
                    throw new Exception("BigcommerceCategoriesCSV file is not current. Please update.");

                //Select only "physical" products that are visible 
                List<BigCommerceProduct> products = BCAccess.GetProductsV3().Where(x => x.ProductType == "physical" && x.IsProductVisible).ToList();

                var prodRecords = new List<dynamic>();

                foreach (var item in products)
                {
                    dynamic product = new ExpandoObject();

                    product.id = item.Id;
                    product.title = item.Name;
                    product.description = BCAccess.StripHTML(item.Description);
                    product.link = storeSafeURL + item.Product_URL;
                    product.price = item.Price;
                    product.sale_price = BCAccess.GetSalePrice(item.SalePrice, item.Price);
                    product.availability = BCAccess.GetAvailability(item.Availability);
                    product.brand = item.BrandName;
                    product.condition = item.Condition.ToLower();
                    product.product_type = BCAccess.formatCategoryName(categories.Where(n => n.Id == item.Categories[0]).FirstOrDefault().Category_URL.Url);
                    product.google_product_category = catBCGoogleList.Where(x => Int32.Parse(x.Id) == item.Categories[0]).FirstOrDefault().GoogleProductCategory;
                    product.image_link = item.ThumbnailImageURL.StandardUrl;
                    product.additional_image_link = BCAccess.GetAdditionalImageLinks(item.Main_Images.Where(x => (!x.IsThumbnail)).ToList());

                    prodRecords.Add(product);
                }

                using (var writer = new StreamWriter(pinterestCatalogCSVPath))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(prodRecords);
                    csv.Flush();
                }

               //Uploading BigCommerce products CSV file to WebDav Server
                bool uploadSuccessful = await BCAccess.UploadBigCommerceCatalogAsync(pinterestCatalogCSVPath);

                if (uploadSuccessful)
                {
                    sb.Append(dateAndTime + " - Job finished OK for store " + storeName + "\n");
                    File.AppendAllText(pathFile, sb.ToString());
                }
               
            }
            catch (Exception ex)
            {
                sb.Append(dateAndTime + " ERROR: " + ex.Message + "\n");
                File.AppendAllText(pathFile, sb.ToString());
            }
            finally
            {
               sb.Clear();                
            }
        }
    }
}
