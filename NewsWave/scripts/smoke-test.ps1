param(
    [int]$Port = 5191
)

$projectRoot = Split-Path $PSScriptRoot -Parent
$baseAddress = "http://127.0.0.1:$Port"
$previousAdminPassword = $env:NewsWave__AdminPassword
Remove-Item Env:NewsWave__AdminPassword -ErrorAction SilentlyContinue

$newsWaveProcess = Start-Process dotnet -ArgumentList "run --project NewsWave.csproj --no-build --urls $baseAddress" -WorkingDirectory $projectRoot -WindowStyle Hidden -PassThru

try {
    $ready = $false
    for ($attempt = 0; $attempt -lt 20; $attempt++) {
        try {
            $healthResponse = Invoke-WebRequest -UseBasicParsing "$baseAddress/health"
            if ($healthResponse.StatusCode -eq 200) {
                $ready = $true
                break
            }
        }
        catch {
            Start-Sleep -Milliseconds 250
        }
    }

    if (-not $ready) {
        throw "NewsWave не запустился или endpoint /health недоступен."
    }

    $checks = [ordered]@{
        "/" = "Центр управления рассылкой"
        "/Mail" = "campaignPreview"
        "/Contacts" = "Подписчики"
        "/Templates" = "Библиотека писем"
    }

    foreach ($route in $checks.Keys) {
        $response = Invoke-WebRequest -UseBasicParsing "$baseAddress$route"
        if ($response.StatusCode -ne 200 -or -not $response.Content.Contains($checks[$route])) {
            throw "Проверка $route завершилась ошибкой."
        }
        Write-Host "OK $route"
    }
}
finally {
    Stop-Process -Id $newsWaveProcess.Id -Force -ErrorAction SilentlyContinue
    if ($null -ne $previousAdminPassword) {
        $env:NewsWave__AdminPassword = $previousAdminPassword
    }
}
