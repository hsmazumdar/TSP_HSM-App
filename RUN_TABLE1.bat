@echo off
REM RESOURCE INTENSIVE: N=1,000,000 x K in {8,16,32,64}
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "experiments\table1_k_sensitivity\scripts\run_table1.ps1"
exit /b %ERRORLEVEL%
