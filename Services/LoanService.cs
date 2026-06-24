using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Services
{
    /// <summary>
    /// Service that handles library business logic for loans,
    /// including borrowing books, returning books, and fine calculation.
    /// </summary>
    public class LoanService
    {
        private readonly ApplicationDbContext _context;

        public LoanService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Borrows a book: verifies quantity, creates loan, and decreases available quantity.
        /// </summary>
        public async Task<(bool success, string message, Loan? loan)> BorrowBookAsync(int bookId, int memberId, DateTime borrowDate, DateTime dueDate)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                return (false, "Book not found.", null);
            }

            var member = await _context.Members.FindAsync(memberId);
            if (member == null)
            {
                return (false, "Member not found.", null);
            }

            // Verify quantity is available
            if (book.QuantityAvailable <= 0)
            {
                return (false, "This book is not available for borrowing.", null);
            }

            // Create the loan
            var loan = new Loan
            {
                BookId = bookId,
                MemberId = memberId,
                BorrowDate = borrowDate,
                DueDate = dueDate,
                Status = LoanStatus.Active
            };

            // Decrease available quantity
            book.QuantityAvailable--;

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            return (true, "Book borrowed successfully.", loan);
        }

        /// <summary>
        /// Returns a book: sets return date, increases available quantity, and calculates fine.
        /// </summary>
        public async Task<(bool success, string message, Loan? loan)> ReturnBookAsync(int loanId, DateTime returnDate)
        {
            var loan = await _context.Loans
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.Id == loanId);

            if (loan == null)
            {
                return (false, "Loan not found.", null);
            }

            if (loan.ReturnDate.HasValue)
            {
                return (false, "This book has already been returned.", null);
            }

            // Set return date
            loan.ReturnDate = returnDate;

            // Calculate fine
            loan.CalculateFine();

            // Update status
            loan.UpdateStatus();

            // Increase available quantity
            if (loan.Book != null)
            {
                loan.Book.QuantityAvailable++;
            }

            await _context.SaveChangesAsync();

            return (true, "Book returned successfully.", loan);
        }

        /// <summary>
        /// Updates overdue loan statuses based on current date.
        /// </summary>
        public async Task UpdateOverdueLoansAsync()
        {
            var overdueLoans = await _context.Loans
                .Where(l => !l.ReturnDate.HasValue && l.DueDate < DateTime.Now && l.Status != LoanStatus.Overdue)
                .ToListAsync();

            foreach (var loan in overdueLoans)
            {
                loan.UpdateStatus();
                loan.CalculateFine();
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Gets the count of active loans.
        /// </summary>
        public async Task<int> GetActiveLoansCountAsync()
        {
            return await _context.Loans
                .Where(l => l.Status == LoanStatus.Active)
                .CountAsync();
        }

        /// <summary>
        /// Gets the count of returned loans.
        /// </summary>
        public async Task<int> GetReturnedLoansCountAsync()
        {
            return await _context.Loans
                .Where(l => l.Status == LoanStatus.Returned)
                .CountAsync();
        }

        /// <summary>
        /// Gets the count of overdue loans.
        /// </summary>
        public async Task<int> GetOverdueLoansCountAsync()
        {
            return await _context.Loans
                .Where(l => l.Status == LoanStatus.Overdue)
                .CountAsync();
        }

        /// <summary>
        /// Gets the total fines amount.
        /// </summary>
        public async Task<decimal> GetTotalFinesAsync()
        {
            return await _context.Loans
                .SumAsync(l => l.FineAmount);
        }
    }
}
