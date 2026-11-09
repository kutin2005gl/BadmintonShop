# User explicitly authorized restarting only SQLEXPRESS on 06/11/2026.
$ErrorActionPreference = 'Stop'
$restartLog = Join-Path $PSScriptRoot 'sql-restart-admin-result.txt'
try {
    & {
        Get-Date -Format o
        Get-Service -Name 'MSSQL$SQLEXPRESS' | Format-List Name,Status
        Restart-Service -Name 'MSSQL$SQLEXPRESS' -ErrorAction Stop
        Get-CimInstance Win32_Service -Filter 'Name="MSSQL$SQLEXPRESS"' |
            Select-Object Name,State,ProcessId | Format-List
        'Authorized restart completed.'
    } *>&1 | Out-File -LiteralPath $restartLog -Encoding utf8
} catch {
    $_ | Out-String | Add-Content -LiteralPath $restartLog
    exit 1
}
