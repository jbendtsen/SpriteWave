@echo off

set CSC="%CD:~0,3%Tools\Mono\bin\csc.bat"

if "%~1"=="" (
	echo No input file was given
	echo Try typing "%0 <name of file>"
	goto exit
)

call %CSC% -unsafe /t:winexe "%~1" ..\Source\Utils.cs ..\Source\GFX\ColourTable.cs ..\Source\Suffix.cs

:exit
	echo.
	pause
	exit /b 0