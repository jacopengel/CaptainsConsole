$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$outDir = Join-Path $root 'dist'
$exe = Join-Path $outDir 'WindroseCaptainsConsole.exe'
$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$shipWheelPng = Join-Path $root 'shipwheel.png'
$appIconIco = Join-Path $PSScriptRoot 'app-icon.ico'
$packageDlls = @(
  (Join-Path $root 'packages\Discord.Net.Core.3.18.0\lib\net461\Discord.Net.Core.dll'),
  (Join-Path $root 'packages\Discord.Net.Rest.3.18.0\lib\net461\Discord.Net.Rest.dll'),
  (Join-Path $root 'packages\Discord.Net.WebSocket.3.18.0\lib\net461\Discord.Net.WebSocket.dll'),
  (Join-Path $root 'packages\Newtonsoft.Json.13.0.3\lib\net45\Newtonsoft.Json.dll'),
  (Join-Path $root 'packages\Microsoft.Bcl.AsyncInterfaces.6.0.0\lib\net461\Microsoft.Bcl.AsyncInterfaces.dll'),
  (Join-Path $root 'packages\System.Collections.Immutable.8.0.0\lib\net462\System.Collections.Immutable.dll'),
  (Join-Path $root 'packages\System.Runtime.CompilerServices.Unsafe.6.0.0\lib\net461\System.Runtime.CompilerServices.Unsafe.dll'),
  (Join-Path $root 'packages\System.Threading.Tasks.Extensions.4.5.4\lib\net461\System.Threading.Tasks.Extensions.dll'),
  (Join-Path $root 'packages\System.Memory.4.5.5\lib\net461\System.Memory.dll'),
  (Join-Path $root 'packages\System.Buffers.4.5.1\lib\net461\System.Buffers.dll'),
  (Join-Path $root 'packages\System.Linq.Async.6.0.1\lib\net48\System.Linq.Async.dll'),
  (Join-Path $root 'packages\System.Interactive.Async.6.0.1\lib\net48\System.Interactive.Async.dll')
)

if (-not (Test-Path $csc)) {
  throw "Could not find C# compiler at $csc"
}

New-Item -ItemType Directory -Force -Path $outDir | Out-Null
$sources = Get-ChildItem -Path $PSScriptRoot -Recurse -Filter *.cs | Sort-Object FullName | Select-Object -ExpandProperty FullName
$resourceArgs = @()
$compilerArgs = @('/target:winexe', '/optimize+', "/out:$exe", '/r:System.dll', '/r:System.Core.dll', '/r:System.Drawing.dll', '/r:System.Windows.Forms.dll', '/r:System.Web.Extensions.dll', '/r:System.IO.Compression.dll', '/r:System.IO.Compression.FileSystem.dll', '/r:System.Net.Http.dll')

foreach ($packageDll in $packageDlls) {
  if (Test-Path $packageDll) {
    $compilerArgs += "/r:$packageDll"
  }
}

if (Test-Path $shipWheelPng) {
  Add-Type -AssemblyName System.Drawing
  $sourceBitmap = New-Object System.Drawing.Bitmap($shipWheelPng)
  $iconBitmap = New-Object System.Drawing.Bitmap 64, 64
  $graphics = [System.Drawing.Graphics]::FromImage($iconBitmap)
  $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
  $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
  $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
  $graphics.Clear([System.Drawing.Color]::Transparent)
  $graphics.DrawImage($sourceBitmap, 0, 0, 64, 64)
  $hicon = $iconBitmap.GetHicon()
  $icon = [System.Drawing.Icon]::FromHandle($hicon)
  try {
    $iconStream = [System.IO.File]::Create($appIconIco)
    try {
      $icon.Save($iconStream)
    }
    finally {
      $iconStream.Dispose()
    }
  }
  finally {
    $graphics.Dispose()
    $sourceBitmap.Dispose()
    $iconBitmap.Dispose()
    $icon.Dispose()
  }

  if (Test-Path $appIconIco) {
    $compilerArgs += "/win32icon:$appIconIco"
    $resourceArgs += "/resource:$appIconIco,WindroseServerManager.Resources.AppIcon"
  }
}

& $csc $compilerArgs $resourceArgs $sources

if ($LASTEXITCODE -ne 0) {
  throw "Desktop build failed."
}

if (Test-Path $appIconIco) {
  Remove-Item -LiteralPath $appIconIco -Force -ErrorAction SilentlyContinue
}

foreach ($artifact in @(
  (Join-Path $outDir 'version.dll'),
  (Join-Path $outDir 'THIRD_PARTY_NOTICES.md'),
  (Join-Path $outDir 'THIRD_PARTY_LICENSES')
)) {
  if (Test-Path $artifact) {
    Remove-Item -LiteralPath $artifact -Recurse -Force
  }
}

foreach ($packageDll in $packageDlls) {
  if (Test-Path $packageDll) {
    Copy-Item -LiteralPath $packageDll -Destination (Join-Path $outDir (Split-Path $packageDll -Leaf)) -Force
  }
}

Write-Output "Built $exe"
