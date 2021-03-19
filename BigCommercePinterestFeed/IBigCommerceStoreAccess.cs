using BigCommerceAccess.Models.Category;
using BigCommerceAccess.Models.Product;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BigCommercePinterestFeed
{
    public interface IBigCommerceStoreAccess
    {
        string CategoriesCSVPath { get; set; }
        string CredentialsFilePath { get; set; }
        string PinterestCatalogCSVPath { get; set; }
        
        List<BigCommerceCategory> GetCategories();
        List<CSV_CategoriesWithGoogleClassifications> GetCategoriesWithGoogleClassification();
        List<BigCommerceProduct> GetProducts();        
        string GetStoreDomain();
        string GetStoreName();
        string GetStoreSafeURL();
        bool IsBigCommerceWithGoogleCategoryFileLatest(List<CSV_CategoriesWithGoogleClassifications> catBCGoogleList, List<BigCommerceCategory> bcCategoriesList);
        void SaveProducts(string storeSafeURL, List<BigCommerceProduct> productsList, List<BigCommerceCategory> bigCommerceCategories, List<CSV_CategoriesWithGoogleClassifications> catBCGoogleList, string pinterestCatalogCSVPath);
        Task<bool> UploadBigCommerceCatalogAsync(string catalogFilePath);
    }
}