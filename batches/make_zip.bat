@REM Create the zip for SpaceDock
echo off
set ConfigurationName=%1
set PROJECT_NAME=%2

@REM call the local_dev_options
call local_dev_options.bat

@REM define the default build mode to Debug

set swinfo_json=..\%PROJECT_NAME%\swinfo.json

@echo off
@REM Title Get Version from swinfo_json.json using PowerShell with a batch file
Set PSCMD=Powershell -C "$(GC %swinfo_json% | ConvertFrom-Json).version"
@for /f %%a in ('%PSCMD%') do set "Ver=%%a"
@REM echo Version=%Ver%

set ZIP_File=%PROJECT_NAME%_v%Ver%.zip
SET OUTPUT=..\%ConfigurationName%

set CWD=%cd%
cd %OUTPUT%

echo ###################### Build %ZIP_File% ####################
del %ZIP_File%
"C:\Program Files\7-Zip\7z.exe" a %ZIP_File% BepInEx

if not exist ..\releases mkdir ..\releases

move %ZIP_File% ..\releases

cd %CWD%

:end