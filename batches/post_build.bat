echo off

@REM call the local_dev_options
call local_dev_options.bat

set ConfigurationName=%1
set PROJECT_NAME=%2

if "%Close_KSP2%"=="True" (
   @REM test if KSP2_x64.exe is running
    tasklist /fi "imagename eq KSP2_x64.exe" |find ":" > nul
    if errorlevel 1 (
        echo "Kill KSP2 !!!!"
        taskkill /f /im "KSP2_x64.exe"
        timeout 2
    ) else echo "KSP2 Not running"
)

if "%Create_zip%"=="True" (
    call make_zip.bat %ConfigurationName% %Project_name%
)

call copy_to_ksp.bat %ConfigurationName% %Project_name%

if "%Open_KSP2%"=="True" (
    "%KSP2_LOCATION%\KSP2_x64.exe"
)

@REM copy to dependencies dll
set SOURCE_DIR=..\%ConfigurationName%\BepInEx\plugins\%PROJECT_NAME%\
SET Copies=%PostBuildCopy:;= %
FOR %%e in (%Copies%) DO (
    echo copy to ..\%%e
    xcopy /Y /s %SOURCE_DIR%\%PROJECT_NAME%.dll ..\%%e
)
