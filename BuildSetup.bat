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

echo [1.5/2] Obfuscating LiteNex.exe resource to prevent false positives...
powershell -NoProfile -Command "$bytes = [System.IO.File]::ReadAllBytes('LiteNex.exe'); for($i=0; $i -lt $bytes.Length; $i++) { $bytes[$i] = $bytes[$i] -bxor 0x5A }; [System.IO.File]::WriteAllBytes('LiteNex.bin', $bytes)"

if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Obfuscation failed!
    exit /b %ERRORLEVEL%
)

echo [2/2] Bundling Single-EXE Setup Installer (LiteNexSetup.exe)...
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /out:LiteNexSetup.exe /win32manifest:app.manifest /res:LiteNex.bin,LiteNex.bin /res:logo.ico,logo.ico /res:logo.png,logo.png /win32icon:logo.ico /r:System.Windows.Forms.dll /r:System.Drawing.dll SetupInstaller.cs

set BUILD_STATUS=%ERRORLEVEL%

if exist LiteNex.bin del /f /q LiteNex.bin > nul 2>&1

if %BUILD_STATUS% EQU 0 (
    echo.
    echo ===================================================
    echo  [SUCCESS] LiteNexSetup.exe is READY!
    echo  You can now send 'LiteNexSetup.exe' to your friend!
    echo ===================================================
) else (
    echo [ERROR] Setup Installer compilation failed!
    exit /b %BUILD_STATUS%
)
