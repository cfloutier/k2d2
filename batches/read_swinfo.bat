set swinfo_json=..\K2D2\swinfo.json


@echo off
Title Get Version from Services.json using PowerShell with a batch file
Set PSCMD=Powershell -C "$(GC %swinfo_json% | ConvertFrom-Json).version"
@for /f %%a in ('%PSCMD%') do set "Ver=%%a"
echo Version=%Ver%



