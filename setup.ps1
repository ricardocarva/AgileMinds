# setup.ps1

# Check if Chocolatey is installed
if (!(Get-Command choco -ErrorAction SilentlyContinue)) {
    Write-Host "Chocolatey is not installed. Installing Chocolatey..."
    
    # Set execution policy for the process to bypass restrictions temporarily
    Set-ExecutionPolicy Bypass -Scope Process -Force;
    
    # Ensure system is using a compatible security protocol (TLS 1.2)
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072;
    
    # Download and install Chocolatey
    iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'));
    
    # Verify if installation was successful
    if (Get-Command choco -ErrorAction SilentlyContinue) {
        Write-Host "Chocolatey installed successfully."
    } else {
        Write-Host "Chocolatey installation failed."
        exit 1
    }
} else {
    Write-Host "Chocolatey is already installed. Skipping installation."
}

# Proceed with the rest of your script (e.g., install gitleaks)
Write-Host "Installing Gitleaks..."
choco install gitleaks -y


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


