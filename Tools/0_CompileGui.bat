@echo off

set CSC="%CD:~0,3%Tools\Mono\bin\csc.bat"

if "%~1"=="" (
	echo No input file was given
	echo Try typing "%0 <name of file>"
	goto exit
)

call %CSC% -unsafe /t:winexe "%~1" ..\Source\Utils.cs ..\Source\GFX\ColorTable.cs ..\Source\Suffix.cs ..\Source\GFX\ColorSpace.cs

:exit
	echo.
	pause
	exit /b 0