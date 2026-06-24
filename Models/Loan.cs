using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    /// <summary>
    /// Represents a loan record for borrowing a book.
    /// </summary>
    public class Loan
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Book is required.")]
        [Display(Name = "Book")]
        public int BookId { get; set; }

        [Required(ErrorMessage = "Member is required.")]
        [Display(Name = "Member")]
        public int MemberId { get; set; }

        [Required(ErrorMessage = "Borrow date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Borrow Date")]
        public DateTime BorrowDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Due date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(14);

        [DataType(DataType.Date)]
        [Display(Name = "Return Date")]
        public DateTime? ReturnDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Fine amount must be a non-negative value.")]
        [Display(Name = "Fine Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal FineAmount { get; set; } = 0;

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = LoanStatus.Active;

        // Navigation properties
        [ForeignKey("BookId")]
        public Book? Book { get; set; }

        [ForeignKey("MemberId")]
        public Member? Member { get; set; }

        /// <summary>
        /// Calculates the fine based on the return date and due date.
        /// Fine = $1 per day late.
        /// </summary>
        public void CalculateFine()
        {
            DateTime checkDate = ReturnDate ?? DateTime.Now;
            if (checkDate > DueDate)
            {
                int daysLate = (checkDate - DueDate).Days;
                FineAmount = daysLate * 1.00m; // $1 per day
            }
            else
            {
                FineAmount = 0;
            }
        }

        /// <summary>
        /// Updates the status based on the loan's current state.
        /// </summary>
        public void UpdateStatus()
        {
            if (ReturnDate.HasValue)
            {
                Status = LoanStatus.Returned;
            }
            else if (DateTime.Now > DueDate)
            {
                Status = LoanStatus.Overdue;
            }
            else
            {
                Status = LoanStatus.Active;
            }
        }
    }

    /// <summary>
    /// Constants for loan status values.
    /// </summary>
    public static class LoanStatus
    {
        public const string Active = "Active";
        public const string Returned = "Returned";
        public const string Overdue = "Overdue";
    }
}
