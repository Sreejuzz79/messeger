# Fix async issues in UserDashboardController
$file = "c:\Users\sreej\OneDrive\Desktop\MessangerWeb-main\MessangerWeb\Controllers\UserDashboardController.cs"

if (Test-Path $file) {
    $content = Get-Content $file -Raw
    
    # Make all IActionResult methods that use await into async Task<IActionResult>
    $content = $content -replace '(\s+)public IActionResult Index\(', '$1public async Task<IActionResult> Index('
    $content = $content -replace '(\s+)public IActionResult SendMessage\(', '$1public async Task<IActionResult> SendMessage('
    $content = $content -replace '(\s+)public IActionResult SendFile\(', '$1public async Task<IActionResult> SendFile('
    $content = $content -replace '(\s+)public IActionResult GetMessages\(', '$1public async Task<IActionResult> GetMessages('
    $content = $content -replace '(\s+)public IActionResult UpdateProfile\(', '$1public async Task<IActionResult> UpdateProfile('
    $content = $content -replace '(\s+)public IActionResult CreateGroup\(', '$1public async Task<IActionResult> CreateGroup('
    $content = $content -replace '(\s+)public IActionResult GetGroupMessages\(', '$1public async Task<IActionResult> GetGroupMessages('
    $content = $content -replace '(\s+)public IActionResult SendGroupMessage\(', '$1public async Task<IActionResult> SendGroupMessage('
    $content = $content -replace '(\s+)public IActionResult SendGroupFile\(', '$1public async Task<IActionResult> SendGroupFile('
    $content = $content -replace '(\s+)public IActionResult GetUnreadMessagesCount\(', '$1public async Task<IActionResult> GetUnreadMessagesCount('
    $content = $content -replace '(\s+)public IActionResult Mark MessagesAsRead\(', '$1public async Task<IActionResult> MarkMessagesAsRead('
    $content = $content -replace '(\s+)public IActionResult MarkGroupMessagesAsRead\(', '$1public async Task<IActionResult> MarkGroupMessagesAsRead('
    $content = $content -replace '(\s+)public IActionResult GetGroupMembers\(', '$1public async Task<IActionResult> GetGroupMembers('
    $content = $content -replace '(\s+)public IActionResult RemoveMemberFromGroup\(', '$1public async Task<IActionResult> RemoveMemberFromGroup('
    $content = $content -replace '(\s+)public IActionResult GetAvailableMembers\(', '$1public async Task<IActionResult> GetAvailableMembers('
    $content = $content -replace '(\s+)public IActionResult AddMembersToGroup\(', '$1public async Task<IActionResult> AddMembersToGroup('
    $content = $content -replace '(\s+)public IActionResult UpdateGroup\(', '$1public async Task<IActionResult> UpdateGroup('
    
    # Fix private helper methods
    $content = $content -replace '(\s+)private List<ChatMessage> GetMessages\(', '$1private async Task<List<ChatMessage>> GetMessages('
    $content = $content -replace '(\s+)private UserInfo GetUserById\(', '$1private async Task<UserInfo> GetUserById('
    $content = $content -replace '(\s+)private List<UserInfo> GetAllUsersWithLastMessage\(', '$1private async Task<List<UserInfo>> GetAllUsersWithLastMessage('
    $content = $content -replace '(\s+)private bool IsConversationExists\(', '$1private async Task<bool> IsConversationExists('
    $content = $content -replace '(\s+)private Dictionary<string, int> GetUnreadCounts\(', '$1private async Task<Dictionary<string, int>> GetUnreadCounts('
    $content = $content -replace '(\s+)private List<GroupInfo> GetUserGroups\(', '$1private async Task<List<GroupInfo>> GetUserGroups('
    $content = $content -replace '(\s+)private List<GroupMessage> GetGroupMessages\(', '$1private async Task<List<GroupMessage>> GetGroupMessages('
    $content = $content -replace '(\s+)private bool IsInGroup\(', '$1private async Task<bool> IsInGroup('
    $content = $content -replace '(\s+)private bool IsGroupMember\(', '$1private async Task<bool> IsGroupMember('
    $content = $content -replace '(\s+)private List<UserInfo> GetAvailableMembersForGroup\(', '$1private async Task<List<UserInfo>> GetAvailableMembersForGroup('
    
    Set-Content -Path $file -Value $content -NoNewline
    Write-Host "Fixed async issues in UserDashboardController" -ForegroundColor Green
}
