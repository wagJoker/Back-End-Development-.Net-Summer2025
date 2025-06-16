# Real-time Chat Application with Sentiment Analysis

A modern real-time chat application built with ASP.NET Core and Angular, featuring sentiment analysis using Azure Cognitive Services.

## Features

- Real-time messaging using SignalR
- User authentication with JWT
- Sentiment analysis of messages
- Message history and search
- Online user tracking
- Message editing and deletion
- Responsive design
- Azure deployment ready

## Technology Stack

### Backend
- ASP.NET Core 8.0
- Entity Framework Core
- SignalR with Azure SignalR Service
- Azure Cognitive Services (Text Analytics)
- SQL Server
- Serilog for logging
- JWT Authentication

### Frontend
- Angular 17
- Tailwind CSS
- SignalR Client
- RxJS for reactive programming
- Angular Material

### Infrastructure
- Azure App Service
- Azure SQL Database
- Azure SignalR Service
- Azure Cognitive Services
- Azure Application Insights
- Azure Key Vault

## Prerequisites

- .NET 8.0 SDK
- Node.js 18.x
- Azure subscription
- Azure CLI
- SQL Server (local development)

## Getting Started

1. Clone the repository:
```bash
git clone https://github.com/yourusername/chat-app.git
cd chat-app
```

2. Configure Azure services:
```bash
# Login to Azure
az login

# Create resource group
az group create --name chat-app-rg --location eastus

# Deploy Azure resources
az deployment group create --resource-group chat-app-rg --template-file azure-deploy.json --parameters azure-deploy.parameters.json
```

3. Configure SSL certificates:
```bash
# Update domain name in configure-ssl.ps1
.\scripts\configure-ssl.ps1
```

4. Configure database backups:
```bash
.\scripts\configure-backup.ps1
```

5. Set up environment variables:
```bash
# Backend (.env)
ConnectionStrings__DefaultConnection="Server=your-sql-server.database.windows.net;Database=ChatApp;User Id=your-username;Password=your-password;"
ConnectionStrings__AzureSignalR="your-signalr-connection-string"
CognitiveServices__TextAnalyticsEndpoint="your-cognitive-services-endpoint"
CognitiveServices__TextAnalyticsKey="your-cognitive-services-key"
Jwt__Key="your-jwt-secret-key"
Jwt__Issuer="your-jwt-issuer"
Jwt__Audience="your-jwt-audience"

# Frontend (.env)
VITE_API_URL="https://api.your-domain.com"
VITE_SIGNALR_URL="https://api.your-domain.com"
```

6. Run the application locally:
```bash
# Backend
cd ChatApp.Backend
dotnet restore
dotnet run

# Frontend
cd chat-app-frontend
npm install
npm start
```

## Deployment

### Azure DevOps Pipeline
The application uses Azure DevOps for CI/CD. The pipeline is configured in `azure-pipelines.yml` and includes:
- Building backend and frontend
- Running tests
- Creating Docker images
- Deploying to Azure App Service

### GitHub Actions
Alternative deployment using GitHub Actions is configured in `.github/workflows/azure-deploy.yml`.

## Security Features

- HTTPS enforced
- JWT authentication
- CORS configured
- Security headers
- Rate limiting
- SQL injection prevention
- XSS protection
- CSRF protection

## Monitoring and Logging

- Application Insights integration
- Serilog for structured logging
- Health check endpoints
- Performance monitoring
- Error tracking
- User activity logging

## Database Backup and Recovery

- Daily automated backups
- Long-term retention (4 weeks, 12 months, 5 years)
- Geo-replication for high availability
- Point-in-time restore
- Backup encryption

## Scaling

- Auto-scaling configured for both backend and frontend
- Backend scales from 1 to 10 instances based on CPU usage
- Frontend scales from 1 to 5 instances based on CPU usage
- Load balancing
- Connection pooling

## API Documentation

API documentation is available at `/swagger` when running in development mode.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support, please open an issue in the GitHub repository or contact the development team.

## Acknowledgments

- Azure Documentation
- ASP.NET Core Documentation
- Angular Documentation
- SignalR Documentation

## Project Structure and Architecture

### Backend Structure
```
ChatApp.Backend/
├── Controllers/           # API Controllers
│   ├── AuthController.cs  # Authentication endpoints
│   └── MessageController.cs # Message management endpoints
├── Data/                 # Data access layer
│   ├── ChatDbContext.cs  # Database context
│   └── Migrations/       # Entity Framework migrations
├── Hubs/                 # SignalR hubs
│   └── ChatHub.cs       # Real-time communication hub
├── Models/              # Domain models
│   ├── ChatMessage.cs   # Message entity
│   └── User.cs         # User entity
├── Services/            # Business logic
│   ├── AuthService.cs   # Authentication service
│   ├── MessageService.cs # Message handling service
│   └── SentimentAnalysisService.cs # Azure Cognitive Services integration
├── Program.cs           # Application entry point
└── appsettings.json     # Configuration
```

### Frontend Structure
```
chat-app-frontend/
├── src/
│   ├── app/
│   │   ├── components/  # Angular components
│   │   │   ├── chat/    # Chat interface
│   │   │   └── auth/    # Authentication forms
│   │   ├── services/    # Angular services
│   │   │   ├── api.service.ts    # REST API client
│   │   │   ├── auth.service.ts   # Authentication
│   │   │   └── signalr.service.ts # Real-time communication
│   │   ├── models/      # TypeScript interfaces
│   │   └── app.module.ts # Main module
│   ├── assets/         # Static files
│   └── environments/   # Environment configurations
└── package.json        # Dependencies
```

### Architecture Overview

The application follows a modern microservices architecture with the following components:

1. **Backend API (.NET Core)**
   - RESTful API endpoints for CRUD operations
   - SignalR hub for real-time communication
   - Entity Framework Core for data access
   - JWT-based authentication
   - Azure Cognitive Services integration

2. **Frontend (Angular)**
   - Single Page Application (SPA)
   - Reactive forms for user input
   - SignalR client for real-time updates
   - Tailwind CSS for styling
   - Angular Material components

3. **Azure Services**
   - App Service for hosting
   - SQL Database for data storage
   - SignalR Service for real-time communication
   - Cognitive Services for sentiment analysis
   - Application Insights for monitoring

### Data Flow

1. **Authentication Flow**
   ```
   User -> Frontend -> AuthController -> JWT Token -> SignalR Connection
   ```

2. **Message Flow**
   ```
   User Input -> Frontend -> SignalR Hub -> MessageService -> Database
   Sentiment Analysis -> Cognitive Services -> Message Update
   ```

3. **Real-time Updates**
   ```
   MessageService -> SignalR Hub -> Connected Clients -> UI Update
   ```

### Running the Application

#### Development Mode

1. **Backend Setup**
   ```bash
   # Navigate to backend directory
   cd ChatApp.Backend

   # Restore dependencies
   dotnet restore

   # Apply database migrations
   dotnet ef database update

   # Run the application
   dotnet run
   ```

2. **Frontend Setup**
   ```bash
   # Navigate to frontend directory
   cd chat-app-frontend

   # Install dependencies
   npm install

   # Start development server
   npm start
   ```

#### Production Mode

1. **Build Backend**
   ```bash
   # Build the application
   dotnet publish -c Release

   # The output will be in bin/Release/net8.0/publish
   ```

2. **Build Frontend**
   ```bash
   # Build the application
   npm run build

   # The output will be in dist/
   ```

3. **Docker Deployment**
   ```bash
   # Build and run using Docker Compose
   docker-compose up -d
   ```

### Testing

1. **Backend Tests**
   ```bash
   # Run all tests
   dotnet test

   # Run specific test project
   dotnet test ChatApp.Backend.Tests
   ```

2. **Frontend Tests**
   ```bash
   # Run unit tests
   npm run test

   # Run e2e tests
   npm run e2e
   ```

### Monitoring and Debugging

1. **Application Insights**
   - View live metrics in Azure Portal
   - Track performance and errors
   - Monitor user activity

2. **Logging**
   - View logs in Azure Portal
   - Check Serilog files in `logs/` directory
   - Monitor real-time logs using `dotnet watch`

3. **Database**
   - Monitor using Azure Portal
   - Check connection pool status
   - View query performance

### Troubleshooting

1. **Common Issues**
   - SignalR connection issues: Check Azure SignalR Service status
   - Database connection: Verify connection string and firewall rules
   - Authentication: Check JWT configuration and token expiration

2. **Debug Tools**
   - Use Azure Portal for service monitoring
   - Check Application Insights for errors
   - View logs in Azure Log Analytics 