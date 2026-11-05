using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Hakedis.App;

/// <summary>Applies the shared window palette to native title bars without touching documents.</summary>
public static class NightTheme
{
    public static readonly DependencyProperty EnabledProperty=DependencyProperty.RegisterAttached(
        "Enabled",typeof(bool),typeof(NightTheme),new PropertyMetadata(false,Changed));
    public static bool GetEnabled(DependencyObject target)=>(bool)target.GetValue(EnabledProperty);
    public static void SetEnabled(DependencyObject target,bool value)=>target.SetValue(EnabledProperty,value);
    private static void Changed(DependencyObject target,DependencyPropertyChangedEventArgs args)
    {
        if(target is not Window window)return;
        window.SourceInitialized-=SourceInitialized;
        if((bool)args.NewValue){window.SourceInitialized+=SourceInitialized;Apply(window);}
    }
    private static void SourceInitialized(object? sender,EventArgs args){if(sender is Window window)Apply(window);}
    private static void Apply(Window window)
    {
        if(window.WindowStyle==WindowStyle.None)return;
        var handle=new WindowInteropHelper(window).Handle;
        if(handle==IntPtr.Zero)return;
        // Older Windows versions may ignore these appearance attributes.
        int enabled=1;_ = DwmSetWindowAttribute(handle,20,ref enabled,sizeof(int));
        int navy=0x00321E06;_ = DwmSetWindowAttribute(handle,35,ref navy,sizeof(int));
    }
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr handle,int attribute,ref int value,int size);
}
