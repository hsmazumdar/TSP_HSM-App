@echo off
setlocal
cd /d "%~dp0"
echo ============================================================
echo  Table 2 SMOKE (N=100..2000, 2 runs) — usually a few minutes
echo ============================================================
where dotnet >nul 2>&1 || (echo ERROR: .NET SDK not found & exit /b 1)
where python >nul 2>&1 || (echo ERROR: Python not found & exit /b 1)

powershell -NoProfile -ExecutionPolicy Bypass -File "experiments\scripts\run_autobench.ps1" ^
  -ManifestPath "%cd%\experiments\configs\table2_grid_smoke.json" ^
  -OutDir "%cd%\experiments\results"

if errorlevel 1 (
  echo AutoBench smoke failed.
  exit /b 1
)

echo.
echo Smoke finished. To rebuild Table-2.docx / Figure-1 from the latest summary:
echo   MAKE_TABLE2_AND_FIGURE1.bat
echo.
pause
endlocal
