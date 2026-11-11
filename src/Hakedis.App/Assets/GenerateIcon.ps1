# Renders the approved gold teacher-group icon at every Windows icon size.
$ErrorActionPreference='Stop'
Add-Type -AssemblyName PresentationFramework
[xml]$art=@"
<DrawingImage xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
 <DrawingImage.Drawing><DrawingGroup>
  <GeometryDrawing>
   <GeometryDrawing.Brush><LinearGradientBrush StartPoint="0,0" EndPoint="0.88,0.83"><GradientStop Color="#F6D999" Offset="0"/><GradientStop Color="#C99942" Offset="1"/></LinearGradientBrush></GeometryDrawing.Brush>
   <GeometryDrawing.Geometry><RectangleGeometry Rect="0,0,80,80" RadiusX="18" RadiusY="18"/></GeometryDrawing.Geometry>
  </GeometryDrawing>
  <GeometryDrawing Brush="#0B243A" Geometry="M9,19 L40,8 L71,19 L40,31 Z M24,27 L40,33 L56,27 V33 Q40,41 24,33 Z"/>
  <GeometryDrawing Geometry="M65,22 V33"><GeometryDrawing.Pen><Pen Brush="#0B243A" Thickness="3.2" StartLineCap="Round" EndLineCap="Round"/></GeometryDrawing.Pen></GeometryDrawing>
  <GeometryDrawing Brush="#34566E"><GeometryDrawing.Geometry><GeometryGroup><EllipseGeometry Center="20,48" RadiusX="5.5" RadiusY="5.5"/><EllipseGeometry Center="60,48" RadiusX="5.5" RadiusY="5.5"/></GeometryGroup></GeometryDrawing.Geometry></GeometryDrawing>
  <GeometryDrawing Brush="#34566E" Geometry="M9,67 V64 Q9,55 20,55 Q31,55 31,64 V67 Z M49,67 V64 Q49,55 60,55 Q71,55 71,64 V67 Z"/>
  <GeometryDrawing Brush="#0B243A"><GeometryDrawing.Geometry><EllipseGeometry Center="40,45" RadiusX="7.5" RadiusY="7.5"/></GeometryDrawing.Geometry></GeometryDrawing>
  <GeometryDrawing Brush="#0B243A" Geometry="M23,69 V65 Q23,54 40,54 Q57,54 57,65 V69 Z"><GeometryDrawing.Pen><Pen Brush="#E8C778" Thickness="2"/></GeometryDrawing.Pen></GeometryDrawing>
  <GeometryDrawing Brush="#E8C778"><GeometryDrawing.Pen><Pen Brush="#0B243A" Thickness="2.6"/></GeometryDrawing.Pen><GeometryDrawing.Geometry><EllipseGeometry Center="63,65" RadiusX="10.5" RadiusY="10.5"/></GeometryDrawing.Geometry></GeometryDrawing>
  <GeometryDrawing Geometry="M58,65 L61.5,68.5 L68,61.5"><GeometryDrawing.Pen><Pen Brush="#0B243A" Thickness="3.5" StartLineCap="Round" EndLineCap="Round" LineJoin="Round"/></GeometryDrawing.Pen></GeometryDrawing>
 </DrawingGroup></DrawingImage.Drawing>
</DrawingImage>
"@
$image=[Windows.Markup.XamlReader]::Load([Xml.XmlNodeReader]::new($art))
$frames=@()
foreach($size in @(16,24,32,48,64,128,256))
{
    $visual=[Windows.Media.DrawingVisual]::new()
    $context=$visual.RenderOpen()
    $context.DrawImage($image,[Windows.Rect]::new(0,0,$size,$size))
    $context.Close()
    $bitmap=[Windows.Media.Imaging.RenderTargetBitmap]::new($size,$size,96,96,[Windows.Media.PixelFormats]::Pbgra32)
    $bitmap.Render($visual)
    $encoder=[Windows.Media.Imaging.PngBitmapEncoder]::new()
    $encoder.Frames.Add([Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
    $stream=[IO.MemoryStream]::new()
    try
    {
        $encoder.Save($stream)
        $bytes=$stream.ToArray()
        $frames+=,@($size,$bytes)
        if($size -eq 256){[IO.File]::WriteAllBytes((Join-Path $PSScriptRoot 'Hakedis.png'),$bytes)}
    }
    finally{$stream.Dispose()}
}
$output = [IO.File]::Create((Join-Path $PSScriptRoot 'Hakedis.ico'))
$writer = [IO.BinaryWriter]::new($output)
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
$writer.Dispose()
