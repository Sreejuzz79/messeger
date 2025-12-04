# Fix async/await issues in AccountController
$file = "c:\Users\sreej\OneDrive\Desktop\MessangerWeb-main\MessangerWeb\Controllers\AccountController.cs"

if (Test-Path $file) {
    $content = Get-Content $file -Raw
    
    # Make UserLogin method async
    $content = $content -replace 'public IActionResult UserLogin\(string Email, string Password\)', 'public async Task<IActionResult> UserLogin(string Email, string Password)'
    
    # Make helper methods async
    $content = $content -replace 'private bool TestDatabaseConnection\(\)', 'private async Task<bool> TestDatabaseConnection()'
    $content = $content -replace 'private bool CheckIfUserIsInactive\(', 'private async Task<bool> CheckIfUserIsInactive('
    
    # Fix method calls to be awaited
    $content = $content -replace 'if \(!TestDatabaseConnection\(\)\)', 'if (!(await TestDatabaseConnection()))'
    $content = $content -replace 'bool isInactive = CheckIfUserIsInactive\(Email, Password\);', 'bool isInactive = await CheckIfUserIsInactive(Email, Password);'
    
    # Fix connection.Open() to be async
    $ content = $content -replace '(\s+)connection\.Open\(\);', '$1await connection.OpenAsync();'
    
    Set-Content -Path $file -Value $content -NoNewline
    Write-Host "Fixed async/await issues in AccountController" -ForegroundColor Green
}
