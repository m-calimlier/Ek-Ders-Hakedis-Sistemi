using System.IO;
using System.Windows;
using Hakedis.Storage;

namespace Hakedis.App;

public partial class BankExcelWindow : Window
{
    private readonly ArchivedPayment record;
    private readonly BankSettingsStore settings;
    private readonly Func<ArchivedPayment> readCurrent;
    private readonly string outputRoot;
    public string? OutputPath {get;private set;}
    public BankExcelWindow(ArchivedPayment record,string root,Func<ArchivedPayment> readCurrent,string? outputRoot=null)
    {
        InitializeComponent();this.record=record;this.readCurrent=readCurrent;settings=new(root);this.outputRoot=outputRoot??PaymentReportService.DefaultOutputRoot;
        RecordText.Text=$"{record.Module} · {record.Period} · {record.Count} kişi · {record.Net:N2} TL";
        var firm=settings.Read(record.Document.ModuleId);
        FirmText.Text=firm.CustomerNumber.Length==0?"Bu bölümün banka sabitleri henüz girilmedi.":$"Müşteri no: {firm.CustomerNumber}   Şube: {firm.BranchCode}\n{firm.Account}";
    }
    public void ExportTo(string path,DateOnly date)
    {
        var current=readCurrent();
        if(current.Path!=record.Path || current.Version!=record.Version)throw new InvalidOperationException("Hakediş kaydı değişti; pencereyi kapatıp kaydı yeniden seçin.");
        var firm=settings.Read(record.Document.ModuleId).Validate();
        HalkbankExcelService.Export(BankTemplateStore.ReadEmbedded(),current,date,path,firm);OutputPath=path;
    }
    private void Export_Click(object sender,RoutedEventArgs e)=>Try(()=>
    {
        if(PaymentDate.SelectedDate is not DateTime date)throw new ArgumentException("Bankanın ödeme yapacağı tarihi seçin.");
        _=settings.Read(record.Document.ModuleId).Validate();
        var folder=Path.Combine(outputRoot,record.Document.ModuleId.ToString(),$"{record.Document.Data.Year:D4}-{record.Document.Data.Month:D2}");
        var path=Path.Combine(folder,$"Halkbank-{record.Document.Data.Year:D4}-{record.Document.Data.Month:D2}-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}.xls");
        ExportTo(path,DateOnly.FromDateTime(date));DialogResult=true;
    });
    private void Try(Action action)
    {try{action();}catch(Exception ex) when(ex is ArgumentException or InvalidOperationException or IOException or UnauthorizedAccessException){ErrorText.Text=ex.Message;}}
}
