echo off
set PROJECT_NAME=K2D2
set CONFIG=Debug

set OUTPUT=..\output
set LOCAL_DIR=%OUTPUT%\BepInEx\plugins\%PROJECT_NAME%\

@REM call the local_dev_options
call local_dev_options.bat

echo ####################### Copy to target Ksp dir #######################
set DEST_PATH="%KSP2_LOCATION%\BepInEx\plugins\%PROJECT_NAME%\"
echo dest path is : %DEST_PATH%

@REM rd /s/q %DEST_PATH%
if not exist %DEST_PATH% mkdir %DEST_PATH%

@REM dir %LOCAL_DIR%
@REM dir %DEST_PATH%

xcopy /Y /s  /d %LOCAL_DIR% %DEST_PATH%

