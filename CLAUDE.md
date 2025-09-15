# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is an ASP.NET Core 8.0 MVC web application for the "Bear Toy Website" backend. It uses Entity Framework Core with SQL Server and includes ASP.NET Core Identity for authentication.

## Essential Commands

### Development Commands
```bash
# Run the application with hot reload
dotnet watch run

# Build the project
dotnet build

# Run without hot reload
dotnet run

# Clean build artifacts
dotnet clean
```

### Database Commands
```bash
# Apply pending migrations to database
dotnet ef database update

# Create a new migration
dotnet ef migrations add <MigrationName>

# Remove last migration (if not applied)
dotnet ef migrations remove

# List all migrations
dotnet ef migrations list
```

### Publishing
```bash
# Publish for production
dotnet publish -c Release -o ./publish
```

## Architecture Overview

### MVC Structure
- **Controllers/** - HTTP request handlers that return views or data
  - Currently only `HomeController` exists
  - Add new controllers here for additional functionality
  
- **Views/** - Razor (.cshtml) templates for rendering HTML
  - `/Views/Home/` - Views specific to HomeController
  - `/Views/Shared/` - Shared layouts and partial views
  - `_ViewStart.cshtml` and `_ViewImports.cshtml` configure view defaults

- **Models/** - View models and data models
  - Currently minimal - expand as needed

### Authentication & Data Layer
- **Data/ApplicationDbContext.cs** - Entity Framework database context
  - Inherits from `IdentityDbContext` for user management
  - Add new `DbSet<T>` properties here for new entities
  
- **Areas/Identity/** - ASP.NET Core Identity pages (scaffolded)
  - Authentication is configured to require email confirmation
  - Identity UI package provides default login/register pages

### Application Pipeline (Program.cs)
The middleware pipeline is configured in this order:
1. Exception handling (developer page in development)
2. HTTPS redirection (production only)
3. Static files (wwwroot)
4. Routing
5. Authentication & Authorization
6. MVC/Razor Pages endpoints

### Key Configuration
- **Connection String**: Configured in `appsettings.json` as "DefaultConnection"
  - Uses LocalDB for development
  - Update for production SQL Server
  
- **Launch URLs**: 
  - HTTPS: https://localhost:7106
  - HTTP: http://localhost:5062

## Development Guidelines

### Adding New Features
1. For new data entities:
   - Create model class in Models/
   - Add `DbSet<T>` to ApplicationDbContext
   - Run `dotnet ef migrations add <Name>`
   - Run `dotnet ef database update`

2. For new pages:
   - Create controller in Controllers/
   - Create corresponding views in Views/<ControllerName>/
   - Follow existing naming conventions (e.g., HomeController → /Views/Home/)

3. For API endpoints:
   - Consider adding a separate API controller inheriting from `ControllerBase`
   - Use `[ApiController]` attribute for automatic model validation

### Database Development
- Migrations are stored in Data/Migrations/
- Always review generated migrations before applying
- Use meaningful migration names describing the changes

### Client-Side Assets
- Static files go in wwwroot/
- JavaScript libraries are in wwwroot/lib/
- Custom CSS in wwwroot/css/site.css
- Custom JS in wwwroot/js/site.js

### Security Considerations
- Identity is configured to require confirmed accounts
- HTTPS is enforced in production
- Anti-forgery tokens are automatically included in forms
- Connection strings should use integrated security or secure credentials

## General Principles
- 以繁體中文回應
- 以台灣習慣的用語
- Avoid introducing any breaking changes during code refactoring.
- Implement all necessary foolproof mechanisms to ensure the code runs reliably and without errors.