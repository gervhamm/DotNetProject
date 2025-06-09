using SQLite;

namespace ArcsomAssetManagement.Client.DTOs.Business;

public class CashedUser
{
    [PrimaryKey]
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Salt { get; set; }
    public DateTime TokenExpiration { get; set; }
}
