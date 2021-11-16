$connection = Get-AutomationConnection -Name AzureRunAsConnection

Connect-AzAccount -ServicePrincipal `
    -Tenant $connection.TenantID `
    -ApplicationId $connection.ApplicationID `
    -CertificateThumbprint $connection.CertificateThumbprint

Set-AzContext -SubscriptionId '<subscription-id>'

$apiBrokenDataNode = @()
$apiBrokenCount = 0
$resourceName = ''
$resources = Get-AzResource -ResourceType Microsoft.Web/connections
$resources | ForEach-Object {     
    $logicAppUrl = $_.ResourceId + '?api-version=2018-07-01-preview'
    
    # Get Logic App Content
    $var = "https://management.azure.com" + $logicAppUrl
    $accsessToken = Get-AzAccessToken `
		-TenantId $connection.TenantID
    $auth = "Bearer " + $accsessToken.Token

    $resourceJson = Invoke-RestMethod -Uri $var -Headers @{ Authorization = $auth }

    $resourceName = $_.Name
    $resourceGroupName = $_.ResourceGroupName

    # Check Logic App Connectors
    $apiConnectionStatus = $resourceJson.properties.overallStatus
    if($apiConnectionStatus -eq 'Error')
    {
        $apiBrokenCount++;
        $apiBrokenDataNode += [pscustomobject]@{
                'ResourceGroupName' = $_.ResourceGroupName;
                'ResourceName' = $_.Name;
                'Status' = $resourceJson.properties.statuses.status;
                'APIName' = $resourceJson.properties.api.name;
                'APIDisplayName' = $resourceJson.properties.api.displayName;
                'ResourceType'= $resourceJson.type;
                'ResourceLocation'= $resourceJson.location;
                'ResourceId'= $resourceJson.id;
                'ErrorCode'= $resourceJson.properties.statuses.error.code
                'ErrorMessage'= $resourceJson.properties.statuses.error.message
            }
    }
}

if($apiBrokenCount -eq 0)
{
    exit
}

$jsonDoc = [pscustomobject]@{
    Monitor = "API Connections"
    Client = "Sandro Pereira"
    Environment = "DEV"
    APIBroken = $apiBrokenDataNode
}

$var = Invoke-WebRequest -Uri '<logic-app-URI>' -Method POST -Body ($jsonDoc|ConvertTo-Json) -ContentType "application/json; charset=utf-8"