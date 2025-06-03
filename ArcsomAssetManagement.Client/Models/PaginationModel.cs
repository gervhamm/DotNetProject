namespace ArcsomAssetManagement.Client.Models;

public class PaginationModel
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);

    public List<PageNumberItem> pageNumbers = new List<PageNumberItem> { new PageNumberItem { Number = "1", IsCurrent = true } };
}

public class PageNumberItem
{
    public string Number { get; set; }
    public bool IsCurrent { get; set; } = false;
}
