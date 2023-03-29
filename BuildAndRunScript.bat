::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: Bootloader batch file for new workstation setup
::
:: 1. Warns user if they run the script as admin
:: 2. Then runs some things as user
::  
:: To add things to be run as the user after that, go to the :finalSteps function
::
::      - David Sikes
::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

: For checking for internet

ping -n 2 -w 700 8.8.8.8 | find "TTL="

IF %ERRORLEVEL% EQU 0 (
    SET internet=internet_connected
) ELSE (
    SET internet=internet_not_connected
)


: ELEV argument present means we're calling this script again but with admin privs now
if "%1" neq "ELEV" (
	
    : For detecting if we're running as admin for warning to user
    cacls "%systemroot%\system32\config\system" 1>nul 2>&1

    call :warnUserIfRunningAsAdministrator

    call :warnUserIfRunningWithoutInternet

    call :finalSteps
)


::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: FINAL STEPS ONLY. THIS RUNS AFTER THE ADMIN PRIVS PART. THIS PART RUNS AS USER.
::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:finalSteps
    echo.
    echo Running!
    echo.
    
	del "%AppData%\NuGet\NuGet.Config"
	
    "C:\Program Files\dotnet\dotnet.exe" restore "%~dp0StringsComparisonBatchAnalyzer\StringsComparisonBatchAnalyzer.sln"
	
    pause

	"C:\Program Files\dotnet\dotnet.exe" build "%~dp0StringsComparisonBatchAnalyzer\StringsComparisonBatchAnalyzer.sln"

    pause

	echo .
	echo Running: "%~dp0StringsComparisonBatchAnalyzer\StringsComparisonBatchAnalyzer.Main\bin\x64\Debug\net7.0-windows\StringsComparisonBatchAnalyzer.Main.exe"
    echo .
	

    pause

	"%~dp0StringsComparisonBatchAnalyzer\StringsComparisonBatchAnalyzer.Main\bin\x64\Debug\net7.0-windows\StringsComparisonBatchAnalyzer.Main.exe"
	
    pause

	echo.
    echo Finished configuring this computer.
	echo.
	
    pause
    
    exit 0

::::::::::::::::::::::::::::::::::::::::::::::
:: SCRIPT UTILITY LOGIC ONLY BELOW HERE
::::::::::::::::::::::::::::::::::::::::::::::

:warnUserIfRunningAsAdministrator

    if "%errorlevel%" equ "0" (
    
        echo -------------------------------------------------------------
        echo ERROR: YOU ARE RUNNING THIS WITH ADMINISTRATOR PRIVILEGES
        echo -------------------------------------------------------------
        echo. 
        echo If you're seeing this, it means you are running this as admin user!
        echo.
        echo You will need to restart this program WITHOUT Administrator 
        echo privileges.
        echo. 
        echo Make sure to NOT Run As Administrator next time!
        echo. 
        echo Press any key to exit . . .

        pause> nul

        exit 1
    )

    exit /B
	
:warnUserIfRunningWithoutInternet

    if "%internet%" equ "internet_not_connected" (
    
        echo -------------------------------------------------------------
        echo ERROR: YOU ARE RUNNING THIS WITHOUT AN INTERNET CONNECTION
        echo -------------------------------------------------------------
        echo. 
        echo If you're seeing this, it means you are running this with no internet!
        echo.
        echo You will need to restart this program after connecting.
        echo. 
        echo Make sure to connect to the internet BEFORE re-running.
        echo. 
        echo Press any key to exit . . .

        pause> nul

        exit 1
    )

    exit /B

:: Set one environment variable from registry key
:SetFromReg
    "%WinDir%\System32\Reg" QUERY "%~1" /v "%~2" > "%TEMP%\_envset.tmp" 2>NUL
    for /f "usebackq skip=2 tokens=2,*" %%A IN ("%TEMP%\_envset.tmp") do (
        echo/set "%~3=%%B"
    )
    goto :EOF

:: Get a list of environment variables from registry
:GetRegEnv
    "%WinDir%\System32\Reg" QUERY "%~1" > "%TEMP%\_envget.tmp"
    for /f "usebackq skip=2" %%A IN ("%TEMP%\_envget.tmp") do (
        if /I not "%%~A"=="Path" (
            call :SetFromReg "%~1" "%%~A" "%%~A"
        )
    )
    goto :EOF

:RefreshEnvironmentVariables
    echo/@echo off >"%TEMP%\_env.cmd"

    :: Slowly generating final file
    call :GetRegEnv "HKLM\System\CurrentControlSet\Control\Session Manager\Environment" >> "%TEMP%\_env.cmd"
    call :GetRegEnv "HKCU\Environment">>"%TEMP%\_env.cmd" >> "%TEMP%\_env.cmd"

    :: Special handling for PATH - mix both User and System
    call :SetFromReg "HKLM\System\CurrentControlSet\Control\Session Manager\Environment" Path Path_HKLM >> "%TEMP%\_env.cmd"
    call :SetFromReg "HKCU\Environment" Path Path_HKCU >> "%TEMP%\_env.cmd"

    :: Caution: do not insert space-chars before >> redirection sign
    echo/set "Path=%%Path_HKLM%%;%%Path_HKCU%%" >> "%TEMP%\_env.cmd"

    :: Cleanup
    del /f /q "%TEMP%\_envset.tmp" 2>nul
    del /f /q "%TEMP%\_envget.tmp" 2>nul

    :: capture user / architecture
    SET "OriginalUserName=%USERNAME%"
    SET "OriginalArchitecture=%PROCESSOR_ARCHITECTURE%"

    :: Set these variables
    call "%TEMP%\_env.cmd"

    :: Cleanup
    del /f /q "%TEMP%\_env.cmd" 2>nul

    :: reset user / architecture
    SET "USERNAME=%OriginalUserName%"
    SET "PROCESSOR_ARCHITECTURE=%OriginalArchitecture%"

    echo | set /p dummy="Finished refreshing environtment variables."
    echo.
    