cat > README.md << 'EOF'
Community Library Management System

An ASP.NET Core MVC application for managing a small community library.

Features:
- Book Management: CRUD operations with search and filter
- Member Management: Track library members
- Loan Management: Create loans, track returns, prevent duplicate active loans
- Admin Role Management: Create/delete roles (Admin only)
- Overdue Detection: Automatic overdue loan highlighting

Technologies:
- .NET 10.0
- ASP.NET Core MVC
- Entity Framework Core (SQLite)
- ASP.NET Core Identity
- Bootstrap 5
- Bogus (for seed data)
- xUnit (for testing)
- GitHub Actions (CI/CD)

Default Admin User:
- Email: admin@library.com
- Password: Admin123!

Setup Instructions:

Clone the repository:
git clone https://github.com/Naranbat423/oop-s2-1-mvc-78577.git
cd oop-s2-1-mvc-78577

Restore dependencies:
dotnet restore

Create and update database:
cd Library.MVC
dotnet ef database update

Run the application:
dotnet run

Access the application at:
http://localhost:5126

Project Structure:
Library.Domain/ - Entity models (Book, Member, Loan)
Library.MVC/ - MVC application (Controllers, Views, Data)
Library.Tests/ - Unit tests

GitHub Actions:
CI workflow runs on every push to main, running build and tests.
