# Login to Azure
az login

# Set variables
$resourceGroup = "chat-app-rg"
$backendAppName = "chat-app-backend"
$frontendAppName = "chat-app-frontend"
$domainName = "your-domain.com" # Replace with your domain

# Create App Service Managed Certificate for backend
az webapp config ssl create --resource-group $resourceGroup --name $backendAppName --hostname "api.$domainName"

# Create App Service Managed Certificate for frontend
az webapp config ssl create --resource-group $resourceGroup --name $frontendAppName --hostname $domainName

# Bind SSL certificates
az webapp config ssl bind --resource-group $resourceGroup --name $backendAppName --certificate-thumbprint $(az webapp config ssl list --resource-group $resourceGroup --name $backendAppName --query "[0].thumbprint" -o tsv) --ssl-type SNI

az webapp config ssl bind --resource-group $resourceGroup --name $frontendAppName --certificate-thumbprint $(az webapp config ssl list --resource-group $resourceGroup --name $frontendAppName --query "[0].thumbprint" -o tsv) --ssl-type SNI

# Configure custom domains
az webapp config hostname add --resource-group $resourceGroup --webapp-name $backendAppName --hostname "api.$domainName"
az webapp config hostname add --resource-group $resourceGroup --webapp-name $frontendAppName --hostname $domainName 