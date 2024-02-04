@REM test if KSP2_x64.exe is running
echo off

tasklist /fi "imagename eq KSP2_x64.exe" |find ":" > nul
if errorlevel 1 (
    echo "Kill KSP2 !!!!"
    taskkill /f /im "KSP2_x64.exe"
    timeout 2
) else echo "KSP2 Not running"
