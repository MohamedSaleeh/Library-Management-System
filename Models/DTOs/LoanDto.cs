using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models.DTOs
{
    /// <summary>
    /// DTO for Loan API operations.
    /// </summary>
    public class LoanDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "BookId is required.")]
        public int BookId { get; set; }

        [Required(ErrorMessage = "MemberId is required.")]
        public int MemberId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime BorrowDate { get; set; } = DateTime.Now;

        [Required]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(14);

        [DataType(DataType.Date)]
        public DateTime? ReturnDate { get; set; }

        public decimal FineAmount { get; set; }

        public string Status { get; set; } = LoanStatus.Active;

        // Related data
        public string? BookTitle { get; set; }
        public string? MemberName { get; set; }
    }

    /// <summary>
    /// DTO for returning a book via API.
    /// </summary>
    public class ReturnLoanDto
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime ReturnDate { get; set; } = DateTime.Now;
    }
}
