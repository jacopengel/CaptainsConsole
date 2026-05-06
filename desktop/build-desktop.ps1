$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$outDir = Join-Path $root 'dist'
$exe = Join-Path $outDir 'WindroseCaptainsConsole.exe'
$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'

if (-not (Test-Path $csc)) {
  throw "Could not find C# compiler at $csc"
}

New-Item -ItemType Directory -Force -Path $outDir | Out-Null
$sources = Get-ChildItem -Path $PSScriptRoot -Recurse -Filter *.cs | Sort-Object FullName | Select-Object -ExpandProperty FullName

& $csc /target:winexe /optimize+ /out:$exe /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Web.Extensions.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll $sources

if ($LASTEXITCODE -ne 0) {
  throw "Desktop build failed."
}

Write-Output "Built $exe"
