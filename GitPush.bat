@echo off
echo ===================================================
echo   LiteNex Client GitHub Push Automation
echo   Target: linezoom7-cloud/LiteNexLauncher
echo ===================================================
echo.

echo [1/3] Building latest binaries (LiteNex.exe & LiteNexSetup.exe)...
call BuildSetup.bat

if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Build failed! Aborting.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo [2/3] Locating Git executable...
set GIT_EXE=git
where git >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    if exist "C:\Program Files\Git\cmd\git.exe" (
        set GIT_EXE="C:\Program Files\Git\cmd\git.exe"
    ) else if exist "%LocalAppData%\Programs\Git\cmd\git.exe" (
        set GIT_EXE="%LocalAppData%\Programs\Git\cmd\git.exe"
    ) else (
        echo [HATA] Git bulunamadi.
        pause
        exit /b 1
    )
)

echo [INFO] Git executable: %GIT_EXE%
echo.

echo [3/3] Committing and Pushing to GitHub...
%GIT_EXE% config user.email "linezoom7-cloud@users.noreply.github.com" >nul 2>nul
%GIT_EXE% config user.name "linezoom7-cloud" >nul 2>nul
%GIT_EXE% init >nul 2>nul
%GIT_EXE% remote remove origin >nul 2>nul
%GIT_EXE% remote add origin https://github.com/linezoom7-cloud/LiteNexLauncher.git >nul 2>nul

%GIT_EXE% add .

set /p commitMsg="Lutfen guncelleme notunu (commit) girin [Varsayilan: LiteNex Update]: "
if "%commitMsg%"=="" set commitMsg=LiteNex Update

%GIT_EXE% commit -m "%commitMsg%"
%GIT_EXE% branch -M main >nul 2>nul
%GIT_EXE% push -u origin main

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ===================================================
    echo  [SUCCESS] LiteNex Launcher GitHub'a basariyla yuklendi!
    echo  Repo: https://github.com/linezoom7-cloud/LiteNexLauncher
    echo ===================================================
) else (
    echo [NOTE] Git push sirasinda GitHub giris ekrani acilmis olabilir.
)
pause
