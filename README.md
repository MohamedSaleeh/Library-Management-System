# Library Management System

A complete, production-quality ASP.NET Core MVC application for managing a library's books, members, and loans.

## By
- **Mohammed Alyounes**
- **Mohammed Saleh**

---

## Features

### Authentication & Authorization
- User Registration with Full Name, Email, Password
- Login / Logout
- ASP.NET Identity with password hashing
- Role-based access control (Admin, Member)
- Admin-only pages protected with `[Authorize(Roles = "Admin")]`

### Books Management
- Create, Read, Update, Delete (CRUD) books
- Search by title, author, or ISBN
- Filter by category
- View book loan history
- Quantity tracking

### Members Management
- Create, Read, Update, Delete (CRUD) members
- Search by name, email, or phone
- View member loan history with statistics

### Loans Management
- Create, Read, Update, Delete (CRUD) loans
- Borrow book with quantity validation
- Return book with automatic fine calculation
- Status tracking (Active, Returned, Overdue)
- Fine calculation: $1 per day late

### Dashboard
- Total Books, Members, Loans statistics
- Active, Returned, Overdue loan counts
- Total fines collected
- Recent loans table
- Overdue loans table with reminders

### RESTful Web API
- Full CRUD API for Books, Members, and Loans
- JSON responses with proper HTTP status codes
- DTOs for API requests/responses
- API endpoints:
  - `GET /api/booksapi` - Get all books
  - `GET /api/booksapi/{id}` - Get book by ID
  - `POST /api/booksapi` - Create book
  - `PUT /api/booksapi/{id}` - Update book
  - `DELETE /api/booksapi/{id}` - Delete book
  - `GET /api/membersapi` - Get all members
  - `GET /api/membersapi/{id}` - Get member by ID
  - `POST /api/membersapi` - Create member
  - `PUT /api/membersapi/{id}` - Update member
  - `DELETE /api/membersapi/{id}` - Delete member
  - `GET /api/loansapi` - Get all loans
  - `GET /api/loansapi/{id}` - Get loan by ID
  - `POST /api/loansapi` - Create loan (borrow)
  - `PUT /api/loansapi/{id}` - Update loan
  - `POST /api/loansapi/{id}/return` - Return book
  - `DELETE /api/loansapi/{id}` - Delete loan
  - `GET /api/loansapi/overdue` - Get overdue loans

### Email Reminder System (Bonus)
- Generates reminders for overdue loans
- Displays notifications on dashboard
- Structured for SMTP integration (ready to add)
- Configurable via `appsettings.json`

---

## Technology Stack

- **.NET 8** - Framework
- **ASP.NET Core MVC** - Web framework
- **Entity Framework Core** - ORM (Code First)
- **SQL Server** - Database
- **ASP.NET Identity** - Authentication & Authorization
- **Bootstrap 5** - UI Framework
- **Bootstrap Icons** - Icon library

---

## Project Structure

```
LibraryManagementSystem/
|-- Controllers/           # MVC Controllers
|   |-- AccountController.cs
|   |-- DashboardController.cs
|   |-- BooksController.cs
|   |-- MembersController.cs
|   |-- LoansController.cs
|-- ApiControllers/        # RESTful API Controllers
|   |-- BooksApiController.cs
|   |-- MembersApiController.cs
|   |-- LoansApiController.cs
|-- Models/                # Entity Models
|   |-- Book.cs
|   |-- Member.cs
|   |-- Loan.cs
|   |-- ApplicationUser.cs
|   |-- ViewModels/        # View Models
|   |   |-- RegisterViewModel.cs
|   |   |-- LoginViewModel.cs
|   |   |-- DashboardViewModel.cs
|   |   |-- LoanViewModel.cs
|   |-- DTOs/              # API Data Transfer Objects
|   |   |-- BookDto.cs
|   |   |-- MemberDto.cs
|   |   |-- LoanDto.cs
|-- Data/                  # Database Context
|   |-- ApplicationDbContext.cs
|-- Services/              # Business Logic Services
|   |-- LoanService.cs
|   |-- ReminderService.cs
|-- Views/                 # Razor Views
|   |-- Account/           # Login, Register, AccessDenied
|   |-- Dashboard/         # Dashboard Index
|   |-- Books/             # Books CRUD views
|   |-- Members/           # Members CRUD views
|   |-- Loans/             # Loans CRUD views + Return
|   |-- Shared/            # _Layout, _AuthLayout, _ValidationScriptsPartial
|-- wwwroot/               # Static files
|   |-- css/site.css
|   |-- js/site.js
|-- Program.cs             # Application entry point
|-- appsettings.json       # Configuration
|-- LibraryManagementSystem.csproj
```

---

## Setup Instructions

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or SQL Server Express / LocalDB)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (optional, recommended)

### Step 1: Clone or Download the Project
```bash
cd LibraryManagementSystem
```

### Step 2: Update Connection String (if needed)
Edit `appsettings.json` to match your SQL Server instance:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LibraryManagementSystem;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

### Step 3: Run Migrations
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Or in Visual Studio Package Manager Console:
```powershell
Add-Migration InitialCreate
Update-Database
```

### Step 4: Run the Application
```bash
dotnet run
```

Or in Visual Studio: Press `F5` or `Ctrl+F5`

### Step 5: Access the Application
- Open browser and navigate to `https://localhost:7001` (or the port shown in console)

### Default Admin Credentials
- **Email:** `admin@library.com`
- **Password:** `Admin@123`

---

## Database Schema

### Books Table
| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | PK, Identity |
| Title | nvarchar(200) | Required |
| Author | nvarchar(150) | Required |
| ISBN | nvarchar(20) | Required |
| Category | nvarchar(100) | Optional |
| PublishedYear | int | Optional |
| QuantityAvailable | int | Default: 1 |

### Members Table
| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | PK, Identity |
| FullName | nvarchar(150) | Required |
| Email | nvarchar(150) | Required, Email |
| PhoneNumber | nvarchar(20) | Optional |
| Address | nvarchar(300) | Optional |
| RegistrationDate | datetime | Default: Now |

### Loans Table
| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | PK, Identity |
| BookId | int | FK → Books |
| MemberId | int | FK → Members |
| BorrowDate | datetime | Required |
| DueDate | datetime | Required |
| ReturnDate | datetime | Optional |
| FineAmount | decimal(18,2) | Default: 0 |
| Status | nvarchar(20) | Required |

---

## API Testing with Postman

### Base URL
```
https://localhost:7001/api
```

### Authentication
Most API endpoints require authentication. Login via the web UI first, then use the cookie for API requests.

### Example Requests

**Get All Books:**
```
GET /api/booksapi
```

**Create a Book:**
```
POST /api/booksapi
Content-Type: application/json

{
  "title": "New Book",
  "author": "Author Name",
  "isbn": "978-1234567890",
  "category": "Fiction",
  "publishedYear": 2024,
  "quantityAvailable": 5
}
```

**Borrow a Book (Create Loan):**
```
POST /api/loansapi
Content-Type: application/json

{
  "bookId": 1,
  "memberId": 1,
  "borrowDate": "2024-06-24",
  "dueDate": "2024-07-08"
}
```

**Return a Book:**
```
POST /api/loansapi/1/return
Content-Type: application/json

{
  "returnDate": "2024-07-01"
}
```

---

## Future Enhancements

- [ ] SMTP Email Service integration for overdue reminders
- [ ] Book reservation system
- [ ] Member self-service portal
- [ ] Barcode/QR code scanning
- [ ] Report generation (PDF/Excel)
- [ ] Multi-language support
- [ ] Dark mode theme

---

## License

This project is created for educational purposes.

## Acknowledgments

Developed by **Mohammed Alyounes** and **Mohammed Saleh** as part of a university project.
