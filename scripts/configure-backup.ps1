# Login to Azure
az login

# Set variables
$resourceGroup = "chat-app-rg"
$sqlServerName = "chat-app-sql"
$databaseName = "ChatApp"
$storageAccountName = "chatappbackups"
$containerName = "sql-backups"

# Create storage account for backups
az storage account create --name $storageAccountName --resource-group $resourceGroup --location eastus --sku Standard_LRS

# Create container for backups
az storage container create --name $containerName --account-name $storageAccountName

# Get storage account key
$storageKey = $(az storage account keys list --account-name $storageAccountName --resource-group $resourceGroup --query "[0].value" -o tsv)

# Configure long-term retention policy
az sql db long-term-retention-policy set --resource-group $resourceGroup --server $sqlServerName --name $databaseName --weekly-retention "P4W" --monthly-retention "P12M" --yearly-retention "P5Y"

# Configure short-term retention policy
az sql db short-term-retention-policy set --resource-group $resourceGroup --server $sqlServerName --name $databaseName --retention-days 7

# Create backup schedule
az sql db backup short-term-retention-policy set --resource-group $resourceGroup --server $sqlServerName --name $databaseName --retention-days 7

# Configure geo-replication
az sql db replica create --resource-group $resourceGroup --server $sqlServerName --name $databaseName --partner-server "chat-app-sql-secondary" --partner-resource-group $resourceGroup 