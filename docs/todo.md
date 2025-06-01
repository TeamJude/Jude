## Project Jude: Authentication TODO List

**Server-Side (jude.server - .NET C#)**

1.  [x] **Initial Project & DB Setup:**
    - Create ASP.NET Core Web API project.
    - Install core NuGet packages (EF Core, JWT, FluentValidation).
    - Configure `appsettings.json` (DB connection, JWT settings).
    - Define `User` model and `AppDbContext`.
    - Create and apply initial EF Core migration.
2.  [x] **Core Middleware & Utilities:**
    - Implement `Result` type for error handling.
    - Set up global exception handling middleware.
    - Configure CORS.
3.  [x] **Implement Auth Feature:**
    - Create DTOs (`RegisterRequest`, `LoginRequest`, `TokenResponse`).
    - Create FluentValidation validators for requests.
    - Implement `PasswordHasher` utility.
    - Develop `IAuthService` & `AuthService` (register, login, token generation).
    - Create Auth Endpoints/Controller (`/register`, `/login`).
4.  [x] **Configure JWT Authentication:**
    - Set up JWT Bearer authentication in `Program.cs`.
    - Add authentication & authorization middleware.

---

**Client-Side (jude.client - React)**

1.  [ ] **Initial Project & UI Setup:**
    - Create React + TypeScript project (Vite).
    - Install core npm packages (Tailwind, Lucide icons, Tanstack Query/Router) (USE tailwind v3 i've had issues with v4).
    - Set up Tailwind CSS (v3) and configure `tailwind.config.js`.
    - Setup HeroUI chosen fonts.
2.  [ ] **Core Structure & API Client:**
    - Establish basic folder structure (`core`, `features`, `components`, `routes`).
    - Configure `apiClient.ts`
    - Set up `QueryClient` and `providers`.
3.  [ ] **Develop Auth UI (Forms & Pages):**
    - Create the auth pages and components
    - Style with Tailwind CSS / HeroUI.
4.  [ ] **Implement Auth Logic & State:**
5.  [ ] **Implement Routing & Protection:**
    - Set up basic routing (login, register, placeholder dashboard).
    - Create `ProtectedRoute` component.
    - Implement logout functionality.
