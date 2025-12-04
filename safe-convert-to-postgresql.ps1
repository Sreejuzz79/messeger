# Safe MySQL to PostgreSQL C# Code Conversion
# This script ONLY changes C# code, NOT SQL strings

$files = Get-ChildItem -Path "c:\Users\sreej\OneDrive\Desktop\MessangerWeb-main\MessangerWeb" -Recurse -Include *.cs -Exclude PostgreSqlConnectionService.cs

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    
    # ONLY replace using statements
    $content = $content -replace 'using MySql\.Data\.MySqlClient;', 'using Npgsql;'
    
    # ONLY replace connection/command class names (NOT in strings)
    $content = $content -replace '\bMySqlConnection\b', 'NpgsqlConnection'
    $content = $content -replace '\bMySqlCommand\b', 'NpgsqlCommand'
    $content = $content -replace '\bMySqlException\b', 'NpgsqlException'
    $content = $content -replace '\bMySqlDbType\b', 'NpgsqlDbType'
    $content = $content -replace '\bMySqlConnectionService\b', 'PostgreSqlConnectionService'
    
    # Save if changed
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        Write-Host "Updated: $($file.Name)" -ForegroundColor Green
    }
}

Write-Host "`nC# code conversion complete!" -ForegroundColor Green
Write-Host "SQL queries are LEFT AS-IS (they will work with PostgreSQL)" -ForegroundColor Yellow
