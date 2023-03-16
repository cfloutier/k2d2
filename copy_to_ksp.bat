echo off
set PROJECT_NAME=K2D2
set CONFIG=Debug

set OUTPUT=output
set LOCAL_DIR=%OUTPUT%\BepInEx\plugins\%PROJECT_NAME%

@REM create local dir
if not exist %OUTPUT% (
    echo %OUTPUT% is missing
    exit
)

echo ####################### Copy to target Ksp dir #######################
echo on

set DEST_PATH="D:\SteamLibrary\steamapps\common\Kerbal Space Program 2\BepInEx\plugins\%PROJECT_NAME%"
echo dest path is : %DEST_PATH%

rd /s/q %DEST_PATH%
mkdir  %DEST_PATH%
if not exist %DEST_PATH% mkdir %DEST_PATH%
xcopy /s /d %LOCAL_DIR% %DEST_PATH%

