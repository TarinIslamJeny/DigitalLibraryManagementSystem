# Digital Library Management System

A full-stack university digital library management system built with ASP.NET Core MVC, .NET 8, EF Core, MySQL, and ASP.NET Core Identity.

## Features

- User authentication and role-based access for Admin, Librarian, and Member
- Book catalog browsing, search, filtering, and pagination
- Book borrowing and return flow
- Reservation management for unavailable books
- Fine calculation for overdue returns
- Reviews and ratings for books
- Notifications for updates and reminders
- Dashboard views for admin, librarian, and member
- MySQL database integration with EF Core migrations
- Responsive Bootstrap 5 UI

## Technology Stack

- C#
- ASP.NET Core MVC
- .NET 8
- Entity Framework Core
- MySQL 8
- Pomelo.EntityFrameworkCore.MySql
- ASP.NET Core Identity
- Razor Views
- Bootstrap 5
- JavaScript

## Project Structure

- Controllers/
- Data/
- Models/
- Services/
- ViewModels/
- Views/
- wwwroot/

## Installation

1. Install Visual Studio 2022.
2. Install .NET 8 SDK.
3. Install MySQL 8 and create a database named `digital_library_db`.
4. Open the project in Visual Studio and configure the connection string in `appsettings.json`.

## MySQL Configuration

Update `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "server=localhost;port=3306;database=digital_library_db;user=root;password=YOUR_MYSQL_PASSWORD;"
}
```

## Migration Commands

From the project root, run:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Default Login Accounts

Admin:
- Email: admin@digitallibrary.com
- Password: Admin@123

Librarian:
- Email: librarian@digitallibrary.com
- Password: Library@123

## How to Run

1. Restore NuGet packages.
2. Ensure MySQL is running.
3. Set connection string in appsettings.json.
4. Run the app with Visual Studio or:

```bash
dotnet run
```

## Notes

The database is seeded automatically on startup with default roles, users, categories, authors, publishers, and sample books.
