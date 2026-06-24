using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data
{
    /// <summary>
    /// Database context for the Library Management System.
    /// Inherits from IdentityDbContext to support ASP.NET Identity authentication.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for library entities
        public DbSet<Book> Books { get; set; } = null!;
        public DbSet<Member> Members { get; set; } = null!;
        public DbSet<Loan> Loans { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships and constraints
            builder.Entity<Loan>()
                .HasOne(l => l.Book)
                .WithMany(b => b.Loans)
                .HasForeignKey(l => l.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Loan>()
                .HasOne(l => l.Member)
                .WithMany(m => m.Loans)
                .HasForeignKey(l => l.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed sample data
            SeedData(builder);
        }

        /// <summary>
        /// Seeds initial data into the database.
        /// </summary>
        private static void SeedData(ModelBuilder builder)
        {
            // Seed Books
            builder.Entity<Book>().HasData(
                new Book
                {
                    Id = 1,
                    Title = "Clean Code",
                    Author = "Robert C. Martin",
                    ISBN = "978-0132350884",
                    Category = "Programming",
                    PublishedYear = 2008,
                    QuantityAvailable = 5
                },
                new Book
                {
                    Id = 2,
                    Title = "Design Patterns",
                    Author = "Erich Gamma, Richard Helm, Ralph Johnson, John Vlissides",
                    ISBN = "978-0201633610",
                    Category = "Software Engineering",
                    PublishedYear = 1994,
                    QuantityAvailable = 3
                },
                new Book
                {
                    Id = 3,
                    Title = "The Pragmatic Programmer",
                    Author = "Andrew Hunt, David Thomas",
                    ISBN = "978-0135957059",
                    Category = "Programming",
                    PublishedYear = 2019,
                    QuantityAvailable = 4
                },
                new Book
                {
                    Id = 4,
                    Title = "Introduction to Algorithms",
                    Author = "Thomas H. Cormen",
                    ISBN = "978-0262033848",
                    Category = "Computer Science",
                    PublishedYear = 2009,
                    QuantityAvailable = 6
                },
                new Book
                {
                    Id = 5,
                    Title = "Structure and Interpretation of Computer Programs",
                    Author = "Harold Abelson, Gerald Jay Sussman",
                    ISBN = "978-0262510875",
                    Category = "Computer Science",
                    PublishedYear = 1996,
                    QuantityAvailable = 2
                },
                new Book
                {
                    Id = 6,
                    Title = "Code Complete",
                    Author = "Steve McConnell",
                    ISBN = "978-0735619678",
                    Category = "Programming",
                    PublishedYear = 2004,
                    QuantityAvailable = 3
                },
                new Book
                {
                    Id = 7,
                    Title = "Refactoring",
                    Author = "Martin Fowler",
                    ISBN = "978-0201485677",
                    Category = "Software Engineering",
                    PublishedYear = 1999,
                    QuantityAvailable = 4
                },
                new Book
                {
                    Id = 8,
                    Title = "Head First Design Patterns",
                    Author = "Eric Freeman, Bert Bates",
                    ISBN = "978-0596007126",
                    Category = "Programming",
                    PublishedYear = 2004,
                    QuantityAvailable = 5
                }
            );

            // Seed Members
            builder.Entity<Member>().HasData(
                new Member
                {
                    Id = 1,
                    FullName = "John Smith",
                    Email = "john.smith@email.com",
                    PhoneNumber = "555-0101",
                    Address = "123 Main St, New York, NY 10001",
                    RegistrationDate = new DateTime(2024, 1, 15)
                },
                new Member
                {
                    Id = 2,
                    FullName = "Sarah Johnson",
                    Email = "sarah.j@email.com",
                    PhoneNumber = "555-0102",
                    Address = "456 Oak Ave, Los Angeles, CA 90001",
                    RegistrationDate = new DateTime(2024, 2, 20)
                },
                new Member
                {
                    Id = 3,
                    FullName = "Michael Brown",
                    Email = "m.brown@email.com",
                    PhoneNumber = "555-0103",
                    Address = "789 Pine Rd, Chicago, IL 60601",
                    RegistrationDate = new DateTime(2024, 3, 10)
                },
                new Member
                {
                    Id = 4,
                    FullName = "Emily Davis",
                    Email = "emily.davis@email.com",
                    PhoneNumber = "555-0104",
                    Address = "321 Elm St, Houston, TX 77001",
                    RegistrationDate = new DateTime(2024, 4, 5)
                },
                new Member
                {
                    Id = 5,
                    FullName = "David Wilson",
                    Email = "d.wilson@email.com",
                    PhoneNumber = "555-0105",
                    Address = "654 Maple Dr, Phoenix, AZ 85001",
                    RegistrationDate = new DateTime(2024, 5, 12)
                }
            );

            // Seed Loans
            builder.Entity<Loan>().HasData(
                new Loan
                {
                    Id = 1,
                    BookId = 1,
                    MemberId = 1,
                    BorrowDate = new DateTime(2024, 6, 1),
                    DueDate = new DateTime(2024, 6, 15),
                    ReturnDate = null,
                    FineAmount = 0,
                    Status = LoanStatus.Active
                },
                new Loan
                {
                    Id = 2,
                    BookId = 3,
                    MemberId = 2,
                    BorrowDate = new DateTime(2024, 5, 20),
                    DueDate = new DateTime(2024, 6, 3),
                    ReturnDate = null,
                    FineAmount = 0,
                    Status = LoanStatus.Overdue
                },
                new Loan
                {
                    Id = 3,
                    BookId = 2,
                    MemberId = 3,
                    BorrowDate = new DateTime(2024, 5, 10),
                    DueDate = new DateTime(2024, 5, 24),
                    ReturnDate = new DateTime(2024, 5, 22),
                    FineAmount = 0,
                    Status = LoanStatus.Returned
                },
                new Loan
                {
                    Id = 4,
                    BookId = 4,
                    MemberId = 1,
                    BorrowDate = new DateTime(2024, 6, 5),
                    DueDate = new DateTime(2024, 6, 19),
                    ReturnDate = null,
                    FineAmount = 0,
                    Status = LoanStatus.Active
                },
                new Loan
                {
                    Id = 5,
                    BookId = 5,
                    MemberId = 4,
                    BorrowDate = new DateTime(2024, 5, 1),
                    DueDate = new DateTime(2024, 5, 15),
                    ReturnDate = null,
                    FineAmount = 0,
                    Status = LoanStatus.Overdue
                },
                new Loan
                {
                    Id = 6,
                    BookId = 6,
                    MemberId = 5,
                    BorrowDate = new DateTime(2024, 4, 15),
                    DueDate = new DateTime(2024, 4, 29),
                    ReturnDate = new DateTime(2024, 4, 28),
                    FineAmount = 0,
                    Status = LoanStatus.Returned
                }
            );
        }
    }
}
