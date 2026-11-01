using Hakedis.Contracts;

namespace Hakedis.Storage;

public sealed class BankTemplateStore(string root)
{
    public string FilePath(ModuleId module)
    {if(!PayrollCores.IsAvailable(module))throw new ArgumentException("Banka modülü geçersiz.");return Path.Combine(root,"BankaSablonlari",module+".xls");}
    public void Import(ModuleId module,string source)
    {
        var bytes=HalkbankExcelService.ReadTemplate(source);_=HalkbankExcelService.Inspect(bytes);
        var path=FilePath(module);Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
        try{File.WriteAllBytes(temp,bytes);File.Move(temp,path,true);}finally{if(File.Exists(temp))File.Delete(temp);}
    }
}
