#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs tests with code coverage and generates HTML reports.

.DESCRIPTION
    This script runs all .NET tests with code coverage enabled using Coverlet,
    then generates HTML reports using ReportGenerator.

.PARAMETER OutputDir
    Directory where coverage reports will be saved. Default: ./coverage

.PARAMETER OpenReport
    Opens the HTML report in the default browser after generation.

.EXAMPLE
    ./run-coverage.ps1
    
.EXAMPLE
    ./run-coverage.ps1 -OpenReport
#>

param(
    [string]$OutputDir = "./coverage",
    [switch]$OpenReport
)

$ErrorActionPreference = "Stop"

# Colors for output
function Write-Info { param($msg) Write-Host "ℹ️  $msg" -ForegroundColor Cyan }
function Write-Success { param($msg) Write-Host "✅ $msg" -ForegroundColor Green }
function Write-Warning { param($msg) Write-Host "⚠️  $msg" -ForegroundColor Yellow }
function Write-Error { param($msg) Write-Host "❌ $msg" -ForegroundColor Red }

$projectRoot = Split-Path -Parent $PSScriptRoot
$testProject = Join-Path $projectRoot "tests/CardDemo.Tests/CardDemo.Tests.csproj"
$runSettings = Join-Path $projectRoot "tests/CardDemo.Tests/coverlet.runsettings"
$coverageDir = Join-Path $projectRoot $OutputDir
$reportDir = Join-Path $coverageDir "report"

Write-Info "Starting code coverage analysis..."
Write-Info "Project root: $projectRoot"

# Clean previous coverage data
if (Test-Path $coverageDir) {
    Write-Info "Cleaning previous coverage data..."
    Remove-Item -Recurse -Force $coverageDir
}
New-Item -ItemType Directory -Path $coverageDir -Force | Out-Null

# Run tests with coverage
Write-Info "Running tests with code coverage..."
$testResultsDir = Join-Path $coverageDir "TestResults"

dotnet test $testProject `
    --configuration Release `
    --collect:"XPlat Code Coverage" `
    --settings $runSettings `
    --results-directory $testResultsDir `
    --verbosity minimal

if ($LASTEXITCODE -ne 0) {
    Write-Error "Tests failed!"
    exit $LASTEXITCODE
}

Write-Success "Tests completed successfully!"

# Find the coverage file
$coverageFile = Get-ChildItem -Path $testResultsDir -Recurse -Filter "coverage.cobertura.xml" | Select-Object -First 1

if (-not $coverageFile) {
    Write-Error "Coverage file not found!"
    exit 1
}

Write-Info "Coverage file: $($coverageFile.FullName)"

# Copy coverage file to output directory
Copy-Item $coverageFile.FullName -Destination (Join-Path $coverageDir "coverage.cobertura.xml")

# Install ReportGenerator tool if not installed
$reportGenTool = Get-Command reportgenerator -ErrorAction SilentlyContinue
if (-not $reportGenTool) {
    Write-Info "Installing ReportGenerator tool..."
    dotnet tool install --global dotnet-reportgenerator-globaltool
}

# Generate HTML report
Write-Info "Generating HTML coverage report..."
reportgenerator `
    -reports:$($coverageFile.FullName) `
    -targetdir:$reportDir `
    -reporttypes:"Html;HtmlSummary;Badges;TextSummary;Cobertura" `
    -title:"CardDemo Code Coverage Report" `
    -assemblyfilters:"+CardDemo.*;-CardDemo.Tests" `
    -classfilters:"-*Migrations*;-*Program"

if ($LASTEXITCODE -ne 0) {
    Write-Error "Report generation failed!"
    exit $LASTEXITCODE
}

Write-Success "Coverage report generated at: $reportDir"

# Display summary
$summaryFile = Join-Path $reportDir "Summary.txt"
if (Test-Path $summaryFile) {
    Write-Host ""
    Write-Host "📊 Coverage Summary:" -ForegroundColor Magenta
    Write-Host "─────────────────────────────────────────" -ForegroundColor DarkGray
    Get-Content $summaryFile | ForEach-Object { Write-Host $_ }
    Write-Host "─────────────────────────────────────────" -ForegroundColor DarkGray
}

# Open report if requested
if ($OpenReport) {
    $indexFile = Join-Path $reportDir "index.html"
    if (Test-Path $indexFile) {
        Write-Info "Opening coverage report in browser..."
        Start-Process $indexFile
    }
}

Write-Host ""
Write-Success "Code coverage analysis complete!"
Write-Host "📁 Report location: $reportDir/index.html" -ForegroundColor Yellow
