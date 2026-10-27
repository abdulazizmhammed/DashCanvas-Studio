$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$destination = Join-Path $PSScriptRoot 'DashCanvas.exe'
$sources = Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'source') -Filter '*.cs' | Select-Object -ExpandProperty FullName
& $compiler /nologo /target:winexe /optimize+ /platform:anycpu "/out:$destination" "/win32manifest:$(Join-Path $PSScriptRoot 'source\app.manifest')" /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Web.Extensions.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll $sources
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
Write-Output "Built $destination"
