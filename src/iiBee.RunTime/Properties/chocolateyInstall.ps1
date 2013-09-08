try { 
    Write-Host "You may need to open a new console for the new path to take effect. Happy scripting!" -ForegroundColor DarkYellow
    Write-ChocolateySuccess 'iiBee'
} catch {
    Write-ChocolateyFailure 'iiBee' "$($_.Exception.Message)"
    throw 
}