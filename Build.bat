@echo off
echo Building LiteNex Launcher...
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /out:LiteNex.exe /r:System.Web.Extensions.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll /win32icon:logo.ico Launcher.cs
if %ERRORLEVEL% EQU 0 (
    echo [SUCCESS] LiteNex.exe successfully compiled!
) else (
    echo [ERROR] Compilation failed.
)
