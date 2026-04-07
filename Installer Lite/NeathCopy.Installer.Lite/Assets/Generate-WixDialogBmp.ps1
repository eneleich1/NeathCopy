$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Drawing

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

$logoPath = 'D:\Company\Logo2.png'
$outputPath = Join-Path $scriptDir 'WixUIDialogBmp.bmp'

$width = 493
$height = 312
$leftPanelWidth = 164

$bitmap = New-Object System.Drawing.Bitmap $width, $height
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)

try {
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $graphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::ClearTypeGridFit

    $whiteBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 255, 255))
    $graphics.FillRectangle($whiteBrush, 0, 0, $width, $height)
    $whiteBrush.Dispose()

    $panelRect = New-Object System.Drawing.Rectangle 0, 0, $leftPanelWidth, $height
    $panelGradient = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        $panelRect,
        [System.Drawing.Color]::FromArgb(12, 57, 95),
        [System.Drawing.Color]::FromArgb(89, 201, 255),
        90
    )
    $graphics.FillRectangle($panelGradient, $panelRect)
    $panelGradient.Dispose()

    $overlayBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(50, 255, 255, 255))
    $graphics.FillEllipse($overlayBrush, -35, 28, 210, 120)
    $graphics.FillEllipse($overlayBrush, 15, 155, 170, 135)
    $overlayBrush.Dispose()

    $shapeBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(70, 4, 33, 64))
    $graphics.FillPolygon($shapeBrush, @(
        [System.Drawing.Point]::new(-10, 255),
        [System.Drawing.Point]::new(90, 120),
        [System.Drawing.Point]::new(190, 312),
        [System.Drawing.Point]::new(30, 312)
    ))
    $graphics.FillPolygon($shapeBrush, @(
        [System.Drawing.Point]::new(15, -15),
        [System.Drawing.Point]::new(110, 25),
        [System.Drawing.Point]::new(60, 135),
        [System.Drawing.Point]::new(-25, 85)
    ))
    $shapeBrush.Dispose()

    $borderPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(120, 230, 248, 255), 1)
    $graphics.DrawLine($borderPen, $leftPanelWidth - 1, 0, $leftPanelWidth - 1, $height)
    $borderPen.Dispose()

    $logo = [System.Drawing.Image]::FromFile($logoPath)
    try {
        $logoBounds = New-Object System.Drawing.Rectangle 26, 34, 112, 112
        $graphics.DrawImage($logo, $logoBounds)
    }
    finally {
        $logo.Dispose()
    }

    $titleFont = New-Object System.Drawing.Font('Segoe UI', 23, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
    $subtitleFont = New-Object System.Drawing.Font('Segoe UI', 11, [System.Drawing.FontStyle]::Regular, [System.Drawing.GraphicsUnit]::Pixel)
    $titleBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(245, 255, 255, 255))
    $subtitleBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(220, 223, 246, 255))

    $stringFormat = New-Object System.Drawing.StringFormat
    $stringFormat.Alignment = [System.Drawing.StringAlignment]::Center

    $graphics.DrawString('NeathCopy', $titleFont, $titleBrush, 82, 177, $stringFormat)
    $graphics.DrawString('Installer Lite', $subtitleFont, $subtitleBrush, 82, 222, $stringFormat)

    $titleFont.Dispose()
    $subtitleFont.Dispose()
    $titleBrush.Dispose()
    $subtitleBrush.Dispose()
    $stringFormat.Dispose()

    $bitmap.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Bmp)
}
finally {
    $graphics.Dispose()
    $bitmap.Dispose()
}
