$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$distDir = Join-Path $root 'dist'
$stageDir = Join-Path $distDir 'installer-stage'
$assetsDir = Join-Path $distDir 'installer-assets'
$desktopBuildScript = Join-Path $root 'desktop\build-desktop.ps1'
$issScript = Join-Path $PSScriptRoot 'WindroseCaptainsConsole.iss'
$assemblyInfoPath = Join-Path $root 'desktop\Properties\AssemblyInfo.cs'
$shipWheelPng = Join-Path $root 'shipwheel.png'
$githubRepoBaseUrl = 'https://github.com/jacopengel/CaptainsConsole'
$githubRawBaseUrl = 'https://raw.githubusercontent.com/jacopengel/CaptainsConsole/main'

if (-not (Test-Path $desktopBuildScript)) {
  throw "Could not find desktop build script at $desktopBuildScript"
}

if (-not (Test-Path $issScript)) {
  throw "Could not find installer script at $issScript"
}

function New-InstallerBitmap {
  param(
    [Parameter(Mandatory = $true)][string]$Path,
    [Parameter(Mandatory = $true)][int]$Width,
    [Parameter(Mandatory = $true)][int]$Height,
    [Parameter(Mandatory = $true)][string]$ShipWheelPath
  )

  Add-Type -AssemblyName System.Drawing
  $bitmap = New-Object System.Drawing.Bitmap -ArgumentList $Width, $Height
  $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
  $sourceBitmap = $null
  try {
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality

    $background = [System.Drawing.Color]::FromArgb(21, 33, 46)
    $accent = [System.Drawing.Color]::FromArgb(227, 197, 122)
    $muted = [System.Drawing.Color]::FromArgb(196, 205, 213)
    $sea = [System.Drawing.Color]::FromArgb(49, 133, 126)
    $foam = [System.Drawing.Color]::FromArgb(112, 163, 214)

    $graphics.Clear($background)

    if ($Width -lt 120) {
      $borderPen = New-Object System.Drawing.Pen -ArgumentList ([System.Drawing.Color]::FromArgb(54, 80, 103)), 1
      $ringPen = New-Object System.Drawing.Pen -ArgumentList ([System.Drawing.Color]::FromArgb(70, $sea)), 2
      try {
        $graphics.DrawRectangle($borderPen, 0, 0, $Width - 1, $Height - 1)
        $graphics.DrawEllipse($ringPen, 5, 5, $Width - 11, $Height - 11)
      }
      finally {
        $borderPen.Dispose()
        $ringPen.Dispose()
      }
    }
    else {
      $rectTop = New-Object System.Drawing.Rectangle -ArgumentList 0, 0, $Width, 120
      $topBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush -ArgumentList $rectTop, ([System.Drawing.Color]::FromArgb(36, 52, 68)), $background, 90.0
      try {
        $graphics.FillRectangle($topBrush, $rectTop)
      }
      finally {
        $topBrush.Dispose()
      }

      $linePen = New-Object System.Drawing.Pen -ArgumentList $accent, 2
      $titleFont = New-Object System.Drawing.Font -ArgumentList "Georgia", 11.8, ([System.Drawing.FontStyle]::Bold)
      $captionFont = New-Object System.Drawing.Font -ArgumentList "Segoe UI", 7.6, ([System.Drawing.FontStyle]::Regular)
      $accentBrush = New-Object System.Drawing.SolidBrush -ArgumentList $accent
      $mutedBrush = New-Object System.Drawing.SolidBrush -ArgumentList $muted
      try {
        $graphics.DrawLine($linePen, 16, 22, $Width - 16, 22)
        $graphics.DrawString("Windrose", $titleFont, $accentBrush, 16, 34)
        $graphics.DrawString("Captain's", $titleFont, $accentBrush, 16, 59)
        $graphics.DrawString("Console", $titleFont, $accentBrush, 16, 84)
        $graphics.DrawString("Dedicated server installer", $captionFont, $mutedBrush, 18, 111)
      }
      finally {
        $linePen.Dispose()
        $titleFont.Dispose()
        $captionFont.Dispose()
        $accentBrush.Dispose()
        $mutedBrush.Dispose()
      }
    }

    if (Test-Path $ShipWheelPath) {
      $sourceBitmap = New-Object System.Drawing.Bitmap -ArgumentList $ShipWheelPath
      $targetSize = if ($Width -ge 120) {
        [Math]::Min(104, [int]($Width * 0.68))
      }
      else {
        [Math]::Min($Width - 14, $Height - 14)
      }

      $wheelX = [int](($Width - $targetSize) / 2)
      $wheelY = if ($Width -ge 120) { 152 } else { [int](($Height - $targetSize) / 2) }

      $imageAttributes = New-Object System.Drawing.Imaging.ImageAttributes
      $matrix = New-Object System.Drawing.Imaging.ColorMatrix
      $matrix.Matrix33 = 1.0
      $imageAttributes.SetColorMatrix($matrix)
      try {
        $targetRect = New-Object System.Drawing.Rectangle -ArgumentList $wheelX, $wheelY, $targetSize, $targetSize
        $graphics.DrawImage(
          $sourceBitmap,
          $targetRect,
          0,
          0,
          $sourceBitmap.Width,
          $sourceBitmap.Height,
          [System.Drawing.GraphicsUnit]::Pixel,
          $imageAttributes)
      }
      finally {
        $imageAttributes.Dispose()
      }

      $glowAlpha = if ($Width -ge 120) { 90 } else { 110 }
      $glowWidth = if ($Width -ge 120) { 3 } else { 2 }
      $glowPen = New-Object System.Drawing.Pen -ArgumentList ([System.Drawing.Color]::FromArgb($glowAlpha, $sea)), $glowWidth
      try {
        $graphics.DrawEllipse($glowPen, $wheelX - 4, $wheelY - 4, $targetSize + 8, $targetSize + 8)
      }
      finally {
        $glowPen.Dispose()
      }

      if ($Width -ge 120) {
        $innerGlowPen = New-Object System.Drawing.Pen -ArgumentList ([System.Drawing.Color]::FromArgb(36, $foam)), 1
        try {
          $graphics.DrawEllipse($innerGlowPen, $wheelX + 8, $wheelY + 8, $targetSize - 16, $targetSize - 16)
        }
        finally {
          $innerGlowPen.Dispose()
        }
      }
    }

    if ($Width -ge 120) {
      $wavePen1 = New-Object System.Drawing.Pen -ArgumentList $foam, 2
      $wavePen2 = New-Object System.Drawing.Pen -ArgumentList $sea, 2
      try {
        $graphics.DrawArc($wavePen1, 20, $Height - 44, 40, 14, 0, 180)
        $graphics.DrawArc($wavePen2, 58, $Height - 40, 48, 14, 0, 180)
        $graphics.DrawArc($wavePen1, 102, $Height - 44, 34, 14, 0, 180)
      }
      finally {
        $wavePen1.Dispose()
        $wavePen2.Dispose()
      }
    }

    $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Bmp)
  }
  finally {
    if ($sourceBitmap) { $sourceBitmap.Dispose() }
    $graphics.Dispose()
    $bitmap.Dispose()
  }
}

function New-InstallerIcon {
  param(
    [Parameter(Mandatory = $true)][string]$Path,
    [Parameter(Mandatory = $true)][string]$ShipWheelPath
  )

  Add-Type -AssemblyName System.Drawing
  $sourceBitmap = New-Object System.Drawing.Bitmap -ArgumentList $ShipWheelPath
  $iconBitmap = New-Object System.Drawing.Bitmap -ArgumentList 64, 64
  $graphics = [System.Drawing.Graphics]::FromImage($iconBitmap)
  try {
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $targetSize = 56
    $offset = [int]((64 - $targetSize) / 2)
    $graphics.DrawImage($sourceBitmap, $offset, $offset, $targetSize, $targetSize)
    $hicon = $iconBitmap.GetHicon()
    $icon = [System.Drawing.Icon]::FromHandle($hicon)
    try {
      $iconStream = [System.IO.File]::Create($Path)
      try {
        $icon.Save($iconStream)
      }
      finally {
        $iconStream.Dispose()
      }
    }
    finally {
      $icon.Dispose()
    }
  }
  finally {
    $graphics.Dispose()
    $sourceBitmap.Dispose()
    $iconBitmap.Dispose()
  }
}

$version = '0.9.5'
if (Test-Path $assemblyInfoPath) {
  $assemblyInfo = Get-Content $assemblyInfoPath -Raw
  $match = [regex]::Match($assemblyInfo, 'AssemblyFileVersion\("([^"]+)"\)')
  if ($match.Success) {
    $version = $match.Groups[1].Value
  }
}

& powershell -ExecutionPolicy Bypass -File $desktopBuildScript
if ($LASTEXITCODE -ne 0) {
  throw "Desktop build failed."
}

$exePath = Join-Path $distDir 'WindroseCaptainsConsole.exe'
if (-not (Test-Path $exePath)) {
  throw "Expected built executable at $exePath"
}

New-Item -ItemType Directory -Force -Path $stageDir | Out-Null
New-Item -ItemType Directory -Force -Path $assetsDir | Out-Null
Copy-Item -LiteralPath $exePath -Destination (Join-Path $stageDir 'WindroseCaptainsConsole.exe') -Force

$readmePath = Join-Path $root 'README.md'
if (Test-Path $readmePath) {
  Copy-Item -LiteralPath $readmePath -Destination (Join-Path $stageDir 'README.md') -Force
}

if (-not (Test-Path $shipWheelPng)) {
  throw "Could not find shipwheel.png at $shipWheelPng"
}

$setupIconPath = Join-Path $assetsDir 'setup-icon.ico'
$wizardImagePath = Join-Path $assetsDir 'wizard-image.bmp'
$wizardSmallImagePath = Join-Path $assetsDir 'wizard-small.bmp'

New-InstallerIcon -Path $setupIconPath -ShipWheelPath $shipWheelPng
New-InstallerBitmap -Path $wizardImagePath -Width 164 -Height 314 -ShipWheelPath $shipWheelPng
New-InstallerBitmap -Path $wizardSmallImagePath -Width 55 -Height 55 -ShipWheelPath $shipWheelPng

$isccCandidates = @(
  'C:\Program Files (x86)\Inno Setup 6\ISCC.exe',
  'C:\Program Files\Inno Setup 6\ISCC.exe'
)
$iscc = $isccCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $iscc) {
  Write-Output "Installer staging completed at $stageDir"
  throw "Inno Setup 6 is not installed. Install it, then rerun installer\build-installer.ps1 to produce Setup.exe."
}

$safeVersion = $version -replace '[^0-9A-Za-z._-]', '_'
$outputBaseFileName = "WindroseCaptainsConsoleSetup_v$safeVersion"
$setupExeName = "$outputBaseFileName.exe"
$manifestUrl = "$githubRawBaseUrl/dist/update-manifest.json"
$downloadUrl = "$githubRawBaseUrl/dist/$setupExeName"
$updateManifestPath = Join-Path $distDir 'update-manifest.json'
$updateFeedPath = Join-Path $distDir 'update-feed-url.txt'

$updateManifest = [ordered]@{
  version = $version
  downloadUrl = $downloadUrl
  notes = "Latest installer build for Windrose Captain's Console."
}

$updateManifest | ConvertTo-Json | Set-Content -LiteralPath $updateManifestPath -Encoding UTF8
Set-Content -LiteralPath $updateFeedPath -Value $manifestUrl -Encoding UTF8

Copy-Item -LiteralPath $updateFeedPath -Destination (Join-Path $stageDir 'update-feed-url.txt') -Force

& $iscc "/DMyAppVersion=$version" "/DMyAppSourceDir=$stageDir" "/DMyAppExeName=WindroseCaptainsConsole.exe" "/DMyOutputBaseFilename=$outputBaseFileName" "/DMySetupIconFile=$setupIconPath" "/DMyWizardImageFile=$wizardImagePath" "/DMyWizardSmallImageFile=$wizardSmallImagePath" $issScript
if ($LASTEXITCODE -ne 0) {
  throw "Installer build failed."
}

Write-Output "Built installer $outputBaseFileName.exe in $distDir"
