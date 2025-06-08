using System.Diagnostics.CodeAnalysis;
using ArcsomAssetManagement.Client.Models;

namespace ArcsomAssetManagement.Client.Utilities;

/// <summary>  
/// Model Extensions  
/// </summary>  
public static class ModelExtensions
{
    /// <summary>  
    /// Check if the model is null or new.  
    /// </summary>  
    /// <typeparam name="T">The type of the product.</typeparam>  
    /// <param name="T">The model instance.</param>  
    /// <returns>True if the model is null or new; otherwise, false.</returns>  
    public static bool IsNullOrNew<T>([NotNullWhen(false)] this T? model) where T : class, IIdentifiable, new()
    {
        return model is null || model.Id == 0;
    }
}
