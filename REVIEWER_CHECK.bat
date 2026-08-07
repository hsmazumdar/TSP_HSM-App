@echo off
setlocal EnableExtensions
cd /d "%~dp0"

:menu
cls
echo ============================================
echo  DVD-TSP Reviewer Reproduction
echo ============================================
echo  Level A = inspect supplied evidence
echo  Level B = regenerate tables from result files
echo  Level C = rerun solver (may be RESOURCE INTENSIVE)
echo ============================================
echo  1 - Quick smoke test (Table 2 Autobench smoke)
echo  2 - Rebuild Table 1 from supplied results
echo  3 - Run Table 1 K-sensitivity [RESOURCE INTENSIVE]
echo  4 - Rebuild Table 3 from supplied results
echo  5 - Run selected TSPLIB instance [needs .xy]
echo  6 - Run complete Table 3 benchmark [RESOURCE INTENSIVE]
echo  7 - Inspect 25M supplied evidence
echo  8 - Run 25M experiment [WARNING: LARGE]
echo  9 - Verify result-file integrity
echo 10 - Exit
echo ============================================
set /p choice=Select option: 

if "%choice%"=="1" goto smoke
if "%choice%"=="2" goto t1build
if "%choice%"=="3" goto t1run
if "%choice%"=="4" goto t3build
if "%choice%"=="5" goto t3one
if "%choice%"=="6" goto t3all
if "%choice%"=="7" goto e25inspect
if "%choice%"=="8" goto e25run
if "%choice%"=="9" goto verify
if "%choice%"=="10" exit /b 0
goto menu

:smoke
call RUN_TABLE2_SMOKE.bat
pause
goto menu

:t1build
python experiments\table1_k_sensitivity\scripts\build_table1.py
pause
goto menu

:t1run
echo RESOURCE INTENSIVE: N=1,000,000 x K={8,16,32,64}
set /p conf=Continue? Y/N: 
if /I not "%conf%"=="Y" goto menu
call RUN_TABLE1.bat
pause
goto menu

:t3build
python experiments\table3_tsplib\scripts\build_table3.py
pause
goto menu

:t3one
set /p inst=Instance name (e.g. berlin52): 
powershell -NoProfile -ExecutionPolicy Bypass -File experiments\table3_tsplib\scripts\run_one_tsplib.ps1 -Instance %inst%
pause
goto menu

:t3all
powershell -NoProfile -ExecutionPolicy Bypass -File experiments\table3_tsplib\scripts\run_table3.ps1
pause
goto menu

:e25inspect
python experiments\extreme_25m\scripts\verify_25m_result.py
type experiments\extreme_25m\results\paper_reported\extreme_25m_summary.json
echo.
pause
goto menu

:e25run
echo WARNING
echo This experiment creates N = 25,000,000 cities and may require substantial
echo memory and computation time. It is not required for ordinary repository
echo verification.
set /p conf=Continue? Y/N: 
if /I not "%conf%"=="Y" goto menu
powershell -NoProfile -ExecutionPolicy Bypass -File experiments\extreme_25m\scripts\run_extreme_25m.ps1 -Force
pause
goto menu

:verify
call VERIFY_REPOSITORY.bat
pause
goto menu
