namespace ArcsomAssetManagement.Api.Models;

public class Product
{
    public ulong Id { get; set; }
    /// <summary>
    /// Name of the product
    /// </summary>
    //[Display(ResourceType = typeof(Resources.Entities.Customer), Name = "Name")]
    //[Required(ErrorMessageResourceType = typeof(Resources.Entities.Customer), ErrorMessageResourceName = "Name_Required")]
    //[StringLength(50, MinimumLength = 3, ErrorMessageResourceType = typeof(Resources.Entities.Customer), ErrorMessageResourceName = "Name_Length")]
    //[RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessageResourceType = typeof(Resources.Entities.Customer), ErrorMessageResourceName = "Name_Regex")]
    public string Name { get; set; } = null!;
    public Manufacturer? Manufacturer { get; set; }

}
