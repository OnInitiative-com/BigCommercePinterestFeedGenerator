using BigCommerceAccess.Models.Category;
using BigCommerceAccess.Models.Product;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BigCommercePinterestFeed
{
    /// <summary>
    /// Defines methods to manipulate the BigCommerce store.
    /// </summary>
    public interface IBigCommerceStoreAccess
    {
        /// <summary>
        /// Implements the path for the BigCommerce categories file.
        /// </summary>
        string CategoriesCSVPath { get; set; }

        /// <summary>
        /// Implements the path for the Pinterest catalog feed file.
        /// </summary>
        string PinterestCatalogCSVPath { get; set; }

        /// <summary>
        /// Implements how to get the categories from the BigCommerce store.
        /// </summary>
        List<BigCommerceCategory> GetCategories();

        /// <summary>
        /// Implements how to get the categories with Google classification in it.
        /// </summary>
        List<CSV_CategoriesWithGoogleClassifications> GetCategoriesWithGoogleClassification();

        /// <summary>
        /// Implements how to get the products from the BigCommerce store.
        /// </summary>
        List<BigCommerceProduct> GetProducts();

        /// <summary>
        /// Implements how to get the BigCommerce store domain.
        /// </summary>
        string GetStoreDomain();

        /// <summary>
        /// Implements how to get the BigCommerce store name.
        /// </summary>
        string GetStoreName();

        /// <summary>
        /// Implements how to get the BigCommerce store safe URL.
        /// </summary>
        string GetStoreSafeURL();

        /// <summary>
        /// Implements whether the categories with Google classification file is synced with the 
        /// current categories within the BigCommerce store.
        /// </summary>
        bool IsBigCommerceWithGoogleCategoryFileLatest(List<CSV_CategoriesWithGoogleClassifications> catBCGoogleList, List<BigCommerceCategory> bcCategoriesList);

        /// <summary>
        /// Implements how to save the products catalog feed to a local file.
        /// </summary>
        void SaveProducts(List<BigCommerceProduct> productsList, List<BigCommerceCategory> bigCommerceCategories, List<CSV_CategoriesWithGoogleClassifications> catBCGoogleList, string pinterestCatalogCSVPath);

        /// <summary>
        /// Implements how to upload the products catalog feed to the BigCommerce WebDav server.
        /// </summary>
        Task<bool> UploadBigCommerceCatalogAsync(string catalogFilePath);
    }
}