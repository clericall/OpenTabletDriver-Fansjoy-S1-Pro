[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$project = Join-Path $root 'src\OpenTabletDriver.FansjoyS1Pro\OpenTabletDriver.FansjoyS1Pro.csproj'
$output = Join-Path $root 'dist'
$pluginOutput = Join-Path $output 'Plugin'
$assembly = Join-Path $root "src\OpenTabletDriver.FansjoyS1Pro\bin\$Configuration\net10.0\OpenTabletDriver.FansjoyS1Pro.dll"
$pluginZip = Join-Path $output 'OpenTabletDriver.FansjoyS1Pro.zip'

dotnet build $project --configuration $Configuration
if ($LASTEXITCODE -ne 0) {
    throw "Plugin build failed with exit code $LASTEXITCODE."
}

New-Item -ItemType Directory -Force -Path $output, $pluginOutput | Out-Null
Copy-Item $assembly $pluginOutput -Force
Copy-Item (Join-Path $root 'Configurations\Fansjoy S1 Pro.json') $output -Force

if (Test-Path $pluginZip) {
    Remove-Item $pluginZip -Force
}
Compress-Archive -Path (Join-Path $pluginOutput 'OpenTabletDriver.FansjoyS1Pro.dll') -DestinationPath $pluginZip

Write-Host "Built support package in '$output'."
