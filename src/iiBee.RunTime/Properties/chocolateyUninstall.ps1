try {
    Write-ChocolateySuccess 'iiBee'
} catch {
    Write-ChocolateyFailure 'iiBee' "$($_.Exception.Message)"
    throw 
}