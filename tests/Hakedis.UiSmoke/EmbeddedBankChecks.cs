using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Hakedis.App;
using Hakedis.Storage;

internal static class EmbeddedBankChecks
{
    public static void Run(string output,Action<FrameworkElement,string> capture)
    {
        var root=Path.Combine(output,"fresh-install");Directory.CreateDirectory(root);
        var shell=new ShellViewModel(Path.Combine(root,"ogretmenler.json"));
        shell.Registration.Editor.FullName="Banka Deneme";shell.Registration.Editor.IdentityNumber="10000000146";
        shell.Registration.Editor.Iban="TR330006100519786457841326";shell.Registration.Editor.OccupationCode="2341.04";shell.SaveNewTeacher();
        shell.Calculation.Rows.Single().DayHours="100";shell.Calculation.CreatePayment();
        shell.Navigate("Outputs");shell.Outputs.Selected=shell.Outputs.Records.Single();
        var record=shell.Outputs.ReadSelected();var original=File.ReadAllBytes(record.Path);
        new BankSettingsStore(root).Save(record.Document.ModuleId,new("87654321","782","TR330006100519786457841327"));
        var events=new List<string>();string? opened=null;Exception? failure=null;
        var window=new MainWindow(shell,_=>events.Add("notice"),(_,_)=>true,folder=>{events.Add("folder");opened=folder;},reportOutputRoot:Path.Combine(output,"created-bank"));
        window.Show();window.Dispatcher.Invoke(()=>{},DispatcherPriority.ApplicationIdle);
        var timer=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(100)};
        timer.Tick+=(_,_)=>{
            var dialog=Application.Current.Windows.OfType<BankExcelWindow>().FirstOrDefault();if(dialog is null)return;timer.Stop();
            try{
                if(Descendants<Button>(dialog).Any(b=>b.Content as string=="Şablon seç"))throw new Exception("Şablon seçimi kaldırılmadı.");
                var button=Descendants<Button>(dialog).Single(b=>b.Content as string=="Banka Excel oluştur");
                button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                if(string.IsNullOrWhiteSpace(((TextBlock)dialog.FindName("ErrorText")).Text) || dialog.OutputPath is not null)throw new Exception("Ödeme tarihi zorunlu olmalı.");
                ((DatePicker)dialog.FindName("PaymentDate")).SelectedDate=new DateTime(2026,11,5);
                dialog.Dispatcher.Invoke(()=>{},DispatcherPriority.ApplicationIdle);
                capture(dialog,Path.Combine(output,"embedded-bank.png"));
                button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                if(dialog.OutputPath is null)throw new Exception(((TextBlock)dialog.FindName("ErrorText")).Text);
            }catch(Exception ex){failure=ex;dialog.Close();}
        };
        timer.Start();
        Descendants<Button>((DataGrid)window.FindName("OutputsGrid")).Single(b=>b.Content as string=="Banka").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        if(failure is not null)throw failure;
        if(!events.SequenceEqual(new[]{"notice","folder"}) || opened is null || Directory.GetFiles(opened,"*.xls").Length!=1)throw new Exception("Banka dosyası kaydedilip klasörü açılmadı.");
        if(Directory.Exists(Path.Combine(root,"BankaSablonlari")) || !original.SequenceEqual(File.ReadAllBytes(record.Path)))throw new Exception("Şablon dosyası gerekti veya arşiv değişti.");
        shell.ClearCurrentWork();window.Close();
        Console.WriteLine("Banka: temiz kurulumda gömülü şablon, zorunlu tarih, otomatik kayıt ve bildirimden sonra klasör açılması doğrulandı; arşiv korundu.");
    }
    private static IEnumerable<T> Descendants<T>(DependencyObject root) where T:DependencyObject
    {for(var i=0;i<VisualTreeHelper.GetChildrenCount(root);i++){var child=VisualTreeHelper.GetChild(root,i);if(child is T value)yield return value;foreach(var nested in Descendants<T>(child))yield return nested;}}
}
