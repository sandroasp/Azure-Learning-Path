####################################################################################################
# Script to properly export all Logic Apps Runs History for a specific Logic App Consumption
#
# Authors: Luis Rigueira
####################################################################################################

# Replace these variables with your actual values
$resourceGroupName = "YOUR RESOURCE GROUP NAME"
$logicAppName = " YOUR LOGIC APP NAME "
$outputDirectory = "D:\Output\Path"

# Authenticate to Azure interactively
Connect-AzAccount

# Function to fetch Logic App runs based on URL
function FetchRuns($url) {
    $runs = Invoke-RestMethod -Uri $url -Method Get -Headers @{Authorization = "Bearer $token"}
    return $runs
}

# Function to fetch Logic App actions based on URL
function FetchActions($url) {
    $actions = Invoke-RestMethod -Uri $url -Method Get -Headers @{Authorization = "Bearer $token"}
    return $actions
}

# Retrieve Logic App runs using Azure REST API
$subscriptionId = (Get-AzContext).Subscription.Id
$token = (Get-AzAccessToken).Token
$baseUrl = "https://management.azure.com/subscriptions/$subscriptionId/resourceGroups/$resourceGroupName/providers/Microsoft.Logic/workflows/$logicAppName/runs?api-version=2016-06-01&`$top=250"
$url = $baseUrl

# Continuously fetch and export runs until no more runs are available
do {
    # Fetch runs
    $runs = FetchRuns -url $url

    # Export runs to .json files
    foreach ($run in $runs.value) {
        # Retrieve the start time of the run and format it appropriately
        $runStartTime = Get-Date $run.properties.startTime
        $formattedStartTime = $runStartTime.ToString("yyyy-MM-dd-HH.mm.ss")

        # Construct a unique identifier for the run
        $runId = "$logicAppName" + "_" + $run.name + "_" + $formattedStartTime

        # Check if the run was successful or failed and modify the runId accordingly
        if ($run.properties.status -eq "Succeeded") {
            $runId += "_Succeeded"
        } elseif ($run.properties.status -eq "Failed") {
            $runId += "_Failed"
        }

        # Fetch actions for this run
        $actionsUrl = "https://management.azure.com/subscriptions/$subscriptionId/resourceGroups/$resourceGroupName/providers/Microsoft.Logic/workflows/$logicAppName/runs/$($run.name)/actions/?api-version=2016-10-01&`$top=250"
        $actions = FetchActions -url $actionsUrl
        
        # Convert run and actions to JSON format
        $runJson = $run
        $actionsJson = $actions.value

        # Create combined object with "Run Details" and "Actions" keys in the desired order
        $combinedJson = [ordered]@{
            "Run Details" = $runJson
            "Actions" = $actionsJson
        }

        # Convert combined object to JSON string
        $combinedContent = $combinedJson | ConvertTo-Json -Depth 10

        # Convert combined object to JSON string with "Run Details" first
        $combinedContent = $combinedJson | ConvertTo-Json -Depth 10

        # Export combined JSON content to .json file
        $outputFile = Join-Path -Path $outputDirectory -ChildPath "$runId.json"
        $combinedContent | Out-File -FilePath $outputFile -Encoding utf8
        Write-Host "Run details and actions for ID $runId exported to $outputFile"
    }

    # Check if there are more runs available
    if ($runs.nextLink) {
        $url = $runs.nextLink
    } else {
        # No more runs available, exit the loop
        break
    }
} while ($true)
