param([string]$Version, [string]$PublishedDirectory)
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $Version) {
    [xml]$project = Get-Content -LiteralPath (Join-Path $repoRoot 'src/Hakedis.App/Hakedis.App.csproj') -Raw
    $Version = @($project.Project.PropertyGroup.Version | Where-Object { $_ })[0]
}
if ($Version -notmatch '^\d+\.\d+\.\d+$') { throw 'A three-component release version is required.' }
if (-not $PublishedDirectory) {
    $latest = Get-ChildItem -LiteralPath (Join-Path $repoRoot 'dist') -Directory |
        Where-Object Name -Like 'Ek Ders Hakediş Sistemi-win-x64-*' |
        Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
    if (-not $latest) { throw 'Publish the Windows x64 application first.' }
    $PublishedDirectory = $latest.FullName
}
$sourceExe = Join-Path $PublishedDirectory 'Ek Ders Hakediş Sistemi.exe'
if (-not (Test-Path -LiteralPath $sourceExe) -or (Get-Item -LiteralPath $sourceExe).VersionInfo.FileVersion -ne "$Version.0") {
    throw 'Published executable version does not match the release.'
}
$payload = Join-Path $repoRoot "dist/v$Version"
$allowed = @('Ek Ders Hakediş Sistemi.exe', 'KULLANIM.txt', 'NPOI-LICENSE.txt')
if (Test-Path -LiteralPath $payload) {
    $unexpected = @(Get-ChildItem -LiteralPath $payload -Force | Where-Object { $_.PSIsContainer -or $_.Name -notin $allowed })
    if ($unexpected.Count) { throw 'Release payload contains unexpected files or directories.' }
}
New-Item -ItemType Directory -Path $payload -Force | Out-Null
Copy-Item -LiteralPath $sourceExe -Destination (Join-Path $payload $allowed[0]) -Force
Copy-Item -LiteralPath (Join-Path $repoRoot 'docs/KULLANIM.txt') -Destination (Join-Path $payload $allowed[1]) -Force
Copy-Item -LiteralPath (Join-Path $repoRoot 'third_party/NPOI/LICENSE') -Destination (Join-Path $payload $allowed[2]) -Force
& (Join-Path $PSScriptRoot 'Build-Installer.ps1') -Version $Version | Out-Host
$portable = Join-Path $repoRoot "dist/Ek-Ders-Hakedis-Sistemi-$Version-win-x64.zip"
Compress-Archive -Path (Join-Path $payload '*') -DestinationPath $portable -Force
$installer = Join-Path $repoRoot "dist/installer/Ek-Ders-Hakedis-Sistemi-Kurulum-$Version.exe"
$sums = @($portable, $installer) | ForEach-Object {
    (Get-FileHash -LiteralPath $_ -Algorithm SHA256).Hash.ToLowerInvariant() + '  ' + [IO.Path]::GetFileName($_)
}
$sums | Set-Content -LiteralPath (Join-Path $repoRoot 'dist/SHA256SUMS.txt') -Encoding ascii
Write-Host "Release assets ready: $Version; payload contains only executable, usage notes and license."
