set ConfigurationName=%1

echo off

@REM call the ksp_location
call local_dev_options.bat

@REM define the default build mode to Debug 
IF [%ConfigurationName%] == [] set ConfigurationName=Debug

if "%Close_KSP2%"=="True" (
    call kill_ksp.bat
)

@REM call make_zip.bat %ConfigurationName%
@REM call copy_to_ksp.bat
