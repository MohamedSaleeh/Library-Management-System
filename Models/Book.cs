using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    /// <summary>
    /// Represents a book in the library.
    /// </summary>
    public class Book
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Author is required.")]
        [StringLength(150, ErrorMessage = "Author cannot exceed 150 characters.")]
        [Display(Name = "Author")]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "ISBN is required.")]
        [StringLength(20, ErrorMessage = "ISBN cannot exceed 20 characters.")]
        [Display(Name = "ISBN")]
        public string ISBN { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters.")]
        [Display(Name = "Category")]
        public string? Category { get; set; }

        [Range(1000, 2100, ErrorMessage = "Published year must be between 1000 and 2100.")]
        [Display(Name = "Published Year")]
        public int? PublishedYear { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative number.")]
        [Display(Name = "Quantity Available")]
        public int QuantityAvailable { get; set; } = 1;

        // Navigation property - One Book can have many Loans
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}
