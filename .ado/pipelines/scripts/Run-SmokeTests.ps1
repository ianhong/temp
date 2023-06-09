param(
  $mode, # "stamp" or "global"
  $smokeTestRetryCount,
  $smokeTestRetryWaitSeconds
)

# -----------
# Load helper functions.
# -----------
. $env:SYSTEM_DEFAULTWORKINGDIRECTORY/.ado/pipelines/scripts/Invoke-WebRequestWithRetry.ps1

# -----------
# Execute smoke tests.
# -----------

if (!("stamp", "global" -eq $mode)) {
  throw "Mode should be either 'stamp' or 'global'."
}

# load json data from downloaded terraform artifacts
$globalInfraDeployOutput = Get-ChildItem $env:PIPELINE_WORKSPACE/terraformOutputGlobalInfra/*.json | Get-Content | ConvertFrom-JSON

# Azure Front Door Endpoint URI
$frontdoorFqdn = $globalInfraDeployOutput.frontdoor_fqdn.value

Write-Output "*******************"
Write-Output "*** SMOKE TESTS ***"
Write-Output "*******************"

# request body needs to be a valid object expected by the API - keep up to date when the contract changes
$post_comment_body = @{
  "authorName" = "Smoke Test Author"
  "text"       = "Just a smoke test"
} | ConvertTo-JSON


# list of targets to test - either all stamps, or one global endpoint
$targets = @()

if ($mode -eq "stamp") {

  # load json data from downloaded terraform artifacts
  $releaseUnitInfraDeployOutput = Get-ChildItem $env:PIPELINE_WORKSPACE/terraformOutputReleaseUnitInfra/*.json | Get-Content | ConvertFrom-JSON
  $privateLinkInfraDeployOutput = Get-ChildItem $env:PIPELINE_WORKSPACE/terraformOutputPrivateLinkInfra/*.json | Get-Content | ConvertFrom-JSON

  $header = @{
    "X-TEST-DATA"  = "true" # Header to indicate that posted comments and rating are just for test and can be deleted again by the app
  }

  # loop through stamps from pipeline artifact json
  foreach ($stamp in $releaseUnitInfraDeployOutput.stamp_properties.value) {
    # from stamp we need:
    # - buildagent_pe_ingress_fqdn = endpoint to be called from the build agent
    # - storage_web_host = ui host

    $privateEndpoint = $privateLinkInfraDeployOutput.stamp_properties.value | Where-Object { $_.location -eq $stamp.location }

    $props = @{
      # Individual Cluster Endpoint FQDN (from pipeline artifact json)
      ApiEndpointFqdn = $privateEndpoint.buildagent_pe_ingress_fqdn
      UiEndpointFqdn  = $stamp.storage_web_host
    }

    $obj = New-Object PSObject -Property $props
    $targets += $obj
  }
}
else {
  $header = @{
    "X-TEST-DATA" = "true"
  }

  $props = @{
    ApiEndpointFqdn = $frontdoorFqdn
    UiEndpointFqdn  = $frontdoorFqdn
  }

  $obj = New-Object PSObject -Property $props
  $targets += $obj
}

Write-Output "*** Testing $($targets.Count) targets"

# loop through targets - either multiple stamps or one front door (global)
foreach ($target in $targets) {

  # shorthand for easier manipulation in strings
  $targetFqdn = $target.ApiEndpointFqdn
  $targetUiFqdn = $target.UiEndpointFqdn

  Write-Output "*** Testing $mode availability using $targetFqdn"

  # test health endpoints for stamps only
  if ($mode -eq "stamp") {
    $stampHealthUrl = "https://$targetFqdn/healthservice/health/stamp"
    Write-Output "*** Call - Stamp Health ($mode)"

    # custom retry loop to handle the situation when the SSL certificate is not valid yet and Invoke-WebRequest throws an exception
    Invoke-WebRequestWithRetry -Uri $stampHealthUrl -Method 'GET' -Headers $header -MaximumRetryCount $smokeTestRetryCount -RetryWaitSeconds $smokeTestRetryWaitSeconds
  }

  $listCatalogUrl = "https://$targetFqdn/catalogservice/api/1.0/catalogitem"
  Write-Output "*** Call - List Catalog ($mode)"
  $responseListCatalog = Invoke-WebRequestWithRetry -Uri $listCatalogUrl -Method 'get' -Headers $header -MaximumRetryCount $smokeTestRetryCount -RetryWaitSeconds $smokeTestRetryWaitSeconds
  $responseListCatalog

  $allItems = $responseListCatalog.Content | ConvertFrom-JSON

  if ($allItems.Count -eq 0) {
    throw "*** No items found in catalog"
  }

  $randomItem = Get-Random $allItems

  $itemUrl = "https://$targetFqdn/catalogservice/api/1.0/catalogitem/$($randomItem.id)"
  Write-Output "*** Call - Get get item ($($randomItem.id)) ($mode)"
  Invoke-WebRequestWithRetry -Uri $itemUrl -Method 'GET' -Headers $header -MaximumRetryCount $smokeTestRetryCount -RetryWaitSeconds $smokeTestRetryWaitSeconds

  $postCommentUrl = "https://$targetFqdn/catalogservice/api/1.0/catalogitem/$($randomItem.id)/comments"
  Write-Output "*** Call - Post new comment to item $($randomItem.id) ($mode)"

  $responsePostComment = Invoke-WebRequestWithRetry -Uri $postCommentUrl -Method 'POST' -Headers $header -Body $post_comment_body -MaximumRetryCount $smokeTestRetryCount -RetryWaitSeconds $smokeTestRetryWaitSeconds -ExpectedResponseCode 202
  $responsePostComment

  Write-Output "*** Sleeping for 10 seconds to give the system time to create the comment"
  Start-Sleep 10

  # The 202-response to POST new comment contains in the 'Location' header the URL under which the new comment will be accessible
  $getCommentPath = $responsePostComment.Headers['Location'][0]

  # The location header does not contain the host part of the URL so we need to prepend it
  $getCommentUrl = "https://$($targetFqdn)$($getCommentPath)"

  Write-Output "*** Call - Get newly created comment ($mode)"
  Invoke-WebRequestWithRetry -Uri $getCommentUrl -Method 'GET' -Headers $header -MaximumRetryCount $smokeTestRetryCount -RetryWaitSeconds $smokeTestRetryWaitSeconds

  Write-Output "*** Call - UI app for $mode"
  $responseUi = Invoke-WebRequestWithRetry -Uri https://$targetUiFqdn -Method 'GET' -MaximumRetryCount $smokeTestRetryCount -RetryWaitSeconds $smokeTestRetryWaitSeconds
  $responseUi

  if (!$responseUi.Content.Contains("<title>Costco ECom UI</title>")) { # Check in the HTML content of the response for a known string (the page title in this case)
    throw "*** Web UI for $targetUiFqdn doesn't contain the expected site title."
  }

  $listInventoryUrl = "https://$targetFqdn/inventoryservice/inventoryservice/api/1.0/Inventory?limit=100"
  Write-Output "*** Call - List Inventory Items ($mode)"
  $responseListInventory = Invoke-WebRequestWithRetry -Uri $listInventoryUrl -Method 'get' -Headers $header -MaximumRetryCount $smokeTestRetryCount -RetryWaitSeconds $smokeTestRetryWaitSeconds
  $responseListInventory

  $allInventoryItems = $responseListInventory.Content | ConvertFrom-JSON

  if ($allInventoryItems.Count -eq 0) {
    throw "*** No items found in inventory service"
  }

  $randomItem = Get-Random $allInventoryItems
}
