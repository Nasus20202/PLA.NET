# PowerShell script to download and extract FFmpeg DLLs for GameOfLife video recording
# This script downloads the latest FFmpeg build and extracts the necessary DLLs

$ErrorActionPreference = "Stop"

# Configuration
$ffmpegUrl = "https://github.com/BtbN/FFmpeg-Builds/releases/download/autobuild-2025-10-26-13-25/ffmpeg-n7.1.2-5-g8f77695e65-win64-gpl-shared-7.1.zip"
$tempZipPath = Join-Path $env:TEMP "ffmpeg-build.zip"
$tempExtractPath = Join-Path $env:TEMP "ffmpeg-extract"
$targetDirectory = Join-Path $PSScriptRoot "ffmpeg-lib"

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "FFmpeg DLL Downloader for Game of Life" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

# Clean up any existing temp files
if (Test-Path $tempZipPath) {
    Write-Host "Cleaning up existing temporary files..." -ForegroundColor Yellow
    Remove-Item $tempZipPath -Force
}

if (Test-Path $tempExtractPath) {
    Write-Host "Cleaning up existing temporary extraction folder..." -ForegroundColor Yellow
    Remove-Item $tempExtractPath -Recurse -Force
}

# Create target directory if it doesn't exist
if (-not (Test-Path $targetDirectory)) {
    Write-Host "Creating target directory: $targetDirectory" -ForegroundColor Green
    New-Item -ItemType Directory -Path $targetDirectory | Out-Null
} else {
    Write-Host "Target directory already exists: $targetDirectory" -ForegroundColor Yellow
}

# Download FFmpeg
Write-Host ""
Write-Host "Downloading FFmpeg from GitHub..." -ForegroundColor Green
Write-Host "URL: $ffmpegUrl" -ForegroundColor Gray
try {
    # Use Invoke-WebRequest with progress
    $ProgressPreference = 'SilentlyContinue'  # Speeds up download
    Invoke-WebRequest -Uri $ffmpegUrl -OutFile $tempZipPath
    $ProgressPreference = 'Continue'
    Write-Host "Download complete! Size: $([math]::Round((Get-Item $tempZipPath).Length / 1MB, 2)) MB" -ForegroundColor Green
} catch {
    Write-Host "Error downloading FFmpeg: $_" -ForegroundColor Red
    exit 1
}

# Extract the zip file
Write-Host ""
Write-Host "Extracting archive..." -ForegroundColor Green
try {
    Expand-Archive -Path $tempZipPath -DestinationPath $tempExtractPath -Force
    Write-Host "Extraction complete!" -ForegroundColor Green
} catch {
    Write-Host "Error extracting archive: $_" -ForegroundColor Red
    Remove-Item $tempZipPath -Force -ErrorAction SilentlyContinue
    exit 1
}

# Find the bin directory in the extracted files
Write-Host ""
Write-Host "Locating FFmpeg DLLs..." -ForegroundColor Green
$binDirectory = Get-ChildItem -Path $tempExtractPath -Recurse -Directory | Where-Object { $_.Name -eq "bin" } | Select-Object -First 1

if ($null -eq $binDirectory) {
    Write-Host "Error: Could not find 'bin' directory in the extracted archive!" -ForegroundColor Red
    Remove-Item $tempZipPath -Force -ErrorAction SilentlyContinue
    Remove-Item $tempExtractPath -Recurse -Force -ErrorAction SilentlyContinue
    exit 1
}

Write-Host "Found bin directory: $($binDirectory.FullName)" -ForegroundColor Gray

# Copy DLL files from bin to target directory
Write-Host ""
Write-Host "Copying DLL files to $targetDirectory..." -ForegroundColor Green

$dllFiles = Get-ChildItem -Path $binDirectory.FullName -Filter "*.dll"
$copiedCount = 0

foreach ($dll in $dllFiles) {
    $targetPath = Join-Path $targetDirectory $dll.Name
    Copy-Item -Path $dll.FullName -Destination $targetPath -Force
    Write-Host "  Copied: $($dll.Name)" -ForegroundColor Gray
    $copiedCount++
}

Write-Host ""
Write-Host "Successfully copied $copiedCount DLL files!" -ForegroundColor Green

# List the required DLLs and verify they exist
Write-Host ""
Write-Host "Verifying required DLLs..." -ForegroundColor Yellow
$requiredDlls = @(
    "avcodec-61.dll",
    "avformat-61.dll",
    "avutil-59.dll",
    "swresample-5.dll",
    "swscale-8.dll"
)

$allFound = $true
foreach ($requiredDll in $requiredDlls) {
    $dllPath = Join-Path $targetDirectory $requiredDll
    if (Test-Path $dllPath) {
        Write-Host "  ✓ $requiredDll" -ForegroundColor Green
    } else {
        Write-Host "  ✗ $requiredDll (NOT FOUND)" -ForegroundColor Red
        $allFound = $false
    }
}

# Clean up temporary files
Write-Host ""
Write-Host "Cleaning up temporary files..." -ForegroundColor Yellow
Remove-Item $tempZipPath -Force -ErrorAction SilentlyContinue
Remove-Item $tempExtractPath -Recurse -Force -ErrorAction SilentlyContinue

# Final status
Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
if ($allFound) {
    Write-Host "SUCCESS! All required FFmpeg DLLs are ready!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "  1. Build your project: dotnet build" -ForegroundColor Gray
    Write-Host "  2. Copy DLLs to output: Copy-Item '$targetDirectory\*.dll' 'bin\Debug\net8.0-windows\'" -ForegroundColor Gray
    Write-Host "  3. Run your application and use the recording feature!" -ForegroundColor Gray
} else {
    Write-Host "WARNING: Some required DLLs are missing!" -ForegroundColor Yellow
    Write-Host "The video recording feature may not work correctly." -ForegroundColor Yellow
}
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

