@echo off

set CSC="%CD:~0,3%Tools\Mono\bin\csc.bat"

if "%~1"=="" (
	echo No input file was given
	echo Try typing "%0 <name of file>"
	goto exit
)

call %CSC% -unsafe "%~1" ..\Source\Utils.cs ..\Source\Suffix.cs ..\Source\GFX\ColorTable.cs ..\Source\GFX\ColorPattern.cs

:exit
	echo.
	pause
	exit /b 0