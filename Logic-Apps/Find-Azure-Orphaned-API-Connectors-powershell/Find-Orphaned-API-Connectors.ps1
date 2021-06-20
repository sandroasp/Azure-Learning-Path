#########################################################
#                                                       #
# Find Azure Orphaned API Connectors                    #
# Author(s):                                            #
#    Mike Stephenson                                    #
#    Sandro Pereira                                     #
#                                                       #
#########################################################

#########################################################
# Azure Configurations
#########################################################
$subscriptioName = '<subscription-name>'
$resourceGroupName = '<resource-name>'

#use the collection to build up objects for the table
$connectorDictionary = New-Object "System.Collections.Generic.Dictionary``2[System.String,System.Object]" 

#########################################################
# Perform Azure Login
#########################################################
# az login
# az account set --subscription $subscriptioname
Connect-AzAccount

#########################################################
# Get Subscription Info
#########################################################
$subscription = Get-AzSubscription -SubscriptionName $subscriptioName
$subscriptionId = $subscription.Id
Write-Host 'Subscription Id: ' $subscriptionId

#########################################################
# Get Resource Group Info
#########################################################
$resourceGroup = Get-AzResourceGroup -Name $resourceGroupName
$resourceGroupPath = $resourceGroup.ResourceId
Write-Host 'Resource Group Path: '  $resourceGroupPath

##############################################################
# Get list of API Connectors available on the Resource Group
##############################################################
Write-Host 'Looking up API Connectors'
$resourceName = ''
$resources = Get-AzResource -ResourceGroupName $resourcegroupName -ResourceType Microsoft.Web/connections
$resources | ForEach-Object {     
    $resourceName = $_.Name
    Write-Host 'Found connector: '$_.Name
    # Write-Host 'Found connector: '$_.id
    
    $resourceIdLower = $_.id.ToLower()

    $azureConnector = New-Object -TypeName psobject
    $azureConnector | Add-Member -MemberType NoteProperty -Name 'Id' -Value $_.Id
    $azureConnector | Add-Member -MemberType NoteProperty -Name 'Name' -Value $_.Name
    $azureConnector | Add-Member -MemberType NoteProperty -Name 'IsUsed' -Value 'FALSE'

    $connectorDictionary.Add($resourceIdLower, $azureConnector)  
}
Write-Host ''
Write-Host ''
Write-Host ''

#########################################################
# Check logic apps to find orphaned connectors
#########################################################
Write-Host 'Looking up Logic Apps'
$resources = Get-AzResource -ResourceGroupName $resourcegroupName -ResourceType Microsoft.Logic/workflows
$resources | ForEach-Object {     
    $resourceName = $_.Name    
    $logicAppName = $resourceName
    $logicApp = Get-AzLogicApp -Name $logicAppName -ResourceGroupName $resourceGroupName        
    $logicAppUrl = $resourceGroupPath + '/providers/Microsoft.Logic/workflows/' + $logicApp.Name + '?api-version=2019-05-01'
    
    # Get Logic App Content
    $logicAppJson = az rest --method get --uri $logicAppUrl
    $logicAppJsonText = $logicAppJson | ConvertFrom-Json    

    # Check Logic App Connectors
    $logicAppParameters = $logicAppJsonText.properties.parameters
    $logicAppConnections = $logicAppParameters.psobject.properties.Where({$_.name -eq '$connections'}).value
    $logicAppConnectionValue = $logicAppConnections.value
    $logicAppConnectionValues = $logicAppConnectionValue.psobject.properties.name
    
    Write-Host 'Logic App: ' $logicAppName

    # Iterate through the connectors
    $logicAppConnectionValue.psobject.properties | ForEach-Object {
        $objectName = $_
        $connection = $objectName.Value             
        
        if($connection -ne $null)
        {
            Write-Host '     Uses connector: name='$connection.connectionName        
            
            $connectorIdLower = $connection.connectionId.ToLower()
            # Check if connector is in the connector dictionary and Mark connector as being used 
            if($connectorDictionary.ContainsKey($connectorIdLower))
            {
                $matchingConnector = $connectorDictionary[$connectorIdLower]
                $matchingConnector.IsUsed = 'TRUE'
                $connectorDictionary[$connectorIdLower] = $matchingConnector 
                #Write-Host 'Marking connector as used: ' $connectorIdLower
            }
        }                                            
   }       
}
Write-Host ''
Write-Host ''
Write-Host ''


#########################################################
# List All the Orphaned API Connectors and add a tag
# on the Azure artifact as deprecated Yes/No
#########################################################
Write-Host 'Orphaned API Connectors'
# Tag Deprecated to be add or modify on the Azure API Connector
$tags = @{"Deprecated"="Yes"}
$tagsUsed = @{"Deprecated"="No"}

$connectorDictionary.Values | ForEach-Object {
    $azureConnector = $_

    #Write-Host $azureConnector.Id ' : ' $azureConnector.IsUsed
    if($azureConnector.IsUsed -eq 'FALSE')
    {
        Write-Host $azureConnector.Name ' : is an orphan'
        $uptags = Update-AzTag -ResourceId $azureConnector.Id -Tag $tags -Operation Merge
    }
    else {
     $uptags = Update-AzTag -ResourceId $azureConnector.Id -Tag $tagsUsed -Operation Merge
    }
}

#Here the delimiter parameter allows you to set the desired columm delimiter on the CSV file. In some settings this can be "," or ";"
$connectorDictionary.Values | Export-Csv -delimiter ";" -Path C:\Temp\OrphanedConnectors.csv