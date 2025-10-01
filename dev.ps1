param(
    [Parameter(Mandatory = $false, ValueFromRemainingArguments = $true)]
    [ValidateSet("format", "clean", "build", "test", "all")]
    [ArgumentCompleter({
        param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameters)
        $actions = @("format", "clean", "build", "test", "all")
        $actions | Where-Object { $_ -like "$wordToComplete*" } | ForEach-Object {
            [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_)
        }
    })]
    [string[]]$Actions
)

# If no actions provided, show usage
if (-not $Actions -or $Actions.Count -eq 0) {
    Write-Host "Usage: .\dev.ps1 <action1> [action2] [action3] ..." -ForegroundColor Yellow
    Write-Host "Available actions: format, clean, build, test, all" -ForegroundColor Yellow
    Write-Host "Examples:" -ForegroundColor Yellow
    Write-Host "  .\dev.ps1 format" -ForegroundColor Gray
    Write-Host "  .\dev.ps1 format build" -ForegroundColor Gray
    Write-Host "  .\dev.ps1 clean build test" -ForegroundColor Gray
    Write-Host "  .\dev.ps1 all" -ForegroundColor Gray
    exit 1
}

$src = Join-Path $PSScriptRoot "src"
$artifacts = Join-Path $PSScriptRoot "artifacts"
$solution = Join-Path $src "Koalas.sln"
$solution = "$src/Koalas.sln"


function Format-Code {
    Write-Host "Formatting code..." -ForegroundColor Green
    dotnet csharpier format $src --log-format Console
}

function Clean-Solution {
    Write-Host "Cleaning solution..." -ForegroundColor Green
    dotnet clean $solution -v:detailed --nologo
    rm -rf $artifacts
}

function Build-Solution {
    Write-Host "Building solution..." -ForegroundColor Green
    dotnet build $solution --no-incremental -v:detailed --nologo
}

function Test-Solution {
    Write-Host "Running tests..." -ForegroundColor Green
    dotnet test $solution -v:detailed --nologo
    #dotnet watch test $solution -v:detailed --nologo
    #dotnet watch --project $solution test -v:detailed --nologo
}

foreach ($Action in $Actions) {
    switch ($Action) {
        "format" { Format-Code }
        "clean" { Clean-Solution }
        "build" { Build-Solution }
        "test" { Test-Solution }
        "all" {
            Format-Code
            Clean-Solution
            Build-Solution
            Test-Solution
        }
    }
}
