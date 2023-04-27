@REM Create the zip for SpaceDock
@REM echo off
echo off
set ConfigurationName=%1

@REM call the local_dev_options
call local_dev_options.bat

@REM define the default build mode to Debug 
IF [%ConfigurationName%] == [] set ConfigurationName=Debug

set PROJECT_NAME=K2D2


set swinfo_json=..\K2D2\swinfo.json


@REM @echo off
@REM @REM Title Get Version from swinfo_json.json using PowerShell with a batch file
@REM Set PSCMD=Powershell -C "$(GC %swinfo_json% | ConvertFrom-Json).version"
@REM @for /f %%a in ('%PSCMD%') do set "Ver=%%a"
@REM echo Version=%Ver%

SET Ver=0.8.1

set ZIP_File=%PROJECT_NAME%_%Ver%.zip

echo ####################### make_zip #######################
SET OUTPUT=..\%ConfigurationName%

set CWD=%cd%

cd %OUTPUT%

dir 



if "%Create_zip%"=="True" (
    echo ###################### Build %ZIP_File% ####################
    del %ZIP_File%
    "C:\Program Files\7-Zip\7z.exe" a %ZIP_File% BepInEx
)

cd %CWD%

:end