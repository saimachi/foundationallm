#!/usr/bin/env pwsh

Param (
    [parameter(Mandatory = $true)][string]$resourceGroup,
    [parameter(Mandatory = $true)][string]$location,
    [parameter(Mandatory = $true)][string]$name,
    [parameter(Mandatory = $true)][string]$keyvaultName,
    [parameter(Mandatory = $false)][string]$configurationFile = "../config/appconfig.json"
)

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

function Invoke-AndRequireSuccess {
    param (
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$Message,

        [Parameter(Mandatory = $true, Position = 1)]
        [ScriptBlock]$ScriptBlock
    )

    Write-Host "${message}..." -ForegroundColor Blue
    $result = & $ScriptBlock

    if ($LASTEXITCODE -ne 0) {
        throw "Failed ${message} (code: ${LASTEXITCODE})"
    }

    return $result
}

Push-Location $($MyInvocation.InvocationName | Split-Path)

Invoke-AndRequireSuccess "Loading AppConfig Values" {
    az appconfig kv import `
        --profile appconfig/kvset `
        --name $name `
        --source file `
        --path $configurationFile `
        --format json `
        --yes `
        --output none
}

Pop-Location
