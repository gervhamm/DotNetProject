using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ArcsomAssetManagement.Api.Models;

[Index(nameof(Name), IsUnique = true)]
public class Manufacturer
{
    public ulong Id { get; set; }
    //[Display(ResourceType = typeof(Resources.Entities.Customer), Name = "Name")]
    //[Required(ErrorMessageResourceType = typeof(Resources.Entities.Customer), ErrorMessageResourceName = "Name_Required")]
    //[StringLength(50, MinimumLength = 3, ErrorMessageResourceType = typeof(Resources.Entities.Customer), ErrorMessageResourceName = "Name_Length")]
    //[RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessageResourceType = typeof(Resources.Entities.Customer), ErrorMessageResourceName = "Name_Regex")]
    public string Name { get; set; } = null!;

    //[Display(ResourceType = typeof(Resources.Entities.Customer), Name = "Contact")]
    //[EmailAddress(ErrorMessageResourceType = typeof(Resources.Entities.Customer), ErrorMessageResourceName = "Contact_InvalidEmail")]
    //[BindProperty]
    public string? Contact { get; set; }

    public ICollection<Product>? Products { get; set; }
}
