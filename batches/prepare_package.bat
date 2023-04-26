@REM Create the zip for SpaceDock
@REM echo off
echo off
set build_mode=%1

@REM call the local_dev_options
call local_dev_options.bat

@REM define the default build mode to Debug 
IF [%build_mode%] == [] set build_mode=Debug

set PROJECT_NAME=K2D2

set OUTPUT=..\output
set LOCAL_DIR=%OUTPUT%\BepInEx\plugins\%PROJECT_NAME%

set ZIP_File=%PROJECT_NAME%.zip

echo ####################### Prepare Package #######################

@REM create local dir
if not exist %OUTPUT% mkdir %OUTPUT%
if not exist %OUTPUT%\BepInEx mkdir %OUTPUT%\BepInEx
if not exist %OUTPUT%\BepInEx\plugins mkdir %OUTPUT%\BepInEx\plugins

rd /s/q %LOCAL_DIR%

if not exist %LOCAL_DIR% mkdir %LOCAL_DIR%

copy /Y ..\LICENSE.md %LOCAL_DIR%\
copy /Y ..\K2D2\swinfo.json %LOCAL_DIR%\
copy /Y ..\README.md %LOCAL_DIR%\

md %LOCAL_DIR%\assets
md %LOCAL_DIR%\assets\images
copy /Y ..\icon.png %LOCAL_DIR%\assets\images
copy /Y ..\K2D2\assets\images\*.png %LOCAL_DIR%\assets\images

@REM Copy Dll
copy /Y ..\K2D2Project\obj\%build_mode%\%PROJECT_NAME%.dll %LOCAL_DIR%
@REM Copy Pdb
copy /Y ..\K2D2Project\obj\%build_mode%\%PROJECT_NAME%.pdb %LOCAL_DIR%

set CWD=%cd%

cd %OUTPUT%

if "%Create_zip%"=="True" (
    echo ###################### Build %ZIP_File% ####################
    del %ZIP_File%
    "C:\Program Files\7-Zip\7z.exe" a %ZIP_File% BepInEx
)

cd %CWD%

:end