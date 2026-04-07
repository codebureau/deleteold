# Chocolatey installation script for DeleteOld
# This script is executed when the package is installed via 'choco install deleteold'

$ErrorActionPreference = 'Stop';

# Get the package directory
$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"

# Output information
Write-Host "Installing DeleteOld..."

# Create a bin directory in the Chocolatey lib directory
$libDir = "$env:ChocolateyInstall\lib\deleteold"
$binDir = "$libDir\bin"

if (-not (Test-Path $binDir)) {
    New-Item -ItemType Directory -Path $binDir | Out-Null
}

# Copy the executable
$exePath = Join-Path $toolsDir "DeleteOld.exe"
$targetExePath = Join-Path $binDir "DeleteOld.exe"

if (Test-Path $exePath) {
    Copy-Item -Path $exePath -Destination $targetExePath -Force | Out-Null
    Write-Host "DeleteOld executable installed to $targetExePath"
} else {
    Write-Warning "DeleteOld.exe not found in $toolsDir"
}

# The Chocolatey shim will automatically add this to PATH
# Users can now run 'DeleteOld' from any PowerShell/Command Prompt window
Write-Host "DeleteOld is ready to use! Type 'DeleteOld --help' for usage information."
