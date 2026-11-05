[CmdletBinding(SupportsShouldProcess)]
param(
    [string] $OpenTabletDriverData = "$env:LOCALAPPDATA\OpenTabletDriver",
    [switch] $Force
)

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$assembly = Join-Path $root 'dist\Plugin\OpenTabletDriver.FansjoyS1Pro.dll'
$configuration = Join-Path $root 'dist\Fansjoy S1 Pro.json'

if (!(Test-Path $assembly) -or !(Test-Path $configuration)) {
    & (Join-Path $root 'Build.ps1')
}

$running = Get-Process -ErrorAction SilentlyContinue | Where-Object {
    $_.ProcessName -like 'OpenTabletDriver*' -or $_.ProcessName -eq 'PenDriver'
}
if ($running) {
    $names = ($running.ProcessName | Sort-Object -Unique) -join ', '
    throw "Close these tablet-driver processes before installing: $names"
}

$configurationDirectory = Join-Path $OpenTabletDriverData 'Configurations'
$pluginDirectory = Join-Path $OpenTabletDriverData 'Plugins\FansjoyS1Pro'
$configurationTarget = Join-Path $configurationDirectory 'Fansjoy S1 Pro.json'
$assemblyTarget = Join-Path $pluginDirectory 'OpenTabletDriver.FansjoyS1Pro.dll'

$existing = @($configurationTarget, $assemblyTarget) | Where-Object { Test-Path $_ }
if ($existing -and !$Force) {
    throw "Support files already exist. Re-run with -Force to replace them: $($existing -join ', ')"
}

if ($PSCmdlet.ShouldProcess($OpenTabletDriverData, 'Install Fansjoy S1 Pro support')) {
    New-Item -ItemType Directory -Force -Path $configurationDirectory, $pluginDirectory | Out-Null
    Copy-Item $configuration $configurationTarget -Force
    Copy-Item $assembly $assemblyTarget -Force
    Write-Host 'Installed Fansjoy S1 Pro support.'
    Write-Host 'Start OpenTabletDriver and run Detect Tablet.'
}
