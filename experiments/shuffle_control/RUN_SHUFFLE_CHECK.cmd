@echo off
rem Confirms the shuffle option changes city numbering: one 10k shuffled run, tour kept and copied here.
cd /d "%~dp0"
set EXE=TspBatch6\bin\Release\net8.0-windows\TspBatch6.exe
%EXE% --n 10000 --runs 1 --maxTrials 1 --runMinutes 2 --shape square --shuffle 1 --seed 777 --out "%~dp0results_shuffle_check" --deleteTours 0 > SHUFFLE_CHECK_log.txt 2>&1
powershell -NoProfile -Command "Get-ChildItem 'E:\TSP_headless_cache' -Recurse -Filter 'rout_seq_01.tsp' | Where-Object { $_.Length -lt 2000000 } | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Copy-Item -Destination '%~dp0shuffle_check_tour.tsp' -Force"
echo CHECK DONE >> SHUFFLE_CHECK_log.txt
