# прогон всех текстов из tests/samples (сервис должен быть запущен)
$base = "http://localhost:5080"
Get-ChildItem "$PSScriptRoot/samples/*.txt" | Sort-Object Name | ForEach-Object {
    Write-Host "=== $($_.Name) ===" -ForegroundColor Cyan
    $body = @{ text = (Get-Content $_.FullName -Raw -Encoding UTF8) } | ConvertTo-Json
    try {
        $r = Invoke-WebRequest -Uri "$base/api/extract" -Method Post -ContentType "application/json; charset=utf-8" `
             -Body ([System.Text.Encoding]::UTF8.GetBytes($body))
        Write-Host "HTTP $($r.StatusCode)"
    } catch {
        Write-Host "HTTP ошибка: $($_.Exception.Message)" -ForegroundColor Red
    }
    Write-Host "Глоссарий после этого текста:"
    (Invoke-RestMethod "$base/api/glossary") | Format-Table -AutoSize
}
