$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'Build.ps1')
$destination = Join-Path $PSScriptRoot 'dist'
New-Item -ItemType Directory -Path $destination -Force | Out-Null
$staging = Join-Path $destination ('stage-' + [Guid]::NewGuid().ToString('N'))
$app = Join-Path $staging 'DashCanvas'
New-Item -ItemType Directory -Path $app -Force | Out-Null
foreach ($name in @('DashCanvas.exe','README.md','LICENSE','THIRD_PARTY_NOTICES.md','CHANGELOG.md','Build.ps1','Test.ps1','Package.ps1','CONTRIBUTING.md')) {
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot $name) -Destination $app
}
foreach ($name in @('source','tests','docs')) {
    $item = Join-Path $PSScriptRoot $name
    if (Test-Path -LiteralPath $item) { Copy-Item -LiteralPath $item -Destination $app -Recurse }
}
$archive = Join-Path $destination 'DashCanvas-Portable.zip'
Compress-Archive -LiteralPath $app -DestinationPath $archive -Force
Write-Output "Created $archive"
