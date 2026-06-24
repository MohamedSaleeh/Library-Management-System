using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    /// <summary>
    /// Represents a library member who can borrow books.
    /// </summary>
    public class Member
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(150, ErrorMessage = "Full Name cannot exceed 150 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number.")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [StringLength(300, ErrorMessage = "Address cannot exceed 300 characters.")]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Display(Name = "Registration Date")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        // Navigation property - One Member can have many Loans
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}
