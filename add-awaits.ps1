# Fix all missing await keywords in UserDashboardController
$file = "c:\Users\sreej\OneDrive\Desktop\MessangerWeb-main\MessangerWeb\Controllers\UserDashboardController.cs"

if (Test-Path $file) {
    $content = Get-Content $file -Raw
    
    # Add await to all calls to async helper methods
    $content = $content -replace '(\s+)(var userProfile = )GetUserById\(', '$1$2await GetUserById('
    $content = $content -replace '(\s+)(model\.Users = )GetAllUsersWithLastMessage\(', '$1$2await GetAllUsersWithLastMessage('
    $content = $content -replace '(\s+)(model\.Groups = )GetUserGroups\(', '$1$2await GetUserGroups('
    $content = $content -replace '(\s+)(model\.SelectedUser = )GetUserById\(', '$1$2await GetUserById('
    $content = $content -replace '(\s+)(model\.Messages = )GetMessages\(', '$1$2await GetMessages('
    $content = $content -replace '(\s+)(var receiverInfo = )GetUserById\(', '$1$2await GetUserById('
    $content = $content -replace '(\s+)(var exists = )IsConversationExists\(', '$1$2await IsConversationExists('
    $content = $content -replace '(\s+)(var unreadCounts = )GetUnreadCounts\(', '$1$2await GetUnreadCounts('
    $content = $content -replace '(\s+)(var groups = )GetUserGroups\(', '$1$2await GetUserGroups('
    $content = $content -replace '(\s+)(var groupMessages = )GetGroupMessages\(', '$1$2await GetGroupMessages('
    $content = $content -replace '(\s+)(var isMember = )IsInGroup\(', '$1$2await IsInGroup('
    $content = $content -replace '(\s+)(var isGroupMember = )IsGroupMember\(', '$1$2await IsGroupMember('
    $content = $content -replace '(\s+)(var availableMembers = )GetAvailableMembersForGroup\(', '$1$2await GetAvailableMembersForGroup('
    $content = $content -replace '(\s+)(List<ChatMessage> messages = )GetMessages\(', '$1$2await GetMessages('
    $content = $content -replace '(\s+)(UserInfo otherUser = )GetUserById\(', '$1$2await GetUserById('
    $content = $content -replace '(\s+)(List<UserInfo> users = )GetAllUsersWithLastMessage\(', '$1$2await GetAllUsersWithLastMessage('
    $content = $content -replace '(\s+)(bool conversationExists = )IsConversationExists\(', '$1$2await IsConversationExists('
    $content = $content -replace '(\s+)(Dictionary<string, int> unreadCounts = )GetUnreadCounts\(', '$1$2await GetUnreadCounts('
    $content = $content -replace '(\s+)(List<GroupInfo> groups = )GetUserGroups\(', '$1$2await GetUserGroups('
    $content = $content -replace '(\s+)(List<GroupMessage> messages = )GetGroupMessages\(', '$1$2await GetGroupMessages('
    $content = $content -replace '(\s+)(bool isMember = )IsInGroup\(', '$1$2await IsInGroup('
    $content = $content -replace '(\s+)(bool isGroupMember = )IsGroupMember\(', '$1$2await IsGroupMember('
    $content = $content -replace '(\s+)(List<UserInfo> availableMembers = )GetAvailableMembersForGroup\(', '$1$2await GetAvailableMembersForGroup('
    
    Set-Content -Path $file -Value $content -NoNewline
    Write-Host "Added await keywords to UserDashboardController" -ForegroundColor Green
}
