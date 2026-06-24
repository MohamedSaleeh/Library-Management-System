namespace LibraryManagementSystem.Models.ViewModels
{
    /// <summary>
    /// ViewModel for the admin dashboard displaying library statistics.
    /// </summary>
    public class DashboardViewModel
    {
        public int TotalBooks { get; set; }
        public int TotalMembers { get; set; }
        public int TotalLoans { get; set; }
        public int OverdueLoans { get; set; }
        public int ActiveLoans { get; set; }
        public int ReturnedLoans { get; set; }
        public decimal TotalFines { get; set; }
        public List<Loan> RecentLoans { get; set; } = new List<Loan>();
        public List<Loan> OverdueLoanList { get; set; } = new List<Loan>();
        public List<ReminderViewModel> OverdueReminders { get; set; } = new List<ReminderViewModel>();
    }

    /// <summary>
    /// Represents a reminder for overdue loans.
    /// </summary>
    public class ReminderViewModel
    {
        public int LoanId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public string BookTitle { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int DaysLate { get; set; }
        public decimal FineAmount { get; set; }
        public string ReminderMessage { get; set; } = string.Empty;
    }
}
