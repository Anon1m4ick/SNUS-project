# PowerShell script to start all components
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "   SNUS Project - Causal Broadcast System" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Check if binaries exist
if (!(Test-Path "ProjectSNUS\bin\Debug\ProjectSNUS.exe")) {
    Write-Host "ERROR: Server not built! Please build the solution first." -ForegroundColor Red
    Write-Host "Run: msbuild ProjectSNUS.sln /p:Configuration=Debug" -ForegroundColor Yellow
  pause
    exit
}

if (!(Test-Path "Client1\bin\Debug\Client1.exe")) {
    Write-Host "ERROR: Client not built! Please build the solution first." -ForegroundColor Red
    Write-Host "Run: msbuild ProjectSNUS.sln /p:Configuration=Debug" -ForegroundColor Yellow
    pause
    exit
}

Write-Host "Starting WCF Server..." -ForegroundColor Yellow
$serverProcess = Start-Process "ProjectSNUS\bin\Debug\ProjectSNUS.exe" -PassThru
Start-Sleep -Seconds 3
Write-Host "? Server started (PID: $($serverProcess.Id))" -ForegroundColor Green
Write-Host ""

Write-Host "Starting Sensors..." -ForegroundColor Yellow
Write-Host ""

Write-Host "[1/4] Starting Sensor 0..." -ForegroundColor Cyan
$sensor0 = Start-Process "Client1\bin\Debug\Client1.exe" -ArgumentList "0" -PassThru
Start-Sleep -Seconds 1

Write-Host "[2/4] Starting Sensor 1..." -ForegroundColor Cyan
$sensor1 = Start-Process "Client1\bin\Debug\Client1.exe" -ArgumentList "1" -PassThru
Start-Sleep -Seconds 1

Write-Host "[3/4] Starting Sensor 2..." -ForegroundColor Cyan
$sensor2 = Start-Process "Client1\bin\Debug\Client1.exe" -ArgumentList "2" -PassThru
Start-Sleep -Seconds 1

Write-Host "[4/4] Starting Sensor 3..." -ForegroundColor Cyan
$sensor3 = Start-Process "Client1\bin\Debug\Client1.exe" -ArgumentList "3" -PassThru
Start-Sleep -Seconds 1

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "   All components started successfully!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Process IDs:" -ForegroundColor White
Write-Host "  Server  : $($serverProcess.Id)" -ForegroundColor Gray
Write-Host "  Sensor 0: $($sensor0.Id)" -ForegroundColor Gray
Write-Host "  Sensor 1: $($sensor1.Id)" -ForegroundColor Gray
Write-Host "  Sensor 2: $($sensor2.Id)" -ForegroundColor Gray
Write-Host "  Sensor 3: $($sensor3.Id)" -ForegroundColor Gray
Write-Host ""
Write-Host "Server URL:" -ForegroundColor White
Write-Host "  http://localhost:8733/Design_Time_Addresses/ProjectSNUS/Service1/" -ForegroundColor Gray
Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
