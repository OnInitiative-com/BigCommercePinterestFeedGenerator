using BigCommerceAccess.Models.Category;
using BigCommerceAccess.Models.Product;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnInitiative.com_Pinterest_Feed_Generator
{
    public interface IBigCommerceStoreAccess
    {
        string CategoriesCSVPath { get; set; }
        string CredentialsFilePath { get; set; }
        string PinterestCatalogCSVPath { get; set; }

        string formatCategoryName(string source);
        string GetAdditionalImageLinks(List<BigCommerceImage> productNotThumbnailImagesList);
        string GetAvailability(string bc_availability);
        List<BigCommerceCategory> GetCategoriesV3();
        List<CSV_CategoriesWithGoogleClassifications> GetCategoriesWithGoogleClassification();
        List<BigCommerceProduct> GetProductsV3();
        string GetSalePrice(decimal? sale_price, decimal? price);
        string GetStoreDomain();
        string GetStoreName();
        string GetStoreSafeURL();
        bool IsBigCommerceWithGoogleCategoryFileLatest(List<CSV_CategoriesWithGoogleClassifications> catBCGoogleList, List<BigCommerceCategory> bcCategoriesList);
        string StripHTML(string source);
        Task<bool> UploadBigCommerceCatalogAsync(string catalogFilePath);
    }
}