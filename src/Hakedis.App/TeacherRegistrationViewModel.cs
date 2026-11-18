using System.ComponentModel;
using Hakedis.Contracts;
using Hakedis.Storage;

namespace Hakedis.App;

// Yeni kayıt formu yalnızca Veri Girişi ekranına bağlanır.
public sealed class TeacherRegistrationViewModel : INotifyPropertyChanged
{
    private TeacherDirectory directory;
    private readonly string path;
    private TeacherRecord? original;
    public bool IsEditingRecord=>original is not null;
    public bool CanSwitchModule=>original?.Assignments is null;
    public bool HasDraft=>Editor.HasSchoolTransfer || new[]{Editor.FullName,Editor.IdentityNumber,Editor.Iban,Editor.School,Editor.OccupationCode}.Any(s=>!string.IsNullOrWhiteSpace(s)) || Editor.StartDate.HasValue || Editor.EndDate.HasValue;
    public string SaveButtonText=>IsEditingRecord?"Öğretmen Değişikliklerini Kaydet":"Öğretmeni Kaydet";
    public string CancelButtonText=>IsEditingRecord?"Vazgeç":"Formu Temizle";
    public void BeginEdit(TeacherRecord record,TeacherRecord source)
    {
        SwitchModule(record.ModuleId);original=source;Editor=TeacherEditor.From(record);ShowStatus("Öğretmen bilgilerini düzenleyin; Kaydet veya Vazgeç düğmesini kullanın.");
    }
    public TeacherRecord SaveEdit()
    {
        var expected=original??throw new InvalidOperationException("Düzenlenecek öğretmen seçilmedi.");
        var replacement=Editor.ToRecord(Module);
        var saved=expected.ModuleId==Module
            ?directory.UpdateBatch([expected],[replacement]).Single(t=>t.Id==expected.Id)
            :directory.MoveModule(expected,replacement);
        directory=new(path,Module);
        Reset();return saved;
    }
    public ModuleId Module { get; private set; }
    public TeacherEditor Editor { get; private set; } = new();
    public string Status { get; private set; } = "Öğretmen bilgilerini doldurun ve Kaydet düğmesine basın.";
    public event PropertyChangedEventHandler? PropertyChanged;
    public TeacherRegistrationViewModel(string path, ModuleId module = ModuleId.TemelEgitim)
    { this.path = path; Module = module; directory = new(path, module);Editor=new(module); }
    public void SwitchModule(ModuleId module)
    {
        if(!CanSwitchModule)throw new InvalidOperationException("Görev geçmişi olan kayıtlar için tarihli okul/modül değişikliğini kullanın.");
        Module=module;
        if(IsEditingRecord)
        {
            ShowStatus("Kaydedildiğinde öğretmen seçili modüle taşınır. Kayıtlı hakedişler kendi modülünde kalır.");
            return;
        }
        directory=new(path,module);Editor=new(module);ShowStatus("Öğretmen kaydı seçili modüle yapılacaktır.");
    }
    public TeacherRecord SaveNew()
    {
        if(IsEditingRecord)throw new InvalidOperationException("Düzenleme sırasında yeni kayıt oluşturulamaz.");
        var record = Editor.ToRecord(Module) with { Id = Guid.NewGuid() };
        var rows = directory.Save(record); var saved = rows.Single(r => r.Id == record.Id);
        Editor = new(Module); ShowStatus($"{saved.FullName} öğretmen listesine kaydedildi."); return saved;
    }
    public void Reset() { original=null;directory=new(path,Module);Editor = new(Module); ShowStatus("Yeni öğretmen kaydı için bilgiler temizlendi."); }
    public void ShowStatus(string message)
    {
        Status = message; PropertyChanged?.Invoke(this, new(nameof(Editor))); PropertyChanged?.Invoke(this, new(nameof(Status)));
        foreach(var name in new[]{nameof(IsEditingRecord),nameof(CanSwitchModule),nameof(SaveButtonText),nameof(CancelButtonText)})PropertyChanged?.Invoke(this,new(name));
    }
}
