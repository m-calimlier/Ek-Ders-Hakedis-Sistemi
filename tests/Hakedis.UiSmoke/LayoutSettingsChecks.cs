using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Markup;
using System.Xml.Linq;
using Hakedis.App;
using Hakedis.Contracts;
using Hakedis.Storage;

internal static class LayoutSettingsChecks
{
    internal static void Run(string output,Action<FrameworkElement,string> capture)
    {
        var root=Path.Combine(output,"settings-data");var model=new ShellViewModel(Path.Combine(root,"ogretmenler.json"));
        model.Registration.Editor.FullName="Görünüm Deneme Öğretmeni";model.Registration.Editor.IdentityNumber="10000000146";
        model.Registration.Editor.Iban="TR330006100519786457841326";model.Registration.Editor.OccupationCode="2341.04";model.SaveNewTeacher();
        model.Calculation.Rows[0].DayHours="100000";
        var window=new MainWindow(model,_=>{},(_,_)=>true){ShowActivated=false,ShowInTaskbar=false};
        window.Show();
        if(window.Title!=AppIdentity.Name || typeof(MainWindow).Assembly.GetName().Name!="Ek Ders Hakediş Sistemi")throw new Exception("Ek Ders Hakediş Sistemi adı güncellenmedi.");
        if(model.Workspace.Modules.Any(m=>m.Id==ModuleId.ImamHatipLisesi))throw new Exception("İmam Hatip listede kaldı.");
        model.Navigate("Constants");Idle(window);
        var fields=Descendants<TextBox>((FrameworkElement)window.FindName("ConstantsFields")).ToArray();
        if(fields.Any(t=>!t.IsReadOnly || ((SolidColorBrush)t.Background).Color!=Color.FromRgb(21,56,79)))throw new Exception("Sabitler kilitliyken koyu pasif yüzeyde değil.");
        ((Button)window.FindName("ConstantsUpdateButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Idle(window);
        if(fields.Any(t=>t.IsReadOnly || t.Background is not SolidColorBrush brush || brush.Color!=Color.FromRgb(35,55,78)))throw new Exception("Güncelle ortak giriş yüzeyiyle düzenlemeyi açmadı.");
        ((Button)window.FindName("ConstantsUpdateButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Idle(window);
        var grid=(DataGrid)window.FindName("HybridGrid");
        var transferHeaders=new[]{"ÖNCEKİ KURUM\nGÜNDÜZ","YENİ KURUM\nGÜNDÜZ","KURUM SAAT\nUYARISI"};
        if(grid.Columns.Where(c=>!transferHeaders.Contains(c.Header as string)).Sum(c=>c.Width.Value)>1800)throw new Exception("Hibrit sütunlar daraltılmadı.");
        foreach(var size in model.Settings.Sizes)
        {
            ((Button)window.FindName("NavSettings")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            ((ComboBox)window.FindName("WindowSizeCombo")).SelectedItem=size;Idle(window);
            ((Button)window.FindName("ApplyWindowSizeButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Idle(window);
            var fit=WindowSizing.Fit(new(size.Width,size.Height),SystemParameters.WorkArea.Width,SystemParameters.WorkArea.Height);
            if(Math.Abs(window.Width-fit.Width)>1 || Math.Abs(window.Height-fit.Height)>1)throw new Exception("Seçilen pencere ekran alanına sığmadı.");
            if(new UiPreferencesStore(root).Read()!=new UiPreferences(size.Width,size.Height))throw new Exception("Pencere tercihi saklanmadı.");
            // HWND küçük monitörde büyük pencereyi sınırlayabilir. İç WPF yüzeyini ayrıca gerçek 1920 DIP ile düzenle.
            window.Width=size.Width;window.Height=size.Height;Idle(window);
            var surface=PreviewSurface(window,size);
            capture(surface,Path.Combine(output,$"Settings-{size.Width}.png"));
            var settings=(Button)window.FindName("NavSettings");var signature=(TextBlock)window.FindName("SignatureAuthor");
            var last=settings.TransformToAncestor(window).TransformBounds(new Rect(0,0,settings.ActualWidth,settings.ActualHeight));
            var sig=signature.TransformToAncestor(window).TransformBounds(new Rect(0,0,signature.ActualWidth,signature.ActualHeight));
            if(last.Bottom>sig.Top || sig.Bottom>size.Height)throw new Exception("Küçük ekranda Ayarlar/imza çakışıyor.");
            foreach(var panel in new[]{"Entry","Details","Calculation","Teachers","Personal","Constants","Outputs"})
                        {
                model.Navigate(panel);Idle(window);
                var preview=PreviewSurface(window,size);
                if(panel=="Calculation")
                {
                    var actionLabels=new[]{"Hakediş oluştur","Çalışmayı temizle","Kayıtları yenile"};
                    var actions=Descendants<Button>(preview).Where(button=>button.ActualHeight>0 && actionLabels.Contains(button.Content as string)).ToArray();
                    var selectors=Descendants<ComboBox>(preview).Where(combo=>combo.ActualHeight>0).ToArray();
                    if(actions.Length!=3 || selectors.Length<3 || actions.Any(button=>Math.Abs(button.ActualHeight-35)>1 || button.FontSize!=12))
                        throw new Exception($"{size.Width}: Hakediş düğmelerinin ortak ölçüsü doğrulanamadı.");
                    var top=selectors[0].TranslatePoint(new Point(),preview).Y;
                    if(actions.Any(button=>Math.Abs(button.TranslatePoint(new Point(),preview).Y-top)>1))
                        throw new Exception($"{size.Width}: Hakediş düğmeleri açılır listelerle hizalı değil.");
                }
                if(panel=="Entry")
                {
                    var forms=Descendants<HeaderedContentControl>(preview)
                        .Where(card=>card.Header as string is "Öğretmen Bilgileri" or "Aylık Ders Bilgileri").ToArray();
                    if(forms.Length!=2 || forms.Any(card=>card.ActualHeight<=0) ||
                       Math.Abs(forms[0].ActualHeight-forms[1].ActualHeight)>1)
                        throw new Exception($"{size.Width}: Veri Girişi kartlarının yükseklikleri eşit değil.");
                }
                if(panel=="Constants")
                {
                    foreach(var name in new[]{"ConstantsFields","RetiredConstantsFields"})
                    {
                        var constants=(FrameworkElement)((UserControl)preview).FindName(name);
                        var editors=Descendants<TextBox>(constants).ToArray();
                        if(editors.Length!=9 || constants.ActualHeight>150)
                            throw new Exception($"{size.Width}: {name} dokuz alanlı kompakt düzende değil ({constants.ActualHeight}).");
                        if(editors.Any(editor=>editor.ActualHeight<30 || editor.ActualWidth<90))
                            throw new Exception($"{size.Width}: {name} düzenleme alanları okunamayacak kadar küçük.");
                    }
                }
                capture(preview,Path.Combine(output,$"Ek Ders Hakediş Sistemi-{panel}-{size.Width}.png"));
            }
            model.Navigate("Calculation");Idle(window);
            var metrics=(System.Windows.Controls.Primitives.UniformGrid)window.FindName("PaymentMetrics");
            metrics.BringIntoView();Idle(window);
            var cards=metrics.Children.OfType<HeaderedContentControl>().ToArray();PersonalPayrollChecks.VerifyMetricRegions(cards);
            if(cards.Length!=4 || cards.Select(c=>c.Tag).Distinct().Count()!=4 || cards.Any(c=>Descendants<Image>(c).Count(i=>i.Width>=16 && Equals(i.Source,c.Tag))!=1))throw new Exception("Alt sırada dört ayrı simge alanı bulunamadı.");
            var metricPreview=(UserControl)PreviewSurface(window,size);
            var previewMetrics=(System.Windows.Controls.Primitives.UniformGrid)metricPreview.FindName("PaymentMetrics");
            previewMetrics.BringIntoView();metricPreview.Dispatcher.Invoke(()=>{},DispatcherPriority.ApplicationIdle);metricPreview.UpdateLayout();
            PersonalPayrollChecks.VerifyMetricRegions(previewMetrics.Children.OfType<HeaderedContentControl>());
            capture(metricPreview,Path.Combine(output,$"Metrics-{size.Width}.png"));
        }
        model.ClearCurrentWork();window.Close();
        Console.WriteLine("Ek Ders Hakediş Sistemi: 1280×720 / 1366×768 / 1920×1080, kalıcı Ayarlar, gri/beyaz sabit kilidi, ayrı simgeler ve büyük tutar geometrisi doğrulandı.");
    }
    private static FrameworkElement PreviewSurface(Window window,WindowSizeOption size)
    {
        // Aynı üretim XAML'ini HWND/monitör kırpması olmadan çiz. İşlev testleri yukarıda gerçek pencerede yapılır.
        var document=XDocument.Load(Path.Combine(AppContext.BaseDirectory,"MainWindow.source.xaml"));
        XNamespace wpf="http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        XNamespace x="http://schemas.microsoft.com/winfx/2006/xaml";
        var root=document.Root!;root.Name=wpf+"UserControl";root.Attribute(x+"Class")!.Remove();
        foreach(var key in new[]{"Title","Icon","MinWidth","MinHeight","WindowStartupLocation","WindowStyle","ResizeMode","Style"})root.Attribute(key)?.Remove();
        root.SetAttributeValue("Foreground","{StaticResource NightTextBrush}");
        root.Element(wpf+"WindowChrome.WindowChrome")!.Remove();root.Element(wpf+"Window.Resources")!.Name=wpf+"UserControl.Resources";
        root.SetAttributeValue(XNamespace.Xmlns+"local","clr-namespace:Hakedis.App;assembly=Ek Ders Hakediş Sistemi");
        foreach(var element in root.DescendantsAndSelf())
        {
        if(element.Name.NamespaceName=="clr-namespace:Hakedis.App")element.Name=XName.Get(element.Name.LocalName,"clr-namespace:Hakedis.App;assembly=Ek Ders Hakediş Sistemi");
        foreach(var attribute in element.Attributes().ToArray())
        {
            if(attribute.Name.LocalName is "Click" or "MouseLeftButtonDown")attribute.Remove();
            else if(attribute.Value.Contains("AncestorType=Window"))attribute.Value=attribute.Value.Replace("AncestorType=Window","AncestorType=UserControl");
        }
        }
        var surface=(UserControl)XamlReader.Parse(document.ToString());surface.DataContext=window.DataContext;
        surface.Width=size.Width;surface.Height=size.Height;surface.ApplyTemplate();
        surface.Dispatcher.Invoke(()=>{},DispatcherPriority.DataBind);
        surface.Measure(new Size(size.Width,size.Height));surface.Arrange(new Rect(0,0,size.Width,size.Height));surface.UpdateLayout();
        if(Math.Abs(surface.ActualWidth-size.Width)>1)throw new Exception($"WPF yüzeyi beklenen genişlikte değil: {surface.ActualWidth}");
        return surface;
    }
    private static void Idle(Window window){window.Dispatcher.Invoke(()=>{},DispatcherPriority.ApplicationIdle);window.UpdateLayout();}
    private static IEnumerable<T> Descendants<T>(DependencyObject root) where T:DependencyObject
    {for(var i=0;i<VisualTreeHelper.GetChildrenCount(root);i++){var child=VisualTreeHelper.GetChild(root,i);if(child is T item)yield return item;foreach(var next in Descendants<T>(child))yield return next;}}
}
