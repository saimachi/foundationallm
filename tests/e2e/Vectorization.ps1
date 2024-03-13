#!/bin/pwsh
Param (
    [parameter(Mandatory = $true)][string]$azdEnvFilePath,
    [parameter(Mandatory = $true)][string]$entraManagementRefreshToken
)

Get-Content $azdEnvFilePath | ForEach-Object {
    $envVar = $_ -split "="
    $configurationParameters[$envVar[0]] = $envVar[1]
}

# Test the Status of Vectorization API & Management API
$managementStatusCode = Invoke-WebRequest `
    -Uri "$($configurationParameters['SERVICE_MANAGEMENT_API_ENDPOINT_URL'])/status" `
    -UseBasicParsing

if ($managementStatusCode.StatusCode -ne 200) {
    throw "Management API Status: $($managementStatusCode.Content)"
}

$vectorizationStatusRequest = Invoke-WebRequest `
    -Uri "$($configurationParameters['SERVICE_VECTORIZATION_API_ENDPOINT_URL'])/status" `
    -UseBasicParsing

if ($vectorizationStatusRequest.StatusCode -ne 200) {
    throw "Vectorization API Failed: $($vectorizationStatusRequest.Content)"
}

# Get Entra Token
$accessToken = curl "https://login.microsoftonline.com/$($configurationParameters['ENTRA_MANAGEMENT_UI_TENANT_ID'])/oauth2/v2.0/token" `
-X POST `
-H 'Content-Type: application/x-www-form-urlencoded' `
-H "Origin: $($configurationParameters['SERVICE_MANAGEMENT_UI_ENDPOINT_URL'].Substring(8))" `
-d "client_id=$($configurationParameters['ENTRA_MANAGEMENT_UI_CLIENT_ID'])" `
-d 'grant_type=refresh_token' `
-d "refresh_token=$($entraManagementRefreshToken)" | ConvertFrom-Json | Select-Object -ExpandProperty access_token

# Write-Host "Entra Access Token: $accessToken"

$mgmtUrlBasePath = "$($configurationParameters['SERVICE_MANAGEMENT_API_ENDPOINT_URL'])/instances/$($configurationParameters['FOUNDATIONALLM_INSTANCE_ID'])"

# Create Content Source Profile
$contentSourceProfileRequest = Invoke-WebRequest `
    -Uri "$mgmtUrlBasePath/providers/FoundationaLLM.Vectorization/contentsourceprofiles/DefaultAzureDataLake_Testing" `
    -Method POST `
    -Headers @{
        "Content-Type" = "application/json"
        "Authorization" = "Bearer $accessToken"
    } `
    -Body (Get-Content "./tests/e2e/requests/content-source-profile.json" | ConvertFrom-Json | ConvertTo-Json)

if ($contentSourceProfileRequest.StatusCode -ne 200) {
    throw "Content Source Profile Creation Failed: $($contentSourceProfileRequest.Content)"
}

# Create Indexing Profile
$indexingProfileRequest = Invoke-WebRequest `
    -Uri "$mgmtUrlBasePath/providers/FoundationaLLM.Vectorization/indexingprofiles/AzureAISearch_Testing" `
    -Method POST `
    -Headers @{
        "Content-Type" = "application/json"
        "Authorization" = "Bearer $accessToken"
    } `
    -Body (Get-Content "./tests/e2e/requests/indexing-profile.json" | ConvertFrom-Json | ConvertTo-Json)

if ($indexingProfileRequest.StatusCode -ne 200) {
    throw "Indexing Profile Creation Failed: $($indexingProfileRequest.Content)"
}

# Create Text Embedding Profile
$embeddingProfileRequest = Invoke-WebRequest `
    -Uri "$mgmtUrlBasePath/providers/FoundationaLLM.Vectorization/textembeddingprofiles/AzureOpenAI_Embedding_Testing" `
    -Method POST `
    -Headers @{
        "Content-Type" = "application/json"
        "Authorization" = "Bearer $accessToken"
    } `
    -Body (Get-Content "./tests/e2e/requests/embedding-profile.json" | ConvertFrom-Json | ConvertTo-Json)

if ($embeddingProfileRequest.StatusCode -ne 200) {
    throw "Embedding Profile Creation Failed: $($embeddingProfileRequest.Content)"
}

# Create Text Partitioning Profile
$partitioningProfileRequest = Invoke-WebRequest `
    -Uri "$mgmtUrlBasePath/providers/FoundationaLLM.Vectorization/textpartitioningprofiles/DefaultTokenTextPartition_Testing" `
    -Method POST `
    -Headers @{
        "Content-Type" = "application/json"
        "Authorization" = "Bearer $accessToken"
    } `
    -Body (Get-Content "./tests/e2e/requests/partitioning-profile.json" | ConvertFrom-Json | ConvertTo-Json)

if ($partitioningProfileRequest.StatusCode -ne 200) {
    throw "Embedding Profile Creation Failed: $($partitioningProfileRequest.Content)"
}

$vectorizationFileParameters = $env['VECTORIZATION_FILE_PARAMETERS'] | ConvertFrom-Json

az storage blob download `
    --account-name $vectorizationFileParameters.account_name `
    --container-name $vectorizationFileParameters.container_name `
    --name $vectorizationFileParameters.blob_name `
    --file $vectorizationFileParameters.blob_output_name

az storage blob upload `
    --account-name $configurationParameters['AZURE_STORAGE_ACCOUNT_NAME'] `
    --container-name 'vectorization-input' `
    --name $vectorizationFileParameters.blob_output_name `
    --file $vectorizationFileParameters.blob_output_name

# Get Vectorization API key from App Config
$apiKey = $(az keyvault secret show --name "foundationallm-apis-vectorizationapi-apikey" --vault-name $configurationParameters["AZURE_KEY_VAULT_NAME"] --query "value" -o tsv)

$vectorizationRequestJson = $(Get-Content "./tests/e2e/requests/vectorization.json" -RAW)
$vectorizationRequestJson = $vectorizationRequestJson `
    -replace '#AZURE_STORAGE_ACCOUNT_NAME#', $configurationParameters['AZURE_STORAGE_ACCOUNT_NAME'] `
    -replace '#VECTORIZATION_INPUT_FILE_NAME#', $vectorizationFileParameters.blob_output_name

# Trigger Vectorization request
curl "$($configurationParameters['SERVICE_VECTORIZATION_API_ENDPOINT_URL'])/vectorizationrequest" `
    -X POST `
    -H "Content-Type: application/json" `
    -H "X-API-KEY: $apiKey" `
    -d $vectorizationRequestJson