#########################################################
#                                                       #
# Find Azure Orphaned API Connectors                    #
# Author(s):                                            #
#    Sandro Pereira                                     #
#                                                       #
#########################################################

#########################################################
# Azure Configurations
#########################################################
$subscriptioName = '<subscription-name>'

#use the collection to build up objects for the table
$connectorDictionary = New-Object "System.Collections.Generic.Dictionary``2[System.String,System.Object]" 

#########################################################
# Perform Azure Login
#########################################################
Connect-AzAccount

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
    $resourceName = $_.Name
    $resourceGroupName = $_.ResourceGroupName
    Write-Host "`t Resource Group: " -NoNewline; Write-Host $resourceGroupName -ForegroundColor Green -NoNewline; Write-Host "`t -> `t API Connection: " -NoNewline; Write-Host $resourceName -ForegroundColor Green;
    
    $resourceIdLower = $_.id.ToLower()

    $azureConnector = New-Object -TypeName psobject
    $azureConnector | Add-Member -MemberType NoteProperty -Name 'Id' -Value $_.Id
    $azureConnector | Add-Member -MemberType NoteProperty -Name 'Name' -Value $_.Name
    $azureConnector | Add-Member -MemberType NoteProperty -Name 'ResourceGroup' -Value $_.ResourceGroupName
    $azureConnector | Add-Member -MemberType NoteProperty -Name 'IsUsed' -Value 'FALSE'

    $connectorDictionary.Add($resourceIdLower, $azureConnector)  
}
Write-Host ''
Write-Host ''

#########################################################
# Check logic apps to find orphaned connectors
#########################################################
Write-Host 'Looking up Logic Apps'
Write-Host '#########################################################'
$resources = Get-AzResource -ResourceType Microsoft.Logic/workflows
$resources | ForEach-Object {     
    $resourceName = $_.Name    
    $resourceGroupName = $_.ResourceGroupName
    $resourceGroupPath = '/subscriptions/' + $subscriptionId + '/resourceGroups/' + $resourceGroupName

    $logicApp = Get-AzLogicApp -Name $resourceName -ResourceGroupName $resourceGroupName        
    $logicAppUrl = $resourceGroupPath + '/providers/Microsoft.Logic/workflows/' + $logicApp.Name + '?api-version=2019-05-01'
    
    # Get Logic App Content
    $logicAppJson = az rest --method get --uri $logicAppUrl
    $logicAppJsonText = $logicAppJson | ConvertFrom-Json    

    # Check Logic App Connectors
    $logicAppParameters = $logicAppJsonText.properties.parameters
    $logicAppConnections = $logicAppParameters.psobject.properties.Where({$_.name -eq '$connections'}).value
    $logicAppConnectionValue = $logicAppConnections.value
    $logicAppConnectionValues = $logicAppConnectionValue.psobject.properties.name
    
    #Write-Host "`t Resource Group: $resourceGroupName `t -> `t Logic App: $resourceName"
    Write-Host "`t Resource Group: " -NoNewline; Write-Host $resourceGroupName -ForegroundColor Green -NoNewline; Write-Host "`t -> `t Logic App: " -NoNewline; Write-Host $resourceName -ForegroundColor Green;

    # Iterate through the connectors
    $logicAppConnectionValue.psobject.properties | ForEach-Object {
        $objectName = $_
        $connection = $objectName.Value             
        
        if($connection -ne $null)
        {
            Write-Host "`t `t Uses API Connection: " -NoNewline; Write-Host $connection.connectionName -ForegroundColor Yellow;  
            
            $connectorIdLower = $connection.connectionId.ToLower()
            # Check if connector is in the connector dictionary and Mark connector as being used 
            if($connectorDictionary.ContainsKey($connectorIdLower))
            {
                $matchingConnector = $connectorDictionary[$connectorIdLower]
                $matchingConnector.IsUsed = 'TRUE'
                $connectorDictionary[$connectorIdLower] = $matchingConnector 
            }
        }                                            
   }       
}
Write-Host ''
Write-Host ''


#########################################################
# List All the Orphaned API Connectors and add a tag
# on the Azure artifact as deprecated Yes/No
#########################################################
Write-Host 'List of Orphaned API Connectors'
Write-Host '#########################################################'
# Tag Deprecated to be add or modify on the Azure API Connector
$tags = @{"Deprecated"="Yes"}
$tagsUsed = @{"Deprecated"="No"}

$connectorDictionary.Values | ForEach-Object {
    $azureConnector = $_

    if($azureConnector.IsUsed -eq 'FALSE')
    {
        Write-Host "`t" -NoNewline; Write-Host $azureConnector.Name -ForegroundColor Red -NoNewline; Write-Host " from " -NoNewline; Write-Host $azureConnector.ResourceGroup -ForegroundColor Red -NoNewline; Write-Host " is an orphan";  
        $uptags = Update-AzTag -ResourceId $azureConnector.Id -Tag $tags -Operation Merge
    }
    else {
     $uptags = Update-AzTag -ResourceId $azureConnector.Id -Tag $tagsUsed -Operation Merge
    }
}
exit

#Here the delimiter parameter allows you to set the desired columm delimiter on the CSV file. In some settings this can be "," or ";"
$connectorDictionary.Values | Export-Csv -delimiter ";" -Path C:\Temp\OrphanedConnectorsFull.csv