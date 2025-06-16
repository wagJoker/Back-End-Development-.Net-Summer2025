# Chat Application with Sentiment Analysis

Real-time chat application with sentiment analysis using Azure services.

## Features

- Real-time chat using SignalR
- Sentiment analysis of messages using Azure Cognitive Services
- User authentication with JWT
- Message history and persistence
- User presence tracking
- Typing indicators
- Message editing and deletion
- Responsive design

## Prerequisites

- .NET 8.0 SDK
- SQL Server (local or Azure)
- Azure account for:
  - Azure SignalR Service
  - Azure Cognitive Services (Text Analytics)
  - Azure App Service (for deployment)

## Configuration

1. Clone the repository
2. Update `appsettings.json` with your configuration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your-sql-server-connection-string",
    "AzureSignalR": "your-azure-signalr-connection-string"
  },
  "CognitiveServices": {
    "TextAnalyticsEndpoint": "your-text-analytics-endpoint",
    "TextAnalyticsKey": "your-text-analytics-key"
  },
  "Jwt": {
    "Key": "your-super-secret-key-with-at-least-32-characters",
    "Issuer": "ChatApp",
    "Audience": "ChatAppUsers",
    "ExpiryInMinutes": 60
  }
}
```

## Development

1. Install dependencies:
```bash
dotnet restore
```

2. Apply database migrations:
```bash
dotnet ef database update
```

3. Run the application:
```bash
dotnet run
```

## API Endpoints

### Authentication

- POST `/api/auth/register` - Register a new user
- POST `/api/auth/login` - Login
- POST `/api/auth/refresh-token` - Refresh JWT token
- POST `/api/auth/logout` - Logout

### Messages

- GET `/api/message` - Get recent messages
- GET `/api/message/user/{username}` - Get messages by user
- GET `/api/message/count` - Get total message count
- PUT `/api/message/{id}` - Update a message
- DELETE `/api/message/{id}` - Delete a message

### SignalR Hub

- `/chathub` - WebSocket endpoint for real-time communication

## Deployment

### Backend

1. Create an Azure App Service
2. Configure the following Application Settings:
   - ConnectionStrings:DefaultConnection
   - ConnectionStrings:AzureSignalR
   - CognitiveServices:TextAnalyticsEndpoint
   - CognitiveServices:TextAnalyticsKey
   - Jwt:Key
   - Jwt:Issuer
   - Jwt:Audience
   - Jwt:ExpiryInMinutes

3. Deploy using Azure DevOps or GitHub Actions

### Frontend

1. Create an Azure Static Web App
2. Configure the following environment variables:
   - REACT_APP_API_URL
   - REACT_APP_SIGNALR_URL

3. Deploy using Azure DevOps or GitHub Actions

## Security

- JWT authentication
- HTTPS enforcement
- CORS configuration
- Rate limiting
- Input validation
- SQL injection prevention
- XSS protection

## Monitoring

- Application Insights integration
- Health checks
- Structured logging with Serilog
- Performance metrics

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details. 