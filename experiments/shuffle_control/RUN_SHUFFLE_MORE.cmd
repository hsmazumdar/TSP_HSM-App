@echo off
rem Waits for RUN_SHUFFLE_CONTROL to finish, then: shuffled 100k and 500k (3 runs each), then shuffled 100M (1 run).
cd /d "%~dp0"
echo [%date% %time%] waiting for control run > SHUFFLE_MORE_log.txt
:wait
findstr /C:"SHUFFLE CONTROL DONE" SHUFFLE_log.txt >nul 2>&1
if errorlevel 1 (timeout /t 60 /nobreak >nul & goto wait)
set EXE=TspBatch6\bin\Release\net8.0-windows\TspBatch6.exe
set OUT=%~dp0results_shuffle
for %%N in (100000 500000) do (
  if exist STOP goto end
  echo [%date% %time%] N=%%N shuffled >> SHUFFLE_MORE_log.txt
  %EXE% --n %%N --runs 3 --maxTrials 5 --runMinutes 20 --shape square --shuffle 1 --seed 12345 --out "%OUT%" --deleteTours 1 >> SHUFFLE_MORE_log.txt 2>&1
)
if exist STOP goto end
echo [%date% %time%] N=100000000 shuffled >> SHUFFLE_MORE_log.txt
%EXE% --n 100000000 --runs 1 --maxTrials 3 --runMinutes 120 --shape square --shuffle 1 --seed 12345 --out "%OUT%" --deleteTours 1 >> SHUFFLE_MORE_log.txt 2>&1
:end
echo [%date% %time%] SHUFFLE MORE DONE >> SHUFFLE_MORE_log.txt
