# SonarQube Local Analysis Script
# Run this script to analyze your code locally with SonarQube

param(
    [string]$SonarQubeUrl = "http://localhost:9000",
    [string]$SonarToken = "",
    [string]$ProjectKey = "csim_koalas"
)

Write-Host "Starting SonarQube analysis for Koalas project..." -ForegroundColor Green

# Install SonarScanner if not present
if (!(Get-Command "dotnet-sonarscanner" -ErrorAction SilentlyContinue)) {
    Write-Host "Installing SonarScanner..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-sonarscanner
}

# Install coverage tools if not present
if (!(Get-Command "reportgenerator" -ErrorAction SilentlyContinue)) {
    Write-Host "Installing ReportGenerator..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-reportgenerator-globaltool
}

try {
    # Start SonarScanner
    Write-Host "Starting SonarScanner..." -ForegroundColor Yellow
    if ($SonarToken) {
        dotnet sonarscanner begin /k:$ProjectKey /d:sonar.host.url=$SonarQubeUrl /d:sonar.token=$SonarToken /d:sonar.cs.opencover.reportsPaths="coverage/coverage.opencover.xml"
    } else {
        dotnet sonarscanner begin /k:$ProjectKey /d:sonar.host.url=$SonarQubeUrl /d:sonar.cs.opencover.reportsPaths="coverage/coverage.opencover.xml"
    }

    # Clean and build
    Write-Host "Building solution..." -ForegroundColor Yellow
    dotnet clean ./src/Koalas.sln
    dotnet build ./src/Koalas.sln --configuration Release

    # Run tests with coverage
    Write-Host "Running tests with coverage..." -ForegroundColor Yellow
    dotnet test ./src/Koalas.sln --configuration Release --no-build --collect:"XPlat Code Coverage" --results-directory ./coverage/raw

    # Convert coverage to OpenCover format
    Write-Host "Converting coverage reports..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Force -Path ./coverage
    reportgenerator -reports:"./coverage/raw/**/coverage.cobertura.xml" -targetdir:"./coverage" -reporttypes:"OpenCover"

    # End SonarScanner
    Write-Host "Finishing SonarScanner analysis..." -ForegroundColor Yellow
    if ($SonarToken) {
        dotnet sonarscanner end /d:sonar.token=$SonarToken
    } else {
        dotnet sonarscanner end
    }

    Write-Host "SonarQube analysis completed!" -ForegroundColor Green
    Write-Host "Check your SonarQube dashboard at: $SonarQubeUrl" -ForegroundColor Cyan
}
catch {
    Write-Error "SonarQube analysis failed: $_"
    exit 1
}