using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Models.DTOs;
using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.ApiControllers
{
    /// <summary>
    /// RESTful API Controller for Loan operations.
    /// Provides CRUD endpoints with borrow/return functionality.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LoansApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly LoanService _loanService;

        public LoansApiController(ApplicationDbContext context, LoanService loanService)
        {
            _context = context;
            _loanService = loanService;
        }

        /// <summary>
        /// GET: api/LoansApi
        /// Retrieves all loans with optional status filter.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoanDto>>> GetLoans([FromQuery] string? status)
        {
            // Update overdue statuses
            await _loanService.UpdateOverdueLoansAsync();

            var query = _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(l => l.Status == status);
            }

            var loans = await query
                .OrderByDescending(l => l.BorrowDate)
                .Select(l => new LoanDto
                {
                    Id = l.Id,
                    BookId = l.BookId,
                    MemberId = l.MemberId,
                    BookTitle = l.Book != null ? l.Book.Title : null,
                    MemberName = l.Member != null ? l.Member.FullName : null,
                    BorrowDate = l.BorrowDate,
                    DueDate = l.DueDate,
                    ReturnDate = l.ReturnDate,
                    FineAmount = l.FineAmount,
                    Status = l.Status
                })
                .ToListAsync();

            return Ok(loans);
        }

        /// <summary>
        /// GET: api/LoansApi/5
        /// Retrieves a specific loan by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<LoanDto>> GetLoan(int id)
        {
            var loan = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null)
            {
                return NotFound(new { message = $"Loan with ID {id} not found." });
            }

            // Recalculate fine
            loan.CalculateFine();

            var loanDto = new LoanDto
            {
                Id = loan.Id,
                BookId = loan.BookId,
                MemberId = loan.MemberId,
                BookTitle = loan.Book?.Title,
                MemberName = loan.Member?.FullName,
                BorrowDate = loan.BorrowDate,
                DueDate = loan.DueDate,
                ReturnDate = loan.ReturnDate,
                FineAmount = loan.FineAmount,
                Status = loan.Status
            };

            return Ok(loanDto);
        }

        /// <summary>
        /// POST: api/LoansApi
        /// Creates a new loan (borrows a book).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<LoanDto>> PostLoan(LoanDto loanDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, message, loan) = await _loanService.BorrowBookAsync(
                loanDto.BookId,
                loanDto.MemberId,
                loanDto.BorrowDate,
                loanDto.DueDate);

            if (!success)
            {
                return BadRequest(new { message });
            }

            loanDto.Id = loan!.Id;
            loanDto.Status = loan.Status;

            return CreatedAtAction(nameof(GetLoan), new { id = loan.Id }, loanDto);
        }

        /// <summary>
        /// PUT: api/LoansApi/5
        /// Updates an existing loan.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutLoan(int id, LoanDto loanDto)
        {
            if (id != loanDto.Id)
            {
                return BadRequest(new { message = "ID mismatch." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var loan = await _context.Loans.FindAsync(id);
            if (loan == null)
            {
                return NotFound(new { message = $"Loan with ID {id} not found." });
            }

            loan.BookId = loanDto.BookId;
            loan.MemberId = loanDto.MemberId;
            loan.BorrowDate = loanDto.BorrowDate;
            loan.DueDate = loanDto.DueDate;
            loan.ReturnDate = loanDto.ReturnDate;
            loan.Status = loanDto.Status;

            loan.CalculateFine();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoanExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// POST: api/LoansApi/5/return
        /// Returns a book for a specific loan.
        /// </summary>
        [HttpPost("{id}/return")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> ReturnBook(int id, [FromBody] ReturnLoanDto? returnDto)
        {
            var returnDate = returnDto?.ReturnDate ?? DateTime.Now;

            var (success, message, loan) = await _loanService.ReturnBookAsync(id, returnDate);

            if (!success)
            {
                return BadRequest(new { message });
            }

            return Ok(new
            {
                message = "Book returned successfully.",
                loanId = loan!.Id,
                fineAmount = loan.FineAmount,
                status = loan.Status
            });
        }

        /// <summary>
        /// DELETE: api/LoansApi/5
        /// Deletes a loan.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteLoan(int id)
        {
            var loan = await _context.Loans
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null)
            {
                return NotFound(new { message = $"Loan with ID {id} not found." });
            }

            // If book was not returned, restore quantity
            if (!loan.ReturnDate.HasValue && loan.Book != null)
            {
                loan.Book.QuantityAvailable++;
            }

            _context.Loans.Remove(loan);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// GET: api/LoansApi/overdue
        /// Retrieves all overdue loans.
        /// </summary>
        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<LoanDto>>> GetOverdueLoans()
        {
            await _loanService.UpdateOverdueLoansAsync();

            var overdueLoans = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .Where(l => l.Status == LoanStatus.Overdue)
                .Select(l => new LoanDto
                {
                    Id = l.Id,
                    BookId = l.BookId,
                    MemberId = l.MemberId,
                    BookTitle = l.Book != null ? l.Book.Title : null,
                    MemberName = l.Member != null ? l.Member.FullName : null,
                    BorrowDate = l.BorrowDate,
                    DueDate = l.DueDate,
                    ReturnDate = l.ReturnDate,
                    FineAmount = l.FineAmount,
                    Status = l.Status
                })
                .ToListAsync();

            return Ok(overdueLoans);
        }

        private bool LoanExists(int id)
        {
            return _context.Loans.Any(e => e.Id == id);
        }
    }
}
