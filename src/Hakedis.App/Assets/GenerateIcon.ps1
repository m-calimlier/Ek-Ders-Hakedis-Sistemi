# Regenerate the same approved icon from the canonical, 256px Hakedis.png artwork.
# IMPORTANT: Do not draw an alternative icon here; preserve the approved composition
# (graduation cap + three people + calculator + payroll document + checkmark).
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$sourceFile = Join-Path $PSScriptRoot 'Hakedis.png'
if (-not (Test-Path -LiteralPath $sourceFile)) { throw "Hakedis.png bulunamadı." }
$source = [Drawing.Image]::FromFile($sourceFile)
if ($source.Width -ne 256 -or $source.Height -ne 256) {
    $source.Dispose()
    throw "Hakedis.png 256x256 kaynak tasarım olmalıdır."
}
$frames = @()
try {
    foreach ($size in @(16, 24, 32, 48, 64, 128, 256)) {
        $bitmap = [Drawing.Bitmap]::new($size, $size, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $g = [Drawing.Graphics]::FromImage($bitmap)
        $g.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $g.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::HighQuality
        $g.CompositingQuality = [Drawing.Drawing2D.CompositingQuality]::HighQuality
        $g.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $g.Clear([Drawing.Color]::Transparent)
        $g.DrawImage($source, [Drawing.Rectangle]::new(0, 0, $size, $size))
        $stream = [IO.MemoryStream]::new()
        $bitmap.Save($stream, [Drawing.Imaging.ImageFormat]::Png)
        $frames += ,@($size, $stream.ToArray())
        $stream.Dispose()
        $g.Dispose()
        $bitmap.Dispose()
    }
}
finally { $source.Dispose() }
$out = [IO.File]::Create((Join-Path $PSScriptRoot 'Hakedis.ico'))
$writer = [IO.BinaryWriter]::new($out)
try {
    $writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]$frames.Count)
    $offset = 6 + 16 * $frames.Count
    foreach ($frame in $frames) {
        $dimension = if ($frame[0] -eq 256) { 0 } else { $frame[0] }
        $writer.Write([byte]$dimension); $writer.Write([byte]$dimension)
        $writer.Write([byte]0); $writer.Write([byte]0)
        $writer.Write([uint16]1); $writer.Write([uint16]32)
        $writer.Write([uint32]$frame[1].Length); $writer.Write([uint32]$offset)
        $offset += $frame[1].Length
    }
    foreach ($frame in $frames) { $writer.Write([byte[]]$frame[1]) }
}
finally { $writer.Dispose() }
Write-Host "Onaylanan tasarım korundu: Hakedis.ico yeniden oluşturuldu."
