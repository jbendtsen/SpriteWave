@echo off

set MONO="%CD:~0,3%Tools\Mono\bin\mono.exe"

if "%~1"=="" (
	echo No input file was given
	echo Try typing "%0 <name of file>"
	goto exit
)

call %MONO% "%~1"
pause