using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Models.ViewModels;
using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    /// <summary>
    /// Handles CRUD operations for Loan management,
    /// including borrowing and returning books with fine calculation.
    /// </summary>
    [Authorize]
    public class LoansController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly LoanService _loanService;
        private readonly ILogger<LoansController> _logger;

        public LoansController(
            ApplicationDbContext context,
            LoanService loanService,
            ILogger<LoansController> logger)
        {
            _context = context;
            _loanService = loanService;
            _logger = logger;
        }

        // ==================== LIST ====================

        /// <summary>
        /// Displays the list of all loans with optional filtering by status.
        /// </summary>
        public async Task<IActionResult> Index(string? status)
        {
            // Update overdue statuses before displaying
            await _loanService.UpdateOverdueLoansAsync();

            var query = _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .AsQueryable();

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(l => l.Status == status);
            }

            ViewBag.CurrentStatus = status;
            ViewBag.StatusList = new List<string> { LoanStatus.Active, LoanStatus.Returned, LoanStatus.Overdue };

            var loans = await query.OrderByDescending(l => l.BorrowDate).ToListAsync();
            return View(loans);
        }

        // ==================== DETAILS ====================

        /// <summary>
        /// Displays details of a specific loan.
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null)
            {
                return NotFound();
            }

            // Recalculate fine
            loan.CalculateFine();

            return View(loan);
        }

        // ==================== CREATE / BORROW ====================

        /// <summary>
        /// Displays the create loan (borrow book) form.
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var viewModel = new LoanViewModel
            {
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14),
                BooksList = await GetAvailableBooksSelectList(),
                MembersList = await GetMembersSelectList()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Creates a new loan (borrows a book).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(LoanViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.BooksList = await GetAvailableBooksSelectList();
                viewModel.MembersList = await GetMembersSelectList();
                return View(viewModel);
            }

            var (success, message, loan) = await _loanService.BorrowBookAsync(
                viewModel.BookId,
                viewModel.MemberId,
                viewModel.BorrowDate,
                viewModel.DueDate);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                viewModel.BooksList = await GetAvailableBooksSelectList();
                viewModel.MembersList = await GetMembersSelectList();
                return View(viewModel);
            }

            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        // ==================== EDIT ====================

        /// <summary>
        /// Displays the edit loan form (Admin only).
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan = await _context.Loans.FindAsync(id);
            if (loan == null)
            {
                return NotFound();
            }

            var viewModel = new LoanViewModel
            {
                Id = loan.Id,
                BookId = loan.BookId,
                MemberId = loan.MemberId,
                BorrowDate = loan.BorrowDate,
                DueDate = loan.DueDate,
                ReturnDate = loan.ReturnDate,
                Status = loan.Status,
                BooksList = await GetAllBooksSelectList(),
                MembersList = await GetMembersSelectList()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Updates an existing loan (Admin only).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, LoanViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                viewModel.BooksList = await GetAllBooksSelectList();
                viewModel.MembersList = await GetMembersSelectList();
                return View(viewModel);
            }

            try
            {
                var loan = await _context.Loans.FindAsync(id);
                if (loan == null)
                {
                    return NotFound();
                }

                loan.BookId = viewModel.BookId;
                loan.MemberId = viewModel.MemberId;
                loan.BorrowDate = viewModel.BorrowDate;
                loan.DueDate = viewModel.DueDate;
                loan.ReturnDate = viewModel.ReturnDate;
                loan.Status = viewModel.Status;

                loan.CalculateFine();

                _context.Update(loan);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Loan updated: ID {Id}", loan.Id);

                TempData["SuccessMessage"] = $"Loan #{loan.Id} has been updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoanExists(viewModel.Id))
                {
                    return NotFound();
                }
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // ==================== RETURN BOOK ====================

        /// <summary>
        /// Displays the return book form.
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Return(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null)
            {
                return NotFound();
            }

            if (loan.ReturnDate.HasValue)
            {
                TempData["ErrorMessage"] = "This book has already been returned.";
                return RedirectToAction(nameof(Details), new { id = loan.Id });
            }

            // Calculate potential fine
            var checkDate = DateTime.Now;
            int daysLate = 0;
            decimal fineAmount = 0;

            if (checkDate > loan.DueDate)
            {
                daysLate = (checkDate - loan.DueDate).Days;
                fineAmount = daysLate * 1.00m;
            }

            var viewModel = new ReturnBookViewModel
            {
                LoanId = loan.Id,
                BookTitle = loan.Book?.Title ?? "Unknown",
                MemberName = loan.Member?.FullName ?? "Unknown",
                BorrowDate = loan.BorrowDate,
                DueDate = loan.DueDate,
                ReturnDate = DateTime.Now,
                FineAmount = fineAmount,
                DaysLate = daysLate
            };

            return View(viewModel);
        }

        /// <summary>
        /// Processes the book return.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Return(ReturnBookViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var (success, message, loan) = await _loanService.ReturnBookAsync(
                viewModel.LoanId,
                viewModel.ReturnDate);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(viewModel);
            }

            TempData["SuccessMessage"] = $"Book returned successfully. Fine amount: ${loan?.FineAmount:F2}";
            return RedirectToAction(nameof(Index));
        }

        // ==================== DELETE ====================

        /// <summary>
        /// Displays the delete confirmation page (Admin only).
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null)
            {
                return NotFound();
            }

            return View(loan);
        }

        /// <summary>
        /// Deletes a loan (Admin only).
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loan = await _context.Loans
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan != null)
            {
                // If book was not returned, restore quantity
                if (!loan.ReturnDate.HasValue && loan.Book != null)
                {
                    loan.Book.QuantityAvailable++;
                }

                _context.Loans.Remove(loan);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Loan deleted: ID {Id}", id);

                TempData["SuccessMessage"] = $"Loan #{id} has been deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ==================== HELPER METHODS ====================

        private bool LoanExists(int id)
        {
            return _context.Loans.Any(e => e.Id == id);
        }

        /// <summary>
        /// Gets a SelectList of books with available quantity.
        /// </summary>
        private async Task<List<SelectListItem>> GetAvailableBooksSelectList()
        {
            return await _context.Books
                .Where(b => b.QuantityAvailable > 0)
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = $"{b.Title} by {b.Author} (Available: {b.QuantityAvailable})"
                })
                .ToListAsync();
        }

        /// <summary>
        /// Gets a SelectList of all books.
        /// </summary>
        private async Task<List<SelectListItem>> GetAllBooksSelectList()
        {
            return await _context.Books
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = $"{b.Title} by {b.Author}"
                })
                .ToListAsync();
        }

        /// <summary>
        /// Gets a SelectList of all members.
        /// </summary>
        private async Task<List<SelectListItem>> GetMembersSelectList()
        {
            return await _context.Members
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = $"{m.FullName} ({m.Email})"
                })
                .ToListAsync();
        }
    }
}
