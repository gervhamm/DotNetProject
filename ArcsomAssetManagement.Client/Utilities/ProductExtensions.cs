using System.Diagnostics.CodeAnalysis;
using ArcsomAssetManagement.Client.Models;

namespace ArcsomAssetManagement.Client.Utilities;


/// <summary>
/// Product Model Extentions
/// </summary>
public static class ProductExtentions
{
    /// <summary>
    /// Check if the product is null or new.
    /// </summary>
    /// <param name="product"></param>
    /// <returns></returns>
    public static bool IsNullOrNew([NotNullWhen(false)] this Product? product)
    {
        return product is null || product.Id == 0;
    }
}
