# LiteNex Client Portable & Shortcut Creator
$desktop = [System.Environment]::GetFolderPath('Desktop')
$appDir  = (Get-Location).Path
$exePath = Join-Path $appDir "LiteNex.exe"
$icoPath = Join-Path $appDir "logo.ico"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  LiteNex Client v5.0 Setup Installer   " -ForegroundColor White
Write-Host "========================================" -ForegroundColor Cyan

if (Test-Path $exePath) {
    # Create Desktop Shortcut
    $WshShell = New-Object -ComObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut("$desktop\LiteNex Launcher.lnk")
    $Shortcut.TargetPath = $exePath
    $Shortcut.WorkingDirectory = $appDir
    if (Test-Path $icoPath) { $Shortcut.IconLocation = $icoPath }
    $Shortcut.Save()

    Write-Host "[OK] Masaüstü kısayolu başarıyla oluşturuldu: LiteNex Launcher.lnk" -ForegroundColor Green
    Write-Host "[OK] LiteNex Client Kurulumu Tamamlandı!" -ForegroundColor Green
} else {
    Write-Host "[HATA] LiteNex.exe bulunamadı!" -ForegroundColor Red
}
