using ArcsomAssetManagement.Client.Models;

namespace ArcsomAssetManagement.Client.PageModels.Helpers;

public static class PaginationHelper
{
    public static List<PageNumberItem> SetPagenumbers(int currentPage, int totalPages)
    {
        List<PageNumberItem> pageNumbers = new List<PageNumberItem>();

        if (currentPage > 1)
        {
            pageNumbers.Add(new PageNumberItem { Number = "Previous" });
        }

        var startPage = Math.Max(currentPage - 2, 1);
        var endPage = Math.Min(currentPage + 2, totalPages);

        for (int i = startPage; i <= endPage; i++)
        {
            if (i == currentPage)
            {
                pageNumbers.Add(new PageNumberItem { Number = i.ToString(), IsCurrent = true });
            }
            else
            {
                pageNumbers.Add(new PageNumberItem { Number = i.ToString(), IsCurrent = false });
            }
        }

        if (currentPage < totalPages)
        {
            pageNumbers.Add(new PageNumberItem { Number = "Next" });
        }

        return pageNumbers;
    }
}
