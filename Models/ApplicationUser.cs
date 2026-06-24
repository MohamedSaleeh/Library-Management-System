using Microsoft.AspNetCore.Identity;

namespace LibraryManagementSystem.Models
{
    /// <summary>
    /// Custom application user that extends ASP.NET Identity User.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string FullName { get; set; } = string.Empty;

        [PersonalData]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
    }
}
