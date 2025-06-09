
namespace ArcsomAssetManagement.Client.Data
{
    internal class CachedUser
    {
        public string Username { get; set; }
        public Task<string> HashedPassword { get; set; }
        public string Token { get; set; }
        public DateTime TokenExpiration { get; set; }
    }
}