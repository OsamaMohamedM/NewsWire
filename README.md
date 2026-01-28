# NewsWire

NewsWire is a modern ASP.NET Core MVC/Razor application (targeting .NET 10) for publishing and managing news articles with user accounts, author ownership, favorites, and a polished UI theme.

## Features
-  News management: create, edit, delete, and view articles with category/topic support.
-  Author ownership: articles are tied to the logged-in author; permissions enforced on edit/delete.
-  Favorites: users can favorite/unfavorite articles and view their list.
-  Profile dashboard: update name/photo, see authored news, favorites, and stats.
-  Identity: login/registration/reset with themed account pages.
-  UI/UX: responsive Bootstrap-based styling with custom profile/account themes.

## Tech Stack
- ASP.NET Core MVC/Razor (.NET 10)
- Entity Framework Core + SQL Server
- ASP.NET Core Identity
- Bootstrap 5 + custom CSS

## Getting Started
1) Restore & build:
```sh
dotnet restore
dotnet build
```
2) Apply migrations:
```sh
dotnet ef database update
```
3) Run:
```sh
dotnet run
```
4) Open: `https://localhost:5001` (or the port shown in console).

## Configuration
- Update `appsettings.json` ? `ConnectionStrings:DefaultConnection` to your SQL Server.
- Ensure EF tools are installed for migrations: `dotnet tool install --global dotnet-ef` (if needed).

## Migrations
- Create: `dotnet ef migrations add <Name>`
- Update DB: `dotnet ef database update`

## Project Structure (high level)
- `Controllers/` — MVC controllers (News, Profile, etc.)
- `Areas/Identity/` — Identity UI (login/register/reset)
- `Models/` — domain models & view models (User, News, Favorites, Profile)
- `Services/` — profile/favorites/news permission services (SOLID)
- `Views/` — Razor views and partials
- `wwwroot/` — static assets (custom CSS, JS)

## Key Security/Behavior
- Only owners/admins can edit/delete their articles.
- Author ID set on create; ownership validated on edit/delete.
- Favorites are per-user with a unique constraint.
- Profile updates refresh sign-in to keep cookies in sync.

## License
MIT (update if different).
