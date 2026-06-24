using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models.DTOs
{
    /// <summary>
    /// DTO for Book API operations.
    /// </summary>
    public class BookDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Author is required.")]
        [StringLength(150)]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "ISBN is required.")]
        [StringLength(20)]
        public string ISBN { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Category { get; set; }

        [Range(1000, 2100)]
        public int? PublishedYear { get; set; }

        [Range(0, int.MaxValue)]
        public int QuantityAvailable { get; set; } = 1;
    }
}
