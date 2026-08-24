param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("feature", "fix", "release", "hotfix")]
    [string]$Type,

    [Parameter(Mandatory = $true)]
    [string]$Name
)

$sanitizedName = $Name.Trim().ToLower() -replace "\s+", "-"
if ([string]::IsNullOrWhiteSpace($sanitizedName)) {
    throw "Branch name cannot be empty."
}

if ($sanitizedName -notmatch "^[a-z0-9._/-]+$") {
    throw "Branch name can contain only a-z, 0-9, ., _, /, and -."
}

$baseBranch = if ($Type -eq "hotfix") { "main" } else { "develop" }
$newBranch = "$Type/$sanitizedName"

Write-Host "Switching to base branch: $baseBranch"
git checkout $baseBranch
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Creating branch: $newBranch"
git checkout -b $newBranch
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Done. Current branch:" -ForegroundColor Green
git branch --show-current
