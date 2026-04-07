# Chocolatey uninstallation script for DeleteOld
# This script is executed when the package is uninstalled via 'choco uninstall deleteold'

$ErrorActionPreference = 'Stop';

Write-Host "Uninstalling DeleteOld..."

# Get the lib directory
$libDir = "$env:ChocolateyInstall\lib\deleteold"
$binDir = "$libDir\bin"
$exePath = Join-Path $binDir "DeleteOld.exe"

# Remove the executable if it exists
if (Test-Path $exePath) {
    Remove-Item -Path $exePath -Force | Out-Null
    Write-Host "Deleted $exePath"
}

# Remove the bin directory if empty
if ((Test-Path $binDir) -and ((Get-ChildItem -Path $binDir | Measure-Object).Count -eq 0)) {
    Remove-Item -Path $binDir -Force | Out-Null
    Write-Host "Cleaned up empty bin directory"
}

Write-Host "DeleteOld has been uninstalled successfully."
