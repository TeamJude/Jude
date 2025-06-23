# Project Jude: AI-Powered Claims Adjudication Platform

## 🚀 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js](https://nodejs.org/) (v18 or higher)
- [Bun](https://bun.sh/) (recommended) or npm
- [PostgreSQL](https://www.postgresql.org/)
- [Docker](https://www.docker.com/) (optional, for containerized development)

### Quick Start with Docker

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Jude
   ```

2. **Start the application with Docker Compose**
   ```bash
   docker-compose up -d
   ```

3. **Access the application**
   - Frontend: http://localhost:3000
   - Backend API: http://localhost:5000
   - Swagger Documentation: http://localhost:5000/swagger

### Manual Setup

#### Backend Setup

1. **Navigate to the server directory**
   ```bash
   cd Jude.Server
   ```

2. **Configure the database connection**
   - Copy `appsettings.Development.json.example` to `appsettings.Development.json`
   - Update the PostgreSQL connection string

3. **Install dependencies and run migrations**
   ```bash
   dotnet restore
   dotnet ef database update
   ```

4. **Run the backend**
   ```bash
   dotnet run
   ```

#### Frontend Setup

1. **Navigate to the client directory**
   ```bash
   cd Jude.Client
   ```

2. **Install dependencies**
   ```bash
   bun install
   # or
   npm install
   ```

3. **Start the development server**
   ```bash
   bun dev
   # or
   npm run dev
   ```

## 🔐 Default User Accounts

The application comes with pre-seeded user accounts for testing and initial setup:

### Administrator Account
- **Username**: `admin`
- **Email**: `admin@jude.com`
- **Password**: `Admin123!`
- **Permissions**: Full access to all features including:
  - Claims management (Read/Write)
  - User management (Read/Write)
  - Role management (Read/Write)
  - System administration

### Regular User Account
- **Username**: `user`
- **Email**: `user@jude.com`
- **Password**: `User123!`
- **Permissions**: Limited access including:
  - Claims management (Read only)

## 🛠️ Development

### Project Structure

```
Jude/
├── Jude.Server/              # .NET Backend
│   ├── Controllers/          # API Controllers
│   ├── Data/                 # Database models and context
│   ├── Domains/              # Business logic domains
│   ├── Middleware/           # Custom middleware
│   └── Extensions/           # Service extensions
├── Jude.Client/             # React Frontend
│   ├── src/
│   │   ├── components/      # Reusable components
│   │   ├── routes/          # Page components and routing
│   │   ├── lib/             # Utilities and services
│   │   └── styles/          # Global styles
│   └── public/              # Static assets
├── docs/                    # Documentation
└── docker-compose.yml       # Docker configuration
```

## 🚀 Deployment

### Production Build

1. **Build the frontend**
   ```bash
   cd Jude.Client
   bun build
   ```

2. **Build the backend**
   ```bash
   cd Jude.Server
   dotnet publish -c Release
   ```

### Docker Deployment

```bash
docker-compose -f docker-compose.prod.yml up -d
```