using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Services
{
    /// <summary>
    /// Service that handles email reminders for overdue loans.
    /// Structure is ready for SMTP integration.
    /// </summary>
    public class ReminderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ReminderService> _logger;

        public ReminderService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<ReminderService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Generates reminder messages for all overdue loans.
        /// </summary>
        public async Task<List<ReminderViewModel>> GenerateOverdueRemindersAsync()
        {
            var overdueLoans = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .Where(l => l.Status == LoanStatus.Overdue || 
                            (!l.ReturnDate.HasValue && l.DueDate < DateTime.Now))
                .ToListAsync();

            var reminders = new List<ReminderViewModel>();

            foreach (var loan in overdueLoans)
            {
                // Ensure status and fine are up to date
                loan.UpdateStatus();
                loan.CalculateFine();

                int daysLate = (DateTime.Now - loan.DueDate).Days;

                var reminder = new ReminderViewModel
                {
                    LoanId = loan.Id,
                    MemberName = loan.Member?.FullName ?? "Unknown",
                    BookTitle = loan.Book?.Title ?? "Unknown",
                    DueDate = loan.DueDate,
                    DaysLate = daysLate,
                    FineAmount = loan.FineAmount,
                    ReminderMessage = GenerateReminderMessage(loan, daysLate)
                };

                reminders.Add(reminder);
            }

            await _context.SaveChangesAsync();
            return reminders;
        }

        /// <summary>
        /// Generates a personalized reminder message for an overdue loan.
        /// </summary>
        private string GenerateReminderMessage(Loan loan, int daysLate)
        {
            return $"Dear {loan.Member?.FullName},\n\n" +
                   $"This is a reminder that the book \"{loan.Book?.Title}\" is overdue.\n" +
                   $"Due Date: {loan.DueDate:yyyy-MM-dd}\n" +
                   $"Days Late: {daysLate}\n" +
                   $"Current Fine: ${loan.FineAmount:F2}\n\n" +
                   $"Please return the book as soon as possible to avoid additional fines.\n\n" +
                   $"Thank you,\n" +
                   $"Library Management System";
        }

        /// <summary>
        /// Sends email reminder (structure ready for SMTP implementation).
        /// Uncomment and configure SMTP settings in appsettings.json to enable.
        /// </summary>
        public async Task<bool> SendEmailReminderAsync(ReminderViewModel reminder)
        {
            try
            {
                // SMTP Configuration - ready for implementation
                // Uncomment the code below and configure SMTP settings in appsettings.json

                /*
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var host = smtpSettings["Host"];
                var port = int.Parse(smtpSettings["Port"]!);
                var enableSsl = bool.Parse(smtpSettings["EnableSsl"]!);
                var username = smtpSettings["Username"];
                var password = smtpSettings["Password"];
                var fromEmail = smtpSettings["FromEmail"];
                var fromName = smtpSettings["FromName"];

                using var client = new SmtpClient(host, port)
                {
                    EnableSsl = enableSsl,
                    Credentials = new NetworkCredential(username, password)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail!, fromName),
                    Subject = $"Overdue Book Reminder - {reminder.BookTitle}",
                    Body = reminder.ReminderMessage,
                    IsBodyHtml = false
                };

                // Get member email
                var member = await _context.Members.FindAsync(reminder.LoanId);
                if (member != null)
                {
                    mailMessage.To.Add(member.Email);
                    await client.SendMailAsync(mailMessage);
                }
                */

                _logger.LogInformation("Email reminder prepared for {MemberName} - Book: {BookTitle}",
                    reminder.MemberName, reminder.BookTitle);

                // For now, return true to indicate the reminder was processed
                // In production, return based on actual email send result
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email reminder for Loan ID: {LoanId}", reminder.LoanId);
                return false;
            }
        }

        /// <summary>
        /// Gets dashboard notification count for overdue reminders.
        /// </summary>
        public async Task<int> GetOverdueReminderCountAsync()
        {
            return await _context.Loans
                .Where(l => !l.ReturnDate.HasValue && l.DueDate < DateTime.Now)
                .CountAsync();
        }
    }
}
