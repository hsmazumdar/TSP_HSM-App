@echo off
cd /d "%~dp0"
python scripts\verify_repository.py --write-sums
exit /b %ERRORLEVEL%
