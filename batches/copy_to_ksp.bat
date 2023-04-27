echo off
set ConfigurationName=%1
set PROJECT_NAME=%2

set SOURCE_DIR=..\%ConfigurationName%\BepInEx\plugins\%PROJECT_NAME%\

@REM call the local_dev_options
call local_dev_options.bat

echo ####################### Copy to target Ksp dir #######################
set DEST_PATH="%KSP2_LOCATION%\BepInEx\plugins\%PROJECT_NAME%\"
echo dest path is : %DEST_PATH%

@REM rd /s/q %DEST_PATH%
if not exist %DEST_PATH% mkdir %DEST_PATH%

@REM dir %SOURCE_DIR%
@REM dir %DEST_PATH%

xcopy /Y /s /d %SOURCE_DIR% %DEST_PATH%

