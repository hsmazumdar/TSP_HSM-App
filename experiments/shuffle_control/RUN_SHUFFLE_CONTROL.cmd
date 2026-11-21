@echo off
rem Shuffled-city control for the square-grid results (G5 engine = G4 + optional city shuffle).
rem Same settings as the 20M-500M runs; only the city numbering differs.
rem Output: headless\results_shuffle\   log: SHUFFLE_log.txt
cd /d "%~dp0"
echo [%date% %time%] building TspBatch6 > SHUFFLE_log.txt
dotnet build TspBatch6\TspBatch6.csproj -c Release --nologo -v q >> SHUFFLE_log.txt 2>&1
set EXE=TspBatch6\bin\Release\net8.0-windows\TspBatch6.exe
set OUT=%~dp0results_shuffle
if not exist %EXE% (echo BUILD FAILED >> SHUFFLE_log.txt & goto end)
rem 10k and 1M: generator (row-major) order vs shuffled, 3 runs each
for %%N in (10000 1000000) do (
  if exist STOP goto end
  echo [%date% %time%] N=%%N row order >> SHUFFLE_log.txt
  %EXE% --n %%N --runs 3 --maxTrials 5 --runMinutes 20 --shape square --out "%OUT%" --deleteTours 1 >> SHUFFLE_log.txt 2>&1
  echo [%date% %time%] N=%%N shuffled >> SHUFFLE_log.txt
  %EXE% --n %%N --runs 3 --maxTrials 5 --runMinutes 20 --shape square --shuffle 1 --seed 12345 --out "%OUT%" --deleteTours 1 >> SHUFFLE_log.txt 2>&1
)
rem 25M shuffled, 3 runs (the row-order 25M runs are already in Exp20M\runs25M)
if exist STOP goto end
echo [%date% %time%] N=25000000 shuffled >> SHUFFLE_log.txt
%EXE% --n 25000000 --runs 3 --maxTrials 5 --runMinutes 40 --shape square --shuffle 1 --seed 12345 --out "%OUT%" --deleteTours 1 >> SHUFFLE_log.txt 2>&1
:end
echo [%date% %time%] SHUFFLE CONTROL DONE >> SHUFFLE_log.txt
type SHUFFLE_log.txt
