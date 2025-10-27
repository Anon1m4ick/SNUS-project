@echo off
echo ================================================
echo   SNUS Project - Causal Broadcast System
echo ================================================
echo.
echo Starting WCF Server...
cd ProjectSNUS\bin\Debug
start "WCF Server" ProjectSNUS.exe
cd ..\..\..
timeout /t 3 /nobreak >nul
echo Server started!
echo.
echo Starting Sensors...
echo.
echo [1/4] Starting Sensor 0...
cd Client1\bin\Debug
start "Sensor 0" Client1.exe 0
cd ..\..\..
timeout /t 1 /nobreak >nul

echo [2/4] Starting Sensor 1...
cd Client1\bin\Debug
start "Sensor 1" Client1.exe 1
cd ..\..\..
timeout /t 1 /nobreak >nul

echo [3/4] Starting Sensor 2...
cd Client1\bin\Debug
start "Sensor 2" Client1.exe 2
cd ..\..\..
timeout /t 1 /nobreak >nul

echo [4/4] Starting Sensor 3...
cd Client1\bin\Debug
start "Sensor 3" Client1.exe 3
cd ..\..\..
echo.
echo ================================================
echo   All components started successfully!
echo ================================================
echo.
echo Server: http://localhost:8733/Design_Time_Addresses/ProjectSNUS/Service1/
echo.
pause
