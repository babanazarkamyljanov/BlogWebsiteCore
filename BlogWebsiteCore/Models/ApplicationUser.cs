using Microsoft.AspNetCore.Identity;

namespace BlogWebsiteCore.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}