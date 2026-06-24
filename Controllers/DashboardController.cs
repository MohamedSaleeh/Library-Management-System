using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Models.ViewModels;
using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    /// <summary>
    /// Admin dashboard controller displaying library statistics and overview.
    /// </summary>
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly LoanService _loanService;
        private readonly ReminderService _reminderService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            ApplicationDbContext context,
            LoanService loanService,
            ReminderService reminderService,
            ILogger<DashboardController> logger)
        {
            _context = context;
            _loanService = loanService;
            _reminderService = reminderService;
            _logger = logger;
        }

        /// <summary>
        /// Displays the main dashboard with library statistics.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Update overdue loan statuses
            await _loanService.UpdateOverdueLoansAsync();

            // Generate overdue reminders
            var overdueReminders = await _reminderService.GenerateOverdueRemindersAsync();

            // Build dashboard view model
            var dashboard = new DashboardViewModel
            {
                TotalBooks = await _context.Books.CountAsync(),
                TotalMembers = await _context.Members.CountAsync(),
                TotalLoans = await _context.Loans.CountAsync(),
                OverdueLoans = await _loanService.GetOverdueLoansCountAsync(),
                ActiveLoans = await _loanService.GetActiveLoansCountAsync(),
                ReturnedLoans = await _loanService.GetReturnedLoansCountAsync(),
                TotalFines = await _loanService.GetTotalFinesAsync(),
                RecentLoans = await _context.Loans
                    .Include(l => l.Book)
                    .Include(l => l.Member)
                    .OrderByDescending(l => l.BorrowDate)
                    .Take(5)
                    .ToListAsync(),
                OverdueLoanList = await _context.Loans
                    .Include(l => l.Book)
                    .Include(l => l.Member)
                    .Where(l => l.Status == LoanStatus.Overdue)
                    .OrderBy(l => l.DueDate)
                    .ToListAsync(),
                OverdueReminders = overdueReminders
            };

            return View(dashboard);
        }
    }
}
