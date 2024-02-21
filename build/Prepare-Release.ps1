$ErrorActionPreference = 'Stop'

$distDirectory = Join-Path $PSScriptRoot '..' 'dist'
$publishDirectory = Join-Path $PSScriptRoot '..' 'publish'

if(-not (Test-Path $distDirectory)) {
  New-Item $distDirectory -ItemType Directory | Out-Null
}

$runtimes = @('win-x64', 'linux-x64', 'linux-arm64')

foreach ($runtime in $runtimes) {
  Write-Host "Preparing $runtime..."

  $suffix = $runtime.StartsWith('win') ? '.exe' : ''
  $cliPath = Join-Path $publishDirectory $runtime "portal-cli$suffix"

  Copy-Item $cliPath (Join-Path $distDirectory "portal-cli-$runtime$suffix")

  Compress-Archive -Path $cliPath -DestinationPath (Join-Path $distDirectory "portal-cli-$runtime.zip")

  tar -czf (Join-Path $distDirectory "portal-cli-$runtime.tar.gz") -C (Join-Path $publishDirectory $runtime) "portal-cli$suffix"
  if($LASTEXITCODE -ne 0) {
    Write-Error 'tar failed'
  }
}

Write-Host "Done!"
