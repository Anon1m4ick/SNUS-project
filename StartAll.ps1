# StartAll.ps1
# Starts server and client sensor processes using paths relative to the script file.

# Resolve script directory (works when run as a script)
$scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Definition }

function Find-Exe {
    param([string[]]$relativePaths)
    foreach ($rel in $relativePaths) {
        $full = Join-Path $scriptDir $rel
        if (Test-Path $full) { return $full }
    }
    return $null
}

# Candidate paths for server and client (add other TFMs/configurations if needed)
$serverCandidates = @(
    "ProjectSNUS\bin\Debug\ProjectSNUS.exe",
    "ProjectSNUS\bin\Debug\net472\ProjectSNUS.exe",
    "ProjectSNUS\bin\Release\ProjectSNUS.exe",
    "ProjectSNUS\bin\Release\net472\ProjectSNUS.exe"
)

$clientCandidates = @(
    "Client1\bin\Debug\Client1.exe",
    "Client1\bin\Debug\net472\Client1.exe",
    "Client1\bin\Release\Client1.exe",
    "Client1\bin\Release\net472\Client1.exe"
)

$serverPath = Find-Exe -relativePaths $serverCandidates
$clientPath = Find-Exe -relativePaths $clientCandidates

if (-not $serverPath) {
    Write-Host "ERROR: Server not built. Checked paths:" -ForegroundColor Red
    $serverCandidates | ForEach-Object { Write-Host " - $($_)" }
    Write-Host "Run: msbuild ProjectSNUS.sln /p:Configuration=Debug" -ForegroundColor Yellow
    exit 1
}

if (-not $clientPath) {
    Write-Host "ERROR: Client not built. Checked paths:" -ForegroundColor Red
    $clientCandidates | ForEach-Object { Write-Host " - $($_)" }
    Write-Host "Run: msbuild ProjectSNUS.sln /p:Configuration=Debug" -ForegroundColor Yellow
    exit 1
}

Write-Host "Starting WCF Server..." -ForegroundColor Yellow
$serverProcess = Start-Process -FilePath $serverPath -WorkingDirectory (Split-Path $serverPath) -PassThru
Start-Sleep -Seconds 3
Write-Host "? Server started (PID: $($serverProcess.Id))" -ForegroundColor Green
Write-Host ""

Write-Host "Starting Sensors..." -ForegroundColor Yellow
for ($i = 0; $i -lt 4; $i++) {
    Write-Host "[$([int]($i+1))/4] Starting Sensor $i..." -ForegroundColor Cyan
    Start-Sleep -Milliseconds 300
    $sensor = Start-Process -FilePath $clientPath -ArgumentList "$i" -WorkingDirectory (Split-Path $clientPath) -PassThru
    Write-Host "  -> PID: $($sensor.Id)" -ForegroundColor Gray
    Start-Sleep -Seconds 1
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "   All components started (if no errors)!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Server path: $serverPath" -ForegroundColor White
Write-Host "Client path: $clientPath" -ForegroundColor White
Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
