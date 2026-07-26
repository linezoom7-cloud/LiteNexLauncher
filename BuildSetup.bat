@echo off
echo ===================================================
echo   LiteNex Client Single-EXE Setup Installer Builder
echo ===================================================
echo.

echo [1/2] Compiling LiteNex Launcher core (LiteNex.exe)...
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /out:LiteNex.exe /win32manifest:app.manifest /r:System.Web.Extensions.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll /win32icon:logo.ico Launcher.cs

if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Launcher compilation failed!
    exit /b %ERRORLEVEL%
)

echo [2/2] Bundling Single-EXE Setup Installer (LiteNexSetup.exe)...
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /out:LiteNexSetup.exe /win32manifest:app.manifest /res:LiteNex.exe,LiteNex.exe /res:logo.ico,logo.ico /res:logo.png,logo.png /win32icon:logo.ico /r:System.Windows.Forms.dll /r:System.Drawing.dll SetupInstaller.cs

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ===================================================
    echo  [SUCCESS] LiteNexSetup.exe is READY!
    echo  You can now send 'LiteNexSetup.exe' to your friend!
    echo ===================================================
) else (
    echo [ERROR] Setup Installer compilation failed!
)
