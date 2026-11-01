using System.ComponentModel;
using Hakedis.Contracts;
using Hakedis.Storage;

namespace Hakedis.App;

public sealed class BankConstantsViewModel : INotifyPropertyChanged
{
    private readonly BankSettingsStore store;
    public IReadOnlyList<ModuleDescriptor> Modules {get;}=PayrollCores.Catalog.Where(m=>m.IsAvailable).ToArray();
    private ModuleDescriptor selectedModule;
    public ModuleDescriptor SelectedModule
    {
        get=>selectedModule;
        set {if(value is null || IsEditing || value.Id==selectedModule.Id)return;selectedModule=value;Load();Notify();}
    }
    public string CustomerNumber {get;set;}="";
    public string BranchCode {get;set;}="";
    public string Account {get;set;}="";
    public bool IsEditing {get;private set;}
    public bool IsReadOnly=>!IsEditing;
    public string ButtonText=>IsEditing?"Kaydet":"Güncelle";
    public string Status {get;private set;}="Her bölümün firma banka bilgisi ayrı saklanır.";
    public event PropertyChangedEventHandler? PropertyChanged;
    public BankConstantsViewModel(string root)
    {store=new(root);selectedModule=Modules.Single(m=>m.Id==ModuleId.TemelEgitim);Load();}
    private void Load()
    {var value=store.Read(selectedModule.Id);CustomerNumber=value.CustomerNumber;BranchCode=value.BranchCode;Account=value.Account;}
    public void ToggleSave()
    {
        if(IsEditing){store.Save(selectedModule.Id,new(CustomerNumber,BranchCode,Account));Load();IsEditing=false;Status=selectedModule.DisplayName+" banka sabitleri kaydedildi.";}
        else {IsEditing=true;Status="Seçilen bölümün firma bilgilerini düzenleyip Kaydet düğmesine basın.";}
        Notify();
    }
    public void ShowError(string message){Status=message;Notify();}
    private void Notify()
    {foreach(var name in new[]{nameof(SelectedModule),nameof(CustomerNumber),nameof(BranchCode),nameof(Account),nameof(IsEditing),nameof(IsReadOnly),nameof(ButtonText),nameof(Status)})PropertyChanged?.Invoke(this,new(name));}
}
