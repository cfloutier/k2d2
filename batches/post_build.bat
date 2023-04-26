set build_mode=%1

echo off

@REM call the ksp_location
call local_dev_options.bat

@REM define the default build mode to Debug 
IF [%build_mode%] == [] set build_mode=Debug

if "%Close_KSP2%"=="True" (
    call kill_ksp.bat
)

call prepare_package.bat %build_mode%
call copy_to_ksp.bat
