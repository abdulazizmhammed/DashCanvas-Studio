$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$testRoot = Join-Path $PSScriptRoot 'test-results'
New-Item -ItemType Directory -Path $testRoot -Force | Out-Null
$exe = Join-Path $testRoot 'DecorationTests.exe'
$sources = Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'source') -Filter '*.cs' | Select-Object -ExpandProperty FullName
& $compiler /nologo /target:exe /main:DecorationTests "/out:$exe" /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Web.Extensions.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll $sources (Join-Path $PSScriptRoot 'tests\DecorationTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Test compilation failed' }
& $exe $testRoot
if ($LASTEXITCODE -ne 0) { throw 'Decoration tests failed' }
