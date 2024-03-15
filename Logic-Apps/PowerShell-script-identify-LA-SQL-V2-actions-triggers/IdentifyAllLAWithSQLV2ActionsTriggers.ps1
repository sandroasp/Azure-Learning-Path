####################################################################################################
# Script to properly identify all SQL version 2 actions and triggers inside Logic Apps Consumption.
# The great thing about this script is that also identifies all nested actions (actions inside 
# other actions like If, Scopes, Switch, and so on)  
#
# Authors: Sandro Pereira and Luis Rigueira
####################################################################################################

# Authenticate to Azure
Connect-AzAccount

# Select the appropriate subscription if you have multiple subscriptions
#Set-AzContext -SubscriptionId $subscriptionId

# Function to extract actions recursively
function Get-Actions {
    param (
        $node
    )
    $actions = @()

    foreach ($key in $node.psobject.Properties.Name) {
        if ($node.$key.type -eq "ApiConnection") {
            if ($node.$key.inputs.path -like "*/v2/datasets*" -and $node.$key.inputs.host.connection.name -like "*sql*") {
                $actions += $key
            }
        } elseif ($node.$key -is [System.Management.Automation.PSCustomObject]) {
            $actions += Get-Actions -node $node.$key
        }
    }

    return $actions
}

# Retrieve all Logic Apps within the subscription
$logicApps = Get-AzResource -ResourceType Microsoft.Logic/workflows

# Iterate through each Logic App and extract actions
foreach ($logicApp in $logicApps) {
    # Retrieve Logic App definition
    $logicAppDefinition = Get-AzResource -ResourceId $logicApp.ResourceId -ExpandProperties

    # Extract actions from the Logic App definition
    $allActions = Get-Actions -node $logicAppDefinition.Properties.Definition.actions

    # Display the Logic App name if filtered actions were found
    if ($allActions.Count -gt 0) {
        Write-Host "Logic App: $($logicApp.Name) - RG: $($logicApp.ResourceGroupName)" -ForegroundColor Red
 
        # Display the list of filtered actions
        Write-Host "Filtered Actions:"

        $allActions
        Write-Host ""
    }
}