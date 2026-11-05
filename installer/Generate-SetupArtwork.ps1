# Rebuild installer artwork from this application's own icon; no user data is read.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$assets = Join-Path $PSScriptRoot 'Assets'
New-Item -ItemType Directory -Path $assets -Force | Out-Null
$icon = [Drawing.Image]::FromFile((Join-Path $PSScriptRoot '..\src\Hakedis.App\Assets\Hakedis.png'))
function Write-Artwork([string]$Name, [int]$Width, [int]$Height, [bool]$Sidebar) {
    $bitmap = [Drawing.Bitmap]::new($Width, $Height)
    $g = [Drawing.Graphics]::FromImage($bitmap)
    $g.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.TextRenderingHint = [Drawing.Text.TextRenderingHint]::AntiAliasGridFit
    $g.Clear([Drawing.ColorTranslator]::FromHtml('#10263D'))
    $white = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#F0F6FC'))
    $muted = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#B9CDDF'))
    $gold = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#E3BA61'))
    $line = [Drawing.Pen]::new([Drawing.ColorTranslator]::FromHtml('#304C63'), 2)
    $title = [Drawing.Font]::new('Segoe UI', 43, [Drawing.FontStyle]::Bold, [Drawing.GraphicsUnit]::Pixel)
    $body = [Drawing.Font]::new('Segoe UI', 32, [Drawing.FontStyle]::Regular, [Drawing.GraphicsUnit]::Pixel)
    $small = [Drawing.Font]::new('Segoe UI', 25, [Drawing.FontStyle]::Regular, [Drawing.GraphicsUnit]::Pixel)
    if ($Sidebar) {
        $g.DrawRectangle($line, 38, 46, 416, 848)
        $g.DrawImage($icon, 72, 103, 116, 116)
        $g.DrawString("EK DERS`nHAKEDİŞ", $title, $white, 70, 270)
        $g.FillRectangle($gold, 74, 393, 144, 5)
        $g.DrawString("Öğretmen kayıtları.`nAylık ders bilgileri.`nGüvenilir bordrolar.", $body, $white, 70, 444)
        $g.DrawString("Düzenli kayıtlar.`nGüvenli çalışma.", $small, $muted, 70, 655)
        $g.DrawString('WINDOWS x64', $small, $gold, 70, 821)
    } else {
        $g.DrawImage($icon, 4, 4, $Width - 8, $Height - 8)
    }
    $bitmap.Save((Join-Path $assets $Name), [Drawing.Imaging.ImageFormat]::Png)
    foreach ($resource in @($g, $bitmap, $white, $muted, $gold, $line, $title, $body, $small)) { $resource.Dispose() }
}
try {
    Write-Artwork 'setup-sidebar.png' 492 942 $true
    Write-Artwork 'setup-mark.png' 192 192 $false
} finally { $icon.Dispose() }
