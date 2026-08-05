@echo off
setlocal
cd /d "%~dp0"
echo ============================================================
echo  Rebuild Table-2.docx + Figure-1.png from published summary
echo ============================================================
where python >nul 2>&1 || (echo ERROR: Python not found & exit /b 1)

set PYTHONIOENCODING=utf-8
pip show python-docx >nul 2>&1 || pip install python-docx
pip show matplotlib >nul 2>&1 || pip install matplotlib numpy

python experiments\scripts\make_table2.py experiments\results\table2\table2_merged_summary.csv --out experiments\results\table2\table2_merged_draft.md
if errorlevel 1 exit /b 1

python experiments\scripts\generate_table2_figure1.py experiments\results\table2\table2_merged_summary.csv --out-dir experiments\results\table2
if errorlevel 1 exit /b 1

echo.
echo Wrote:
echo   experiments\results\table2\Table-2.docx
echo   experiments\results\table2\Figure-1.png
echo   experiments\results\table2\table2_merged_draft.md
echo.
pause
endlocal
