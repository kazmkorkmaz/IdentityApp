using Microsoft.AspNetCore.Identity;

namespace IdentityAPI.Models
{
    public class AppUser : IdentityUser<int>
    {
        public string FirsName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}