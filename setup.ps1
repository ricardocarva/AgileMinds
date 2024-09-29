# setup.ps1

# Set permission for certificates directory
$certDir = "./certificates"

# Check if certificates directory exists
if (-Not (Test-Path $certDir)) {
    Write-Host "Error: Certificates directory ($certDir) not found!" -ForegroundColor Red
    exit 1
}

# Set permission for certificates directory
Write-Host "Setting permissions for certificates directory..."
# Ensure current user has FullControl permissions on the certificates folder
icacls $certDir /grant "$($env:USERNAME):(OI)(CI)F" /T

# setup-hooks.ps1

# Get the .git/hooks directory
$hooksDir = ".git/hooks"

# Check if hooks directory exists
if (-Not (Test-Path $hooksDir)) {
    Write-Host "Error: .git/hooks directory not found!" -ForegroundColor Red
    exit 1
}

# Copy the hook files to the .git/hooks directory
Copy-Item -Path "hooks/pre-commit" -Destination "$hooksDir/pre-commit" -Force
Copy-Item -Path "hooks/pre-push" -Destination "$hooksDir/pre-push" -Force

# Set executable permissions for Git hooks (handled automatically on Windows)
Write-Host "Git hooks have been set up!"

Write-Host "Setup completed!"


