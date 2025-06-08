using System.Diagnostics.CodeAnalysis;
using ArcsomAssetManagement.Client.Models;

namespace ArcsomAssetManagement.Client.Utilities;


/// <summary>
/// Manufacturer Model Extentions
/// </summary>
public static class ManufacturerExtentions
{
    /// <summary>
    /// Check if the manufacturer is null or new.
    /// </summary>
    /// <param name="manufacturer"></param>
    /// <returns></returns>
    public static bool IsNullOrNew([NotNullWhen(false)] this Manufacturer? manufacturer)
    {
        return manufacturer is null || manufacturer.Id == 0;
    }
}
