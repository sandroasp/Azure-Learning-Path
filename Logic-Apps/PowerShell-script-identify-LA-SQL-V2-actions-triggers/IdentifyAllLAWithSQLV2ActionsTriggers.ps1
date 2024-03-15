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

# Function to extract actions and triggers recursively
function Get-ActionsAndTriggers {
    param (
        $node
    )
    $actionsAndTriggers = @()
    foreach ($key in $node.psobject.Properties.Name) {
        if ($node.$key.type -eq "ApiConnection") {
            if ($node.$key.inputs.path -like "*v2/datasets/*" -and $node.$key.inputs.host.connection.name -like "*sql*") {
                $actionsAndTriggers += $key
            }
        } elseif ($node.$key -is [System.Management.Automation.PSCustomObject]) {
            $actionsAndTriggers += Get-ActionsAndTriggers -node $node.$key
        }
    }
    return $actionsAndTriggers
}
 
# Retrieve all Logic Apps within the subscription
$logicApps = Get-AzResource -ResourceType Microsoft.Logic/workflows
 
# Iterate through each Logic App and extract actions and triggers
foreach ($logicApp in $logicApps) {
    # Retrieve Logic App definition
    $logicAppDefinition = Get-AzResource -ResourceId $logicApp.ResourceId -ExpandProperties
 
    # Extract actions and triggers from the Logic App definition
    $allActionsAndTriggers = Get-ActionsAndTriggers -node $logicAppDefinition.Properties.Definition.triggers
    $allActionsAndTriggers += Get-ActionsAndTriggers -node $logicAppDefinition.Properties.Definition.actions
 
    # Display the Logic App name if filtered actions and triggers were found
    if ($allActionsAndTriggers.Count -gt 0) {
        Write-Host "Logic App: $($logicApp.Name) - RG: $($logicApp.ResourceGroupName)" -ForegroundColor Red
        # Display the list of filtered actions and triggers
        Write-Host "Filtered Actions and Triggers:"
        $allActionsAndTriggers
        Write-Host ""
    }
}