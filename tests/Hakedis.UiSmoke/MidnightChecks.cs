using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

internal static class MidnightChecks
{
    internal static void Run(string output, Action<FrameworkElement,string> capture)
    {
        var resources=Application.Current.Resources;
        if(((SolidColorBrush)resources["Navy900Brush"]).Color!=Color.FromRgb(6,30,50))
            throw new Exception("Onaylanan Gece Mavisi paleti uygulanmadı.");
        var combo=new ComboBox {Style=(Style)resources["FormCombo"],Width=260,ItemsSource=new[]{"Modül seçiniz","Din Öğretimi"},SelectedIndex=1};
        var list=new ListBox {Style=(Style)resources["ModernListBox"],ItemsSource=new[]{"Birinci öğretmen","İkinci öğretmen"},SelectedIndex=0,Height=100};
        var table=new DataGrid {Style=(Style)resources["ModernDataGrid"],ItemsSource=new[]{new {Ogretmen="Örnek öğretmen",Saat=80}},AutoGenerateColumns=true,Height=100};
        var date=new DatePicker {Style=(Style)resources["FormDate"],SelectedDate=new DateTime(2026,11,5)};
        var defaultDate=new DatePicker {SelectedDate=new DateTime(2026,11,6)};
        var windowButton=new Button {Style=(Style)resources["WindowButton"],Content="×"};
        var stack=new StackPanel {Margin=new Thickness(24)};
        foreach(var child in new FrameworkElement[]{combo,list,table,date,defaultDate,windowButton,new TextBox{Style=(Style)resources["FormText"],Text="80"}})stack.Children.Add(child);
        var window=new Window {Title="Gece Mavisi kontrolü",Width=650,Height=540,Content=stack};
        window.Show();Idle(window);
        if(!Light(windowButton.Foreground) || windowButton.Background is not SolidColorBrush transparent || transparent.Color.A!=0)throw new Exception("Pencere kontrol simgeleri açık renk ve zemini şeffaf olmalı.");
        var defaultDateText=(System.Windows.Controls.Primitives.DatePickerTextBox)defaultDate.Template.FindName("PART_TextBox",defaultDate);
        if(!Light(defaultDate.Foreground) || !Light(defaultDateText.Foreground) || defaultDate.CalendarStyle is null)throw new Exception("Stil belirtilmeyen banka/tablo tarih alanında tarih okunamıyor.");
        if(!Dark(window.Background) || !Light(combo.Foreground) || !Dark(list.Background) || !Dark(table.Background))throw new Exception("Pencere, seçim veya tablo yüzeyleri ortak koyu temaya uymuyor.");
        table.SelectedIndex=0;table.UpdateLayout();
        var row=(DataGridRow)table.ItemContainerGenerator.ContainerFromIndex(0);
        if(row is null || Contrast(row.Foreground,row.Background)<4.5)throw new Exception("Seçili tablo satırı okunaklı değil.");
        var item=(ListBoxItem)list.ItemContainerGenerator.ContainerFromIndex(0);
        if(item is null || !Dark(item.Foreground))throw new Exception("Seçili liste satırında açık arka plan üzerinde koyu metin bekleniyor.");
        capture(window,Path.Combine(output,"Midnight-controls.png"));
        combo.IsDropDownOpen=true;Idle(window);
        capture((FrameworkElement)((System.Windows.Controls.Primitives.Popup)combo.Template.FindName("PART_Popup",combo)).Child,Path.Combine(output,"Midnight-combo-popup.png"));combo.IsDropDownOpen=false;
        date.IsDropDownOpen=true;Idle(window);
        var calendar=Descendants<Calendar>(((System.Windows.Controls.Primitives.Popup)date.Template.FindName("PART_Popup",date)).Child).Single();
        if(!Dark(calendar.Background) || !Light(calendar.Foreground))throw new Exception("Takvim açılır alanı koyu temaya uymuyor.");
        var calendarItem=Descendants<System.Windows.Controls.Primitives.CalendarItem>(calendar).Single();
        var monthView=(Grid)calendarItem.Template.FindName("PART_MonthView",calendarItem);
        capture(calendar,Path.Combine(output,"Midnight-calendar-headers.png"));
        if(monthView.Children.Cast<UIElement>().Where(c=>Grid.GetRow(c)==0).Count(c=>Descendants<TextBlock>(c).Any(t=>!string.IsNullOrEmpty(t.Text)))!=7)
            throw new Exception("Takvim haftanın yedi gününü göstermiyor.");
        calendar.Focus();calendar.RaiseEvent(new System.Windows.Input.KeyEventArgs(System.Windows.Input.Keyboard.PrimaryDevice,PresentationSource.FromVisual(calendar),Environment.TickCount,System.Windows.Input.Key.Right){RoutedEvent=System.Windows.Input.Keyboard.KeyDownEvent});Idle(window);
        if(calendar.SelectedDate!=new DateTime(2026,11,6))throw new Exception("Takvimde sağ okla gün seçilemiyor.");
        var previous=calendar.DisplayDate;((Button)calendarItem.Template.FindName("PART_NextButton",calendarItem)).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Idle(window);
        if(calendar.DisplayDate.Month==previous.Month)throw new Exception("Takvimde sonraki ay düğmesi çalışmıyor.");
        ((Button)calendarItem.Template.FindName("PART_HeaderButton",calendarItem)).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Idle(window);
        if(calendar.DisplayMode!=CalendarMode.Year || ((Grid)calendarItem.Template.FindName("PART_YearView",calendarItem)).Visibility!=Visibility.Visible)throw new Exception("Takvimde ay/yıl seçimi açılmıyor.");
        ((Button)calendarItem.Template.FindName("PART_HeaderButton",calendarItem)).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Idle(window);
        if(calendar.DisplayMode!=CalendarMode.Decade)throw new Exception("Takvimde yıl seçimine geçilemiyor.");
        calendar.DisplayMode=CalendarMode.Month;calendar.DisplayDate=new DateTime(2026,11,5);Idle(window);
        calendar.SelectedDate=new DateTime(2026,12,12);Idle(window);
        if(date.SelectedDate!=calendar.SelectedDate)throw new Exception("Takvim seçimi tarih girişine aktarılmıyor.");
        capture((FrameworkElement)((System.Windows.Controls.Primitives.Popup)date.Template.FindName("PART_Popup",date)).Child,Path.Combine(output,"Midnight-calendar-popup.png"));date.IsDropDownOpen=false;
        window.Close();
        var vault=new Hakedis.App.VaultWindow(Path.Combine(output,"style-vault.edhskasa"));vault.Show();Idle(vault);
        if(!Hakedis.App.NightTheme.GetEnabled(vault))throw new Exception("Yardımcı pencerelerde yerel başlık çubuğu teması etkin değil.");
        vault.Close();
        var shell=new Hakedis.App.ShellViewModel(Path.Combine(output,"caption-data","ogretmenler.json"));
        var main=new Hakedis.App.MainWindow(shell,_=>{},(_,_)=>true){ShowActivated=false,ShowInTaskbar=false};main.Show();Idle(main);
        var icons=new[]{"MinimizeGlyph","MaximizeGlyph","CloseGlyph"}.Select(name=>main.FindName(name) as System.Windows.Shapes.Path).ToArray();
        if(icons.Any(p=>p is null || p.Width!=12 || p.Height!=12 || p.StrokeThickness!=1.2 || p.Stroke is not SolidColorBrush b || b.Color!=Colors.White))throw new Exception("Pencere simgeleri ortak boyut ve çizgi stili kullanmıyor.");
        var maximize=icons[1]!;var normal=maximize.Data.ToString();capture(main,Path.Combine(output,"Window-icons.png"));
        main.WindowState=WindowState.Maximized;Idle(main);
        if(maximize.Data.ToString()==normal)throw new Exception("Büyütülmüş pencerede geri yükleme simgesi gösterilmiyor.");
        shell.ClearCurrentWork();main.Close();
        Console.WriteLine("Gece Mavisi: giriş/liste/tablo/takvim kontrastı; ortak 12×12 beyaz pencere simgeleri ve büyüt/geri yükle durumu doğrulandı.");
    }
    private static bool Dark(Brush b)=>b is SolidColorBrush s && Luminance(s.Color)<.15;
    private static bool Light(Brush b)=>b is SolidColorBrush s && Luminance(s.Color)>.5;
    internal static double Contrast(Brush a,Brush b)=>a is SolidColorBrush x && b is SolidColorBrush y ? (Math.Max(Luminance(x.Color),Luminance(y.Color))+.05)/(Math.Min(Luminance(x.Color),Luminance(y.Color))+.05):0;
    private static double Luminance(Color c){double F(byte v){var s=v/255d;return s<=.04045?s/12.92:Math.Pow((s+.055)/1.055,2.4);}return .2126*F(c.R)+.7152*F(c.G)+.0722*F(c.B);}
    private static void Idle(Window w){w.Dispatcher.Invoke(()=>{},DispatcherPriority.ApplicationIdle);w.UpdateLayout();}
    private static IEnumerable<T> Descendants<T>(DependencyObject root) where T:DependencyObject
    {if(root is T t)yield return t;for(var i=0;i<VisualTreeHelper.GetChildrenCount(root);i++)foreach(var child in Descendants<T>(VisualTreeHelper.GetChild(root,i)))yield return child;}
}
