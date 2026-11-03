using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Hakedis.App;
using Hakedis.Contracts;
using Hakedis.Storage;

internal static class PayrollRevisionChecks
{
    public static void Run(string output,Action<FrameworkElement,string> capture)
    {
        var root=Path.Combine(output,"workflow");Directory.CreateDirectory(root);
        new ReportDetailsStore(root).Save(new("Deneme Kurumu","Deneme Bankası","Görevli","Şube Müdürü","Yetkili","Müdür"));
        var shell=new ShellViewModel(Path.Combine(root,"ogretmenler.json"));
        shell.Registration.Editor.FullName="Deneme Öğretmeni";shell.Registration.Editor.IdentityNumber="10000000146";
        shell.Registration.Editor.Iban="TR330006100519786457841326";shell.Registration.Editor.OccupationCode="2341.04";shell.SaveNewTeacher();
        shell.Calculation.Rows.Single().DayHours="100";shell.Calculation.CreatePayment();
        shell.Navigate("Outputs");shell.Outputs.Selected=shell.Outputs.Records.Single();
        var archived=shell.Outputs.ReadSelected();var original=File.ReadAllBytes(archived.Path);
        var events=new List<string>();string? opened=null;
        var window=new MainWindow(shell,message=>events.Add("notice"),(message,title)=>true,folder=>{events.Add("folder");opened=folder;},reportOutputRoot:Path.Combine(output,"created-pdfs"));
        window.Show();window.Dispatcher.Invoke(()=>{},DispatcherPriority.ApplicationIdle);
        var pdf=Descendants<Button>((DataGrid)window.FindName("OutputsGrid")).Single(b=>b.Content as string=="PDF");
        pdf.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));window.Dispatcher.Invoke(()=>{},DispatcherPriority.ApplicationIdle);
        if(!events.SequenceEqual(new[]{"notice","folder"}) || opened is null || Directory.GetFiles(opened,"*.pdf").Length!=3)throw new Exception("PDF bildirimi sonrası tamamlanan klasör açılmadı.");
        var detail=new PaymentDetailsWindow(archived){Owner=window};detail.Show();detail.Dispatcher.Invoke(()=>{},DispatcherPriority.ApplicationIdle);
        var card=(FrameworkElement)detail.FindName("RequestedAppropriationsCard");var texts=Descendants<TextBlock>(card).Select(t=>t.Text).ToArray();
        var model=(PaymentDetailsViewModel)detail.DataContext;
        if(!texts.Contains("01.04_Kişi ücreti") || !texts.Contains("02.04_SGK Ücreti") || !texts.Contains(model.Gross) || !texts.Contains(model.EmployerContribution) || !texts.Contains(model.Accrual))throw new Exception("Kayıtlı ödenek kartı eksik veya güncel hesaptan alınmış.");
        capture(detail,Path.Combine(output,"requested-appropriations.png"));detail.Close();
        if(!original.SequenceEqual(File.ReadAllBytes(archived.Path)))throw new Exception("Çıktı akışı kaynak hakedişi değiştirdi.");
        shell.ClearCurrentWork();window.Close();
        Console.WriteLine("PDF: başarı bildirimi ardından tam klasör açıldı. Detay: arşivden kişi ücreti/işveren SGK/toplam ödenek kartı doğrulandı; arşiv değişmedi.");
    }
    private static IEnumerable<T> Descendants<T>(DependencyObject root) where T:DependencyObject
    {for(var i=0;i<VisualTreeHelper.GetChildrenCount(root);i++){var child=VisualTreeHelper.GetChild(root,i);if(child is T value)yield return value;foreach(var nested in Descendants<T>(child))yield return nested;}}
}
