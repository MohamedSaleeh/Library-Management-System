using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    /// <summary>
    /// Handles CRUD operations for Book management.
    /// Admin-only access for create, edit, and delete operations.
    /// </summary>
    [Authorize]
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BooksController> _logger;

        public BooksController(ApplicationDbContext context, ILogger<BooksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ==================== LIST ====================

        /// <summary>
        /// Displays the list of all books.
        /// </summary>
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? search, string? category)
        {
            var query = _context.Books.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(b =>
                    b.Title.ToLower().Contains(searchLower) ||
                    b.Author.ToLower().Contains(searchLower) ||
                    b.ISBN.ToLower().Contains(searchLower));
            }

            // Apply category filter
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(b => b.Category == category);
            }

            // Get distinct categories for filter dropdown
            var categories = await _context.Books
                .Where(b => b.Category != null)
                .Select(b => b.Category)
                .Distinct()
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = category;

            var books = await query.OrderBy(b => b.Title).ToListAsync();
            return View(books);
        }

        // ==================== DETAILS ====================

        /// <summary>
        /// Displays details of a specific book.
        /// </summary>
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Loans)
                .ThenInclude(l => l.Member)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // ==================== CREATE ====================

        /// <summary>
        /// Displays the create book form (Admin only).
        /// </summary>
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Creates a new book (Admin only).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Title,Author,ISBN,Category,PublishedYear,QuantityAvailable")] Book book)
        {
            if (!ModelState.IsValid)
            {
                return View(book);
            }

            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Book created: {Title} by {Author}", book.Title, book.Author);

            TempData["SuccessMessage"] = $"Book '{book.Title}' has been created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ==================== EDIT ====================

        /// <summary>
        /// Displays the edit book form (Admin only).
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        /// <summary>
        /// Updates an existing book (Admin only).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,ISBN,Category,PublishedYear,QuantityAvailable")] Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(book);
            }

            try
            {
                _context.Update(book);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Book updated: ID {Id}", book.Id);

                TempData["SuccessMessage"] = $"Book '{book.Title}' has been updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(book.Id))
                {
                    return NotFound();
                }
                throw;
            }

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

            var book = await _context.Books
                .Include(b => b.Loans)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        /// <summary>
        /// Deletes a book (Admin only).
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Book deleted: ID {Id}", id);

                TempData["SuccessMessage"] = $"Book '{book.Title}' has been deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}
