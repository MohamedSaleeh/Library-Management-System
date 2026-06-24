using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LibraryManagementSystem.Models.ViewModels
{
    /// <summary>
    /// ViewModel for creating/editing a loan.
    /// </summary>
    public class LoanViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a book.")]
        [Display(Name = "Book")]
        public int BookId { get; set; }

        [Required(ErrorMessage = "Please select a member.")]
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

        public string Status { get; set; } = LoanStatus.Active;

        public List<SelectListItem>? BooksList { get; set; }
        public List<SelectListItem>? MembersList { get; set; }
    }

    /// <summary>
    /// ViewModel for returning a book.
    /// </summary>
    public class ReturnBookViewModel
    {
        public int LoanId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Return date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Return Date")]
        public DateTime ReturnDate { get; set; } = DateTime.Now;

        public decimal FineAmount { get; set; }
        public int DaysLate { get; set; }
    }
}
