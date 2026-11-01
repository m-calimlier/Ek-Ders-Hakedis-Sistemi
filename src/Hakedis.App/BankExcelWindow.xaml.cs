using System.IO;
using System.Windows;
using Hakedis.Storage;
using Microsoft.Win32;

namespace Hakedis.App;

public partial class BankExcelWindow : Window
{
    private readonly ArchivedPayment record;
    private readonly BankTemplateStore templates;
    private readonly BankSettingsStore settings;
    private readonly Func<ArchivedPayment> readCurrent;
    public string? OutputPath {get;private set;}
    public BankExcelWindow(ArchivedPayment record,string root,Func<ArchivedPayment> readCurrent)
    {
        InitializeComponent();this.record=record;this.readCurrent=readCurrent;templates=new(root);settings=new(root);
        RecordText.Text=$"{record.Module} · {record.Period} · {record.Count} kişi · {record.Net:N2} TL";
        var firm=settings.Read(record.Document.ModuleId);
        FirmText.Text=firm.CustomerNumber.Length==0?"Bu bölümün banka sabitleri henüz girilmedi.":$"Müşteri no: {firm.CustomerNumber}   Şube: {firm.BranchCode}\n{firm.Account}";
        RefreshTemplate();
    }
    private void RefreshTemplate()
    {var path=templates.FilePath(record.Document.ModuleId);TemplateText.Text=File.Exists(path)?path:"Şablon seçilmedi";}
    private void Template_Click(object sender,RoutedEventArgs e)
    {
        var dialog=new OpenFileDialog{Title="Bu bölümün Halkbank şablonunu seçin",Filter="Excel 97–2003 (*.xls)|*.xls",CheckFileExists=true};
        if(dialog.ShowDialog(this)!=true)return;
        Try(()=>{templates.Import(record.Document.ModuleId,dialog.FileName);RefreshTemplate();ErrorText.Text="Şablon bu bölüm için kaydedildi.";});
    }
    public void ExportTo(string path,DateOnly date)
    {
        var current=readCurrent();
        if(current.Path!=record.Path || current.Version!=record.Version)throw new InvalidOperationException("Hakediş kaydı değişti; pencereyi kapatıp kaydı yeniden seçin.");
        var firm=settings.Read(record.Document.ModuleId).Validate();
        HalkbankExcelService.Export(templates.FilePath(record.Document.ModuleId),current,date,path,firm);OutputPath=path;
    }
    private void Export_Click(object sender,RoutedEventArgs e)=>Try(()=>
    {
        if(PaymentDate.SelectedDate is not DateTime date)throw new ArgumentException("Bankanın ödeme yapacağı tarihi seçin.");
        _=settings.Read(record.Document.ModuleId).Validate();
        if(!File.Exists(templates.FilePath(record.Document.ModuleId)))throw new ArgumentException("Bu bölüm için orijinal Halkbank .xls şablonunu seçin.");
        var dialog=new SaveFileDialog{Title="Banka ödeme dosyasını kaydet",Filter="Excel 97–2003 (*.xls)|*.xls",DefaultExt=".xls",AddExtension=true,OverwritePrompt=true,FileName=$"Halkbank_{record.Document.ModuleId}_{record.Document.Data.Year}-{record.Document.Data.Month:00}.xls"};
        if(dialog.ShowDialog(this)!=true)return;ExportTo(dialog.FileName,DateOnly.FromDateTime(date));DialogResult=true;
    });
    private void Try(Action action)
    {try{action();}catch(Exception ex) when(ex is ArgumentException or InvalidOperationException or IOException or UnauthorizedAccessException){ErrorText.Text=ex.Message;}}
}
