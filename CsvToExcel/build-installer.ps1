# Скрипт сборки инсталлятора CsvToExcel
# Требования: .NET 8 SDK, Inno Setup 6 (ISCC.exe в PATH или укажите путь ниже)

param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$InnoSetupCompiler = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectDir = Resolve-Path "$ScriptDir"
$SolutionDir = Resolve-Path "$ScriptDir\.."
$PublishDir = "$SolutionDir\publish"
$ZipDir = "$SolutionDir\_zip"
$SetupScript = "$ScriptDir\setup.iss"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Сборка инсталлятора CsvToExcel" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Шаг 1: dotnet publish (single-file)
Write-Host "[1/3] Публикация single-file exe..." -ForegroundColor Yellow
$publishArgs = @(
    "publish", $ProjectDir,
    "-c", $Configuration,
    "-r", $Runtime,
    "--self-contained", "true",
    "-p:PublishSingleFile=true",
    "-p:IncludeNativeLibrariesForSelfExtract=true",
    "-o", $PublishDir
)
$publishResult = & dotnet $publishArgs 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ОШИБКА: dotnet publish завершился с кодом $LASTEXITCODE" -ForegroundColor Red
    Write-Host $publishResult
    exit 1
}
Write-Host "  Готово: $PublishDir\CsvToExcel.exe" -ForegroundColor Green
Write-Host ""

# Шаг 2: Inno Setup компиляция
Write-Host "[2/3] Компиляция setup.exe (Inno Setup)..." -ForegroundColor Yellow
if (-not (Test-Path $InnoSetupCompiler)) {
    Write-Host "ОШИБКА: ISCC.exe не найден по пути: $InnoSetupCompiler" -ForegroundColor Red
    Write-Host "  Укажите правильный путь через параметр -InnoSetupCompiler" -ForegroundColor Red
    exit 1
}
if (-not (Test-Path $SetupScript)) {
    Write-Host "ОШИБКА: setup.iss не найден: $SetupScript" -ForegroundColor Red
    exit 1
}

# Создаём _zip если нет
if (-not (Test-Path $ZipDir)) {
    New-Item -ItemType Directory -Force -Path $ZipDir | Out-Null
}

$isccResult = & $InnoSetupCompiler $SetupScript 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ОШИБКА: ISCC завершился с кодом $LASTEXITCODE" -ForegroundColor Red
    Write-Host $isccResult
    exit 1
}
Write-Host "  Готово: $ZipDir" -ForegroundColor Green
Write-Host ""

# Шаг 3: Итог
Write-Host "[3/3] Инсталлятор собран успешно!" -ForegroundColor Green
$setupFile = Get-ChildItem "$ZipDir\CsvToExcel_Setup_*.exe" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($setupFile) {
    Write-Host "  Файл: $($setupFile.FullName)" -ForegroundColor White
    Write-Host "  Размер: $([math]::Round($setupFile.Length / 1MB, 2)) MB" -ForegroundColor White
}
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Готово!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
