using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Dynamic;
using CsvHelper;
using System.Globalization;
using BigCommerceAccess.Models.Category;
using BigCommercePinterestFeed;

namespace TestProject
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
                BigCommerceStoreAccess BCAccess = new BigCommerceStoreAccess();

                //Config initial paramaters
                BCAccess.CredentialsFilePath = Directory.GetCurrentDirectory() + "\\BigCommerceCredentials.csv";
                BCAccess.CategoriesCSVPath = Directory.GetCurrentDirectory() + "\\BigCommerceCategoriesCSV.csv";
                BCAccess.PinterestCatalogCSVPath = Directory.GetCurrentDirectory() + "\\products.csv";

                var storeName = BCAccess.GetStoreName();
                var storeDomain = BCAccess.GetStoreDomain();
                var storeSafeURL = BCAccess.GetStoreSafeURL();

                //Get only BigCommerce categories that are visible
                List<BigCommerceCategory> categories = BCAccess.GetCategories().Where(x => x.IsVisible).ToList();

                //Check if categories file has already been generated
                bool categoryFileExists = File.Exists(BCAccess.CategoriesCSVPath);

                //Create BigCommerce category file first time
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

                    using (var writer = new StreamWriter(BCAccess.CategoriesCSVPath))
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

                //Reading BigCommerce categories file with Google classification.
                List<CSV_CategoriesWithGoogleClassifications> catBCGoogleList = BCAccess.GetCategoriesWithGoogleClassification();

                //Checking integrity of BigCommerceCategoriesCSV file
                bool isBigcommerceCategoriesCSVCurrent = BCAccess.IsBigCommerceWithGoogleCategoryFileLatest(catBCGoogleList, categories);

                if (!isBigcommerceCategoriesCSVCurrent)
                    throw new Exception("BigcommerceCategoriesCSV file is not current. Please update.");
                
                //Saving generated Pinterest products catalog to local file
                BCAccess.SaveProducts(storeName, BCAccess.GetProducts(), categories, catBCGoogleList, BCAccess.PinterestCatalogCSVPath);

                //Uploading BigCommerce products CSV file to WebDav Server
                bool uploadSuccessful = await BCAccess.UploadBigCommerceCatalogAsync(BCAccess.PinterestCatalogCSVPath);

                if (uploadSuccessful)
                {
                    sb.Append(dateAndTime + " - BigCommerce's Pinterest catalog exported OK for store " + storeName + "\n");
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
