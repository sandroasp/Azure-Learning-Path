#########################################################
#                                                       #
# Find Azure Broken API Connectors                      #
# Author(s):                                            #
#    Sandro Pereira                                     #
#                                                       #
#########################################################

$apiBrokenDataNode = @()
$apiBrokenCount = 0

#########################################################
# Azure Configurations
#########################################################
$subscriptioName = 'Your Subscription name'

#########################################################
# Perform Azure Login
#########################################################
az login

#########################################################
# Get Subscription Info
#########################################################
Write-Host 'Looking up API Connectors'
Write-Host '#########################################################'
$subscription = Get-AzSubscription -SubscriptionName $subscriptioName
$subscriptionId = $subscription.Id
Write-Host "`t Subscription Name: " -NoNewline; Write-Host $subscriptioName -ForegroundColor Green;  
Write-Host "`t Subscription Id: " -NoNewline; Write-Host $subscriptionId -ForegroundColor Green;  
Write-Host ''
Write-Host ''

##############################################################
# Get list of API Connectors available on the Resource Group
##############################################################
Write-Host 'Looking up API Connectors'
Write-Host '#########################################################'
$resourceName = ''
$resources = Get-AzResource -ResourceType Microsoft.Web/connections
$resources | ForEach-Object {     
    $logicAppUrl = $_.ResourceId + '?api-version=2018-07-01-preview'
    
    # Get Logic App Content
    $resourceJsonResult = az rest --method get --uri $logicAppUrl
    $resourceJson = $resourceJsonResult | ConvertFrom-Json 

    $resourceName = $_.Name
    $resourceGroupName = $_.ResourceGroupName

    # Check Logic App Connectors
    $apiConnectionStatus = $resourceJson.properties.overallStatus
    if($apiConnectionStatus -eq 'Error')
    {
        Write-Host "`t Resource Group: " -NoNewline; Write-Host $resourceGroupName -ForegroundColor Red -NoNewline; Write-Host "`t -> `t API Connection: " -NoNewline; Write-Host $resourceName -ForegroundColor Red -NoNewline;  Write-Host "`t -> `t Status: " -NoNewline; Write-Host $apiConnectionStatus -ForegroundColor Red;
        Write-Host "`t`t Target: " -NoNewline; Write-Host $resourceJson.properties.statuses.target -ForegroundColor Red -NoNewline; 
        Write-Host "`t -> `t Error Code: " -NoNewline; Write-Host $resourceJson.properties.statuses.error.code -ForegroundColor Red -NoNewline;  Write-Host "`t -> `t Message: " -NoNewline; Write-Host $resourceJson.properties.statuses.error.message -ForegroundColor Red;
    }
    else
    {
        Write-Host "`t Resource Group: " -NoNewline; Write-Host $resourceGroupName -ForegroundColor Green -NoNewline; Write-Host "`t -> `t API Connection: " -NoNewline; Write-Host $resourceName -ForegroundColor Green -NoNewline;  Write-Host "`t -> `t Status: " -NoNewline; Write-Host $apiConnectionStatus -ForegroundColor Green;
    }
}
Write-Host ''
Write-Host ''