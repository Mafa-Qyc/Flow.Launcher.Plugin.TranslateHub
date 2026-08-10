dotnet publish Flow.Launcher.Plugin.translate-demo -c Debug -r win-x64 --no-self-contained

$ErrorActionPreference = "Stop"

# --- Locate Flow Launcher executable (scoop install) ---
$scoopExe = Get-ChildItem "D:\scoop\apps\flow-launcher\*\Flow.Launcher.exe" -ErrorAction SilentlyContinue | Select-Object -First 1
$standardExe = "$env:LOCALAPPDATA\FlowLauncher\Flow.Launcher.exe"

$flowLauncherExe = $null
if ($scoopExe) {
    $flowLauncherExe = $scoopExe.FullName
} elseif (Test-Path $standardExe) {
    $flowLauncherExe = $standardExe
}

# --- Locate Plugins folder (scoop persists to UserData) ---
$scoopPlugins = "D:\scoop\persist\flow-launcher\UserData\Plugins"
$standardPlugins = "$env:APPDATA\FlowLauncher\Plugins"

$pluginsFolder = $null
if (Test-Path $scoopPlugins) {
    $pluginsFolder = $scoopPlugins
} elseif (Test-Path $standardPlugins) {
    $pluginsFolder = $standardPlugins
}

$pluginName = "TranslateHub"
$publishDir = "Flow.Launcher.Plugin.translate-demo\bin\Debug\win-x64\publish"

if ($null -eq $flowLauncherExe -or $null -eq $pluginsFolder) {
    Write-Host "Flow Launcher not found. Check install location."
    exit 1
}

Write-Host "Flow Launcher: $flowLauncherExe"
Write-Host "Plugins folder: $pluginsFolder"

# --- Stop Flow ---
Stop-Process -Name "Flow.Launcher" -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

# --- Replace plugin folder ---
$target = Join-Path $pluginsFolder $pluginName
if (Test-Path $target) {
    Remove-Item -Recurse -Force $target
}

Copy-Item $publishDir $pluginsFolder -Recurse -Force
Rename-Item -Path (Join-Path $pluginsFolder "publish") -NewName $pluginName

# --- Restart Flow ---
Start-Sleep -Seconds 2
Start-Process $flowLauncherExe
Write-Host "Done. Plugin deployed to $target"
