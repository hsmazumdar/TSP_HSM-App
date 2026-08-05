@echo off
setlocal
cd /d "%~dp0"
echo ============================================================
echo  Table 2 FULL (N=100..1,000,000) — many hours on a desktop PC
echo  Prefer reviewing published results under experiments\results\table2\
echo ============================================================
where dotnet >nul 2>&1 || (echo ERROR: .NET SDK not found & exit /b 1)
where python >nul 2>&1 || (echo ERROR: Python not found & exit /b 1)

powershell -NoProfile -ExecutionPolicy Bypass -File "experiments\scripts\run_autobench.ps1" ^
  -ManifestPath "%cd%\experiments\configs\table2_grid_scalability.json" ^
  -OutDir "%cd%\experiments\results"

if errorlevel 1 (
  echo AutoBench full sweep failed.
  exit /b 1
)

set PYTHONIOENCODING=utf-8
for /f "delims=" %%D in ('dir /b /ad /o-d experiments\results\table2_grid_scalability_* 2^>nul') do (
  set LATEST=experiments\results\%%D
  goto :found
)
:found
if not defined LATEST (
  echo Could not find a table2_grid_scalability_* results folder.
  exit /b 1
)
echo Using %LATEST%
python experiments\scripts\make_table2.py "%LATEST%" --out experiments\results\table2\table2_from_full.md
python experiments\scripts\generate_table2_figure1.py "%LATEST%\summary.csv" --out-dir experiments\results\table2
echo.
echo Done. See experiments\results\table2\
pause
endlocal
