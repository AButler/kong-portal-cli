param(
  [Parameter(Mandatory=$true)]
  [string]$Runtime,
  [Parameter(Mandatory=$true)]
  [string]$Version,
  [string]$VersionSuffix
)

$ErrorActionPreference = 'Stop'

$distDirectory = Join-Path $PSScriptRoot '..' 'dist'
$publishDirectory = Join-Path $PSScriptRoot '..' 'publish' $Runtime

if(-not (Test-Path $distDirectory)) {
  New-Item $distDirectory -ItemType Directory | Out-Null
}

Write-Host "Publishing $Runtime..."

dotnet publish src/CLI `
  -r $Runtime `
  -o $publishDirectory `
  -p:PublishSingleFile=true `
  -p:SelfContained=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:VersionPrefix="$Version" `
  --version-suffix "$VersionSuffix"

$isWin = $Runtime.StartsWith('win')

$suffix = $isWin ? '.exe' : ''
$cliPath = Join-Path $publishDirectory "portal-cli$suffix"

Copy-Item $cliPath (Join-Path $distDirectory "portal-cli-$runtime$suffix")

if ($isWin)
{
  Compress-Archive -Path $cliPath -DestinationPath (Join-Path $distDirectory "portal-cli-$Runtime.zip")
}
else
{
  tar -czf (Join-Path $distDirectory "portal-cli-$Runtime.tar.gz") -C $publishDirectory "portal-cli$suffix"
  if ($LASTEXITCODE -ne 0)
  {
    Write-Error 'tar failed'
  }
}

Write-Host "Done!"
