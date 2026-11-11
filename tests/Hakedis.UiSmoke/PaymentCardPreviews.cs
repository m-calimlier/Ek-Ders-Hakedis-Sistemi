using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

internal static class PaymentCardPreviews
{
    private static readonly Brush Gold=Ink("#E8C778");
    private static readonly Brush White=Ink("#F0F6FC");
    private static readonly Brush Navy=Ink("#081D2E");
    private static readonly string[] Names=["Saydam sembol","Altın şerit","Ayrı simge alanı","Başlık bandı","Halka vurgu"];

    public static void Render(string output,Action<FrameworkElement,string> capture)
    {
        var panel=new StackPanel{Margin=new(28)};
        panel.Children.Add(new TextBlock{Text="Hakediş kartları",Foreground=White,FontSize=26,FontWeight=FontWeights.SemiBold});
        var options=new UniformGrid{Columns=3,Margin=new(0,18,0,0)};
        for(var index=0;index<Names.Length;index++)
        {
            var option=new StackPanel{Margin=new(0,0,16,18)};
            option.Children.Add(new TextBlock
            {
                Text=$"{index+1} · {Names[index]}",Foreground=White,FontSize=14,
                FontWeight=FontWeights.SemiBold,Margin=new(0,0,0,8)
            });
            option.Children.Add(Card(index));
            options.Children.Add(option);
        }
        panel.Children.Add(options);
        var window=new Window
        {
            Style=null,WindowStyle=WindowStyle.None,Width=1160,Height=475,
            Background=Navy,Content=panel,ShowInTaskbar=false,WindowStartupLocation=WindowStartupLocation.CenterScreen
        };
        window.Show();
        window.Dispatcher.Invoke(()=>{},DispatcherPriority.ApplicationIdle);
        capture(window,Path.Combine(output,"bes-kart-tasarimi.png"));
        window.Close();
        Console.WriteLine("Beş lacivert ve altın kart seçeneği WPF ile çizildi.");
    }

    private static Border Card(int index)
    {
        var surface=new Grid{ClipToBounds=true};
        var title=new TextBlock{Text="NET ÖDEME",Foreground=Gold,FontSize=11,FontWeight=FontWeights.SemiBold};
        var value=new TextBlock{Text="107.555,17 TL",Foreground=White,FontSize=28,FontWeight=FontWeights.SemiBold};
        var words=new StackPanel
        {
            Margin=new(18,0,15,0),VerticalAlignment=VerticalAlignment.Center,
            HorizontalAlignment=HorizontalAlignment.Left
        };
        var border=new Border
        {
            Height=133,CornerRadius=new(12),BorderThickness=new(1),BorderBrush=Ink("#365165"),
            Background=new LinearGradientBrush(Color.FromRgb(20,47,67),Color.FromRgb(9,30,47),36),
            Child=surface
        };
        if(index==4)
        {
            words.Children.Add(value);
            title.Margin=new(0,6,0,0);
            words.Children.Add(title);
        }
        else
        {
            words.Children.Add(title);
            value.Margin=new(0,10,0,0);
            words.Children.Add(value);
        }
        switch(index)
        {
            case 0:
            {
                var mark=Symbol("M3,8 H27 M20,2 L27,8 L20,14 M28,23 H4 M11,17 L4,23 L11,29",117);
                mark.Opacity=.21;
                mark.HorizontalAlignment=HorizontalAlignment.Right;
                mark.VerticalAlignment=VerticalAlignment.Bottom;
                mark.Margin=new(0,0,7,-17);
                surface.Children.Add(mark);
                break;
            }
            case 1:
                border.BorderBrush=Ink("#8B744C");
                words.Margin=new(23,0,0,0);
                surface.Children.Add(new Border
                {
                    Width=3,Height=64,CornerRadius=new(0,3,3,0),Background=Gold,
                    HorizontalAlignment=HorizontalAlignment.Left,VerticalAlignment=VerticalAlignment.Center
                });
                surface.Children.Add(SymbolTile("M4,6 H28 V25 H4 Z M4,11 Q10,11 10,6 M22,25 Q22,19 28,19 M16,11 A5,5 0 1 1 15.99,11",56,12));
                break;
            case 2:
            {
                var symbol=Symbol("M5,3 H20 L27,10 V29 H5 Z M20,3 V10 H27 M10,15 H22 M10,20 H22 M10,25 H18",51);
                var compartment=new Border
                {
                    Width=101,Background=Ink("#18384E"),BorderBrush=Ink("#726344"),
                    BorderThickness=new(1,0,0,0),CornerRadius=new(0,11,11,0),
                    HorizontalAlignment=HorizontalAlignment.Right,Child=symbol
                };
                surface.Children.Add(compartment);
                break;
            }
            case 3:
            {
                var header=new Border
                {
                    Height=37,Background=Ink("#1B3B50"),BorderThickness=new(0,0,0,1),
                    BorderBrush=Ink("#7A6848"),CornerRadius=new(11,11,0,0),
                    VerticalAlignment=VerticalAlignment.Top,Padding=new(17,0,0,0)
                };
                words.Children.Remove(title);
                title.VerticalAlignment=VerticalAlignment.Center;
                header.Child=title;
                surface.Children.Add(header);
                words.Margin=new(17,30,0,0);
                surface.Children.Add(SymbolTile("M5,4 H27 V28 H5 Z M10,11 H22 M10,16 H22 M10,21 H15 M17,23 L20,26 L26,19",45,9));
                break;
            }
            case 4:
            {
                value.Foreground=Gold;
                var ring=SymbolTile("M4,9 H27 V24 H4 Z M9,9 V6 H24 V9 M24,13 H29 V20 H24 Q20,20 20,16.5 Q20,13 24,13 Z",68,34);
                ring.BorderBrush=Ink("#9B8050");
                ring.BorderThickness=new(1);
                surface.Children.Add(ring);
                break;
            }
        }
        surface.Children.Add(words);
        return border;
    }

    private static Border SymbolTile(string path,double size,double radius)=>new()
    {
        Width=size,Height=size,CornerRadius=new(radius),Background=Ink("#1C3D52"),
        HorizontalAlignment=HorizontalAlignment.Right,VerticalAlignment=VerticalAlignment.Center,
        Margin=new(0,0,15,0),Child=Symbol(path,size*.56)
    };

    private static Viewbox Symbol(string path,double size)=>new()
    {
        Width=size,Height=size,Child=new System.Windows.Shapes.Path
        {
            Width=32,Height=32,Data=Geometry.Parse(path),Stroke=Gold,StrokeThickness=1.8,
            StrokeLineJoin=PenLineJoin.Round,StrokeStartLineCap=PenLineCap.Round,StrokeEndLineCap=PenLineCap.Round
        }
    };

    private static Brush Ink(string value)=>(Brush)new BrushConverter().ConvertFromString(value)!;
}
