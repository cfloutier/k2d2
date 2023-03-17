echo off
set PROJECT_NAME=K2D2
set CONFIG=Debug

set OUTPUT=output
set LOCAL_DIR=%OUTPUT%\BepInEx\plugins\%PROJECT_NAME%

@REM call the ksp_location 
call ksp_location.bat

@REM create local dir
if not exist %OUTPUT% (
    echo %OUTPUT% is missing
    exit
)

echo ####################### Copy to target Ksp dir #######################
echo on

set DEST_PATH="%KSP2_LOCATION%\BepInEx\plugins\%PROJECT_NAME%"
echo dest path is : %DEST_PATH%

rd /s/q %DEST_PATH%
mkdir  %DEST_PATH%
if not exist %DEST_PATH% mkdir %DEST_PATH%
xcopy /s /d %LOCAL_DIR% %DEST_PATH%

