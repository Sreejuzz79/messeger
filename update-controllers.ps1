# Update all controllers to use PostgreSqlConnectionService
$files = @(
    "c:\Users\sreej\OneDrive\Desktop\MessangerWeb-main\MessangerWeb\Controllers\StudentController.cs",
    "c:\Users\sreej\OneDrive\Desktop\MessangerWeb-main\MessangerWeb\Controllers\AccountController.cs",
    "c:\Users\sreej\OneDrive\Desktop\MessangerWeb-main\MessangerWeb\Controllers\UserDashboardController.cs"
)

foreach ($file in $files) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw
        
        # Replace connection string field declarations
        $content = $content -replace 'private readonly string connectionString;', 'private readonly PostgreSqlConnectionService _dbService;'
        
        # Replace constructor parameter injection for StudentController
        $content = $content -replace 'public StudentController\(IConfiguration configuration\)', 'public StudentController(PostgreSqlConnectionService dbService)'
        $content = $content -replace 'connectionString = configuration.GetConnectionString\("DefaultConnection"\);(\s+)', '_dbService = dbService;$1'
        
        # Replace constructor parameter injection for AccountController  
        $content = $content -replace 'public AccountController\(IConfiguration configuration\)', 'public AccountController(PostgreSqlConnectionService dbService)'
        
        # Replace constructor parameter injection for UserDashboardController
        $content = $content -replace 'public UserDashboardController\(IConfiguration configuration, IVideoCallHistoryService videoCallHistoryService, IVideoCallParticipantService videoCallParticipantService\)',         'public UserDashboardController(PostgreSqlConnectionService dbService, IVideoCallHistoryService videoCallHistoryService, IVideoCallParticipantService videoCallParticipantService)'
        
        # Replace all instances of new NpgsqlConnection(connectionString)
        $content = $content -replace 'new NpgsqlConnection\(connectionString\)', 'await _dbService.GetConnectionAsync()'
        
        # Add using statement if not present
        if ($content -notmatch 'using MessangerWeb.Services;') {
            $content = $content -replace '(using [^;]+;[\r\n]+namespace)', "using MessangerWeb.Services;`r`n`$1"
        }
        
        # Remove IConfiguration using if it's no longer needed
        # (keeping it for now as other parts might use it)
        
        Set-Content -Path $file -Value $content -NoNewline
        Write-Host "Updated: $file" -ForegroundColor Green
    }
}

Write-Host "`nAll controllers updated to use PostgreSqlConnectionService!" -ForegroundColor Green
