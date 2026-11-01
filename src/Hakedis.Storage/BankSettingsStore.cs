using System.Text.RegularExpressions;
using Hakedis.Contracts;

namespace Hakedis.Storage;

public sealed record BankFirmSettings(string CustomerNumber="",string BranchCode="",string Account="")
{
    public BankFirmSettings Validate()
    {
        var value=this with {CustomerNumber=CustomerNumber.Trim(),BranchCode=BranchCode.Trim(),Account=Account.Replace(" ","").ToUpperInvariant()};
        if(!Regex.IsMatch(value.CustomerNumber,"^[0-9]{1,8}$"))throw new ArgumentException("Firma müşteri numarası 1–8 rakam olmalıdır. Seçilen modülün Banka Sabitleri alanını doldurun.");
        if(!Regex.IsMatch(value.BranchCode,"^[0-9]{0,4}$"))throw new ArgumentException("Firma şube kodu boş veya en fazla 4 rakam olmalıdır.");
        if(!TeacherDirectory.IsBankAccountFormatValid(value.Account) && !Regex.IsMatch(value.Account,"^[0-9]{1,8}$"))throw new ArgumentException("Firma IBAN/hesap bilgisini Banka Sabitleri alanından tamamlayın.");
        if(!value.Account.StartsWith("TR",StringComparison.Ordinal) && value.BranchCode.Length==0)throw new ArgumentException("Firma hesap numarası kullanılırken firma şube kodu gereklidir.");
        return value;
    }
}

public sealed class BankSettingsStore(string root)
{
    public string FilePath(ModuleId module)
    {
        if(!Enum.IsDefined(module))throw new ArgumentException("Banka sabiti modülü geçersiz.");
        return Path.Combine(root,"BankaSabitleri",module+".json");
    }
    public BankFirmSettings Read(ModuleId module)
    {
        var path=FilePath(module);
        return File.Exists(path)?new JsonDraftStore().Load<BankFirmSettings>(path,module,"halkbank-firma.v1").Data.Validate():new();
    }
    public void Save(ModuleId module,BankFirmSettings settings)
    {
        var normalized=settings.Validate();
        new JsonDraftStore().Save(FilePath(module),new DraftDocument<BankFirmSettings>(1,module,"halkbank-firma.v1",DateTimeOffset.UtcNow,normalized));
    }
}
