using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    /// <summary>
    /// Handles CRUD operations for Member management.
    /// Admin-only access for create, edit, and delete operations.
    /// </summary>
    [Authorize]
    public class MembersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MembersController> _logger;

        public MembersController(ApplicationDbContext context, ILogger<MembersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ==================== LIST ====================

        /// <summary>
        /// Displays the list of all members.
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Members.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(m =>
                    m.FullName.ToLower().Contains(searchLower) ||
                    m.Email.ToLower().Contains(searchLower) ||
                    (m.PhoneNumber != null && m.PhoneNumber.Contains(search)));
            }

            ViewBag.CurrentSearch = search;
            var members = await query.OrderBy(m => m.FullName).ToListAsync();
            return View(members);
        }

        // ==================== DETAILS ====================

        /// <summary>
        /// Displays details of a specific member.
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .Include(m => m.Loans)
                .ThenInclude(l => l.Book)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // ==================== CREATE ====================

        /// <summary>
        /// Displays the create member form (Admin only).
        /// </summary>
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Creates a new member (Admin only).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("FullName,Email,PhoneNumber,Address")] Member member)
        {
            if (!ModelState.IsValid)
            {
                return View(member);
            }

            member.RegistrationDate = DateTime.Now;
            _context.Members.Add(member);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Member created: {FullName}", member.FullName);

            TempData["SuccessMessage"] = $"Member '{member.FullName}' has been created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ==================== EDIT ====================

        /// <summary>
        /// Displays the edit member form (Admin only).
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        /// <summary>
        /// Updates an existing member (Admin only).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Email,PhoneNumber,Address,RegistrationDate")] Member member)
        {
            if (id != member.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(member);
            }

            try
            {
                _context.Update(member);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Member updated: ID {Id}", member.Id);

                TempData["SuccessMessage"] = $"Member '{member.FullName}' has been updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemberExists(member.Id))
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

            var member = await _context.Members
                .Include(m => m.Loans)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        /// <summary>
        /// Deletes a member (Admin only).
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member != null)
            {
                _context.Members.Remove(member);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Member deleted: ID {Id}", id);

                TempData["SuccessMessage"] = $"Member '{member.FullName}' has been deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.Id == id);
        }
    }
}
