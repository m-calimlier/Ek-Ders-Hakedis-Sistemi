using Hakedis.App;
using Hakedis.Contracts;
using Hakedis.Storage;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace Hakedis.Tests;

public sealed class BankExcelTests : IDisposable
{
    private readonly string root=Path.Combine(Path.GetTempPath(),"HakedisBank",Guid.NewGuid().ToString("N"));

    public static byte[] Template()
    {
        using var book=new HSSFWorkbook();var sheet=book.CreateSheet("HalkbankDTO");
        var style=book.CreateCellStyle();style.DataFormat=book.CreateDataFormat().GetFormat("#,##0.00");
        for(var r=0;r<35;r++){var row=sheet.CreateRow(r);row.Height=300;for(var c=0;c<8;c++)row.CreateCell(c,CellType.Blank);}
        void Text(int r,int c,string value)=>sheet.GetRow(r).GetCell(c).SetCellValue(value);
        Text(0,0,"Ödeme/Kesinti Tarihi (YYYYAAGG)");sheet.GetRow(0).GetCell(1).SetCellValue(20261005);
        Text(1,0,"Firma Müşteri No");sheet.GetRow(1).GetCell(1).SetCellValue(12345678);
        Text(2,0,"Firma Şube Kodu");sheet.GetRow(2).GetCell(1).SetCellValue(782);
        Text(3,0,"Firma Hesap No / IBAN");Text(3,1,"TR330006100519786457841326");
        Text(4,0,"Dosya Açıklaması");Text(4,1,"OLD PERIOD DESCRIPTION");
        Text(5,1,"Ödeme");Text(0,6,"Ödeme");Text(0,7,"Kesinti");Text(6,1,"İşçi Ücret");
        Text(8,1,"OLD PERIOD");
        var headers=new[]{"Personel Şube","Personel Hesap No / IBAN","Tutar","TCKN/VKN/YKN/KKTCNO","Personel Adı Soyadı"};
        for(var c=0;c<5;c++)Text(11,c,headers[c]);
        for(var r=12;r<15;r++){Text(r,1,"TR000000000000000000000000");sheet.GetRow(r).GetCell(2).SetCellValue(999);sheet.GetRow(r).GetCell(2).CellStyle=style;sheet.GetRow(r).GetCell(3).SetCellValue(10000000999d);Text(r,4,"OLD PERSON "+r);}
        sheet.GetRow(7).GetCell(4).SetCellFormula("SUM(C13:C65536)");sheet.GetRow(8).GetCell(4).SetCellFormula("COUNTA(E13:E65536)");
        sheet.SetColumnWidth(1,8000);sheet.SetColumnWidth(4,7000);sheet.SetColumnHidden(6,true);
        var instruction=book.CreateSheet("Açıklama");instruction.CreateRow(0).CreateCell(0).SetCellValue("KEEP THIS SHEET EXACTLY");instruction.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0,0,0,1));
        using var stream=new MemoryStream();book.Write(stream,true);return stream.ToArray();
    }
    private ArchivedPayment Archive()
    {
        var shell=new ShellViewModel(Path.Combine(root,"ogretmenler.json"));
        for(var i=0;i<2;i++){
            shell.Registration.Editor.FullName="Banka Deneme "+i;shell.Registration.Editor.IdentityNumber=i==0?"10000000146":"10000000224";
            shell.Registration.Editor.Iban=i==0?"TR330006100519786457841326":"TR330006100519786457841327";
            shell.Registration.Editor.OccupationCode="2341.04";shell.SaveNewTeacher();
        }
        shell.Calculation.Year=2026;shell.Calculation.Month=9;
        for(var i=0;i<shell.Calculation.Rows.Count;i++)shell.Calculation.Rows[i].DayHours=(40+i*20).ToString();
        shell.Calculation.CreatePayment();return shell.Outputs.Records.Single();
    }
    [Fact] public void Selected_archive_populates_only_expected_cells_and_preserves_blanks_styles_and_formulas()
    {
        var template=Template();var original=template.ToArray();var record=Archive();var archiveBytes=File.ReadAllBytes(record.Path);
        var output=HalkbankExcelService.Build(template,record,new DateOnly(2026,11,5));
        using var book=new HSSFWorkbook(new MemoryStream(output));var sheet=book.GetSheetAt(0);
        Assert.Equal(2,book.NumberOfSheets);Assert.Equal("HalkbankDTO",sheet.SheetName);Assert.Equal("Açıklama",book.GetSheetAt(1).SheetName);
        Assert.Equal(20261105,sheet.GetRow(0).GetCell(1).NumericCellValue);
        Assert.Equal(record.Document.Data.Rows[0].Teacher.Iban,sheet.GetRow(12).GetCell(1).StringCellValue);
        Assert.Equal((double)record.Document.Data.Rows[0].Result.Net,sheet.GetRow(12).GetCell(2).NumericCellValue,8);
        Assert.Equal(double.Parse(record.Document.Data.Rows[0].Teacher.IdentityNumber,System.Globalization.CultureInfo.InvariantCulture),sheet.GetRow(12).GetCell(3).NumericCellValue);
        Assert.Equal(record.Document.Data.Rows[0].Teacher.FullName,sheet.GetRow(12).GetCell(4).StringCellValue);
        Assert.Equal(CellType.Blank,sheet.GetRow(12).GetCell(0).CellType);Assert.Equal(CellType.Blank,sheet.GetRow(7).GetCell(1).CellType);
        Assert.Equal(CellType.Blank,sheet.GetRow(14).GetCell(1).CellType);Assert.Equal(CellType.Blank,sheet.GetRow(14).GetCell(2).CellType);
        Assert.Equal("SUM(C13:C65536)",sheet.GetRow(7).GetCell(4).CellFormula);Assert.Equal((double)record.Net,sheet.GetRow(7).GetCell(4).NumericCellValue,8);Assert.Equal(2,sheet.GetRow(8).GetCell(4).NumericCellValue);
        Assert.Equal(12345678,sheet.GetRow(1).GetCell(1).NumericCellValue);Assert.Equal(782,sheet.GetRow(2).GetCell(1).NumericCellValue);
        Assert.Equal(8000,sheet.GetColumnWidth(1));Assert.True(sheet.IsColumnHidden(6));Assert.Equal(1,book.GetSheetAt(1).NumMergedRegions);
        Assert.Equal(original,template);Assert.Equal(archiveBytes,File.ReadAllBytes(record.Path));
        using var before=new HSSFWorkbook(new MemoryStream(template));
        for(var r=0;r<35;r++)for(var c=0;c<8;c++){
            var cell=sheet.GetRow(r).GetCell(c);var old=before.GetSheetAt(0).GetRow(r).GetCell(c);
            Assert.Equal(old.CellStyle.Index,cell.CellStyle.Index);
            if((r>=12 && c is >=1 and <=4) || (c==1 && r is 0 or 4 or 8) || (c==4 && r is 7 or 8))continue;
            Assert.Equal(old.CellType,cell.CellType);Assert.Equal(old.ToString(),cell.ToString());
        }
    }
    [Fact] public void Unused_template_recipient_names_are_not_left_in_shared_strings()
    {
        var output=HalkbankExcelService.Build(Template(),Archive(),new DateOnly(2026,11,5));
        Assert.DoesNotContain("OLD PERSON",System.Text.Encoding.Unicode.GetString(output));
        Assert.DoesNotContain("OLD PERIOD",System.Text.Encoding.Unicode.GetString(output));
        Assert.DoesNotContain("OLD PERSON",System.Text.Encoding.Latin1.GetString(output));
        Assert.DoesNotContain("OLD PERIOD",System.Text.Encoding.Latin1.GetString(output));
    }
    [Fact] public void Bank_constants_are_isolated_per_module_and_survive_restart()
    {
        var vm=new BankConstantsViewModel(root);vm.ToggleSave();vm.CustomerNumber="12345678";vm.BranchCode="782";vm.Account="TR330006100519786457841326";vm.ToggleSave();
        vm.SelectedModule=vm.Modules.Single(m=>m.Id==ModuleId.DinOgretimi);
        Assert.Empty(vm.CustomerNumber);Assert.Empty(vm.Account);
        vm.ToggleSave();vm.CustomerNumber="87654321";vm.Account="TR330006100519786457841327";vm.ToggleSave();
        var restarted=new BankConstantsViewModel(root);Assert.Equal("12345678",restarted.CustomerNumber);
        restarted.SelectedModule=restarted.Modules.Single(m=>m.Id==ModuleId.DinOgretimi);Assert.Equal("87654321",restarted.CustomerNumber);
        Assert.Throws<ArgumentException>(()=>new BankSettingsStore(root).Read(ModuleId.MeslekiEgitim).Validate());
    }
    [Fact] public void Firm_override_and_numeric_totals_preserve_template_row_heights()
    {
        using var original=new HSSFWorkbook(new MemoryStream(Template()));var source=original.GetSheetAt(0);
        source.GetRow(13).Height=480;source.GetRow(7).GetCell(4).SetCellType(CellType.Numeric);source.GetRow(8).GetCell(4).SetCellType(CellType.Numeric);
        using var buffer=new MemoryStream();original.Write(buffer,true);var record=Archive();
        var output=HalkbankExcelService.Build(buffer.ToArray(),record,new DateOnly(2026,11,5),new("87654321","","TR330006100519786457841327"));
        using var result=new HSSFWorkbook(new MemoryStream(output));var sheet=result.GetSheetAt(0);
        Assert.Equal(87654321,sheet.GetRow(1).GetCell(1).NumericCellValue);Assert.Equal(CellType.Blank,sheet.GetRow(2).GetCell(1).CellType);
        Assert.Equal("TR330006100519786457841327",sheet.GetRow(3).GetCell(1).StringCellValue);Assert.Equal(480,sheet.GetRow(13).Height);
        Assert.Equal((double)record.Net,sheet.GetRow(7).GetCell(4).NumericCellValue,8);
    }
    [Fact] public void Private_templates_are_module_specific_and_source_is_unchanged()
    {
        Directory.CreateDirectory(root);var source=Path.Combine(root,"original.xls");var bytes=Template();File.WriteAllBytes(source,bytes);
        var store=new BankTemplateStore(root);store.Import(ModuleId.TemelEgitim,source);
        Assert.Equal(bytes,File.ReadAllBytes(source));Assert.Equal(bytes,File.ReadAllBytes(store.FilePath(ModuleId.TemelEgitim)));
        Assert.False(File.Exists(store.FilePath(ModuleId.DinOgretimi)));
    }
    [Fact] public void Template_can_be_read_while_open_in_Excel()
    {
        Directory.CreateDirectory(root);var path=Path.Combine(root,"open.xls");var bytes=Template();File.WriteAllBytes(path,bytes);
        using var excelHandle=new FileStream(path,FileMode.Open,FileAccess.ReadWrite,FileShare.ReadWrite);
        Assert.Equal(bytes,HalkbankExcelService.ReadTemplate(path));
    }
    [Theory][InlineData("iban")][InlineData("identity")][InlineData("zero")][InlineData("negative")]
    public void Invalid_recipient_data_is_rejected_without_changing_the_archive(string issue)
    {
        var record=Archive();var row=record.Document.Data.Rows[0];
        row=issue switch {"iban"=>row with {Teacher=row.Teacher with {Iban=""}},"identity"=>row with {Teacher=row.Teacher with {IdentityNumber=""}},"zero"=>row with {Result=row.Result with {Net=0}},_=>row with {Result=row.Result with {Net=-1}}};
        var invalid=record with {Document=record.Document with {Data=record.Document.Data with {Rows=[row]}}};
        var bytes=File.ReadAllBytes(record.Path);
        Assert.Throws<ArgumentException>(()=>HalkbankExcelService.Build(Template(),invalid,new DateOnly(2026,11,5)));
        Assert.Equal(bytes,File.ReadAllBytes(record.Path));
    }
    [Fact] public void Wrong_template_and_attempt_to_overwrite_original_template_are_rejected()
    {
        var record=Archive();Assert.ThrowsAny<Exception>(()=>HalkbankExcelService.Build([1,2,3],record,new DateOnly(2026,11,5)));
        var path=Path.Combine(root,"template.xls");File.WriteAllBytes(path,Template());var original=File.ReadAllBytes(path);
        Assert.Throws<ArgumentException>(()=>HalkbankExcelService.Export(path,record,new DateOnly(2026,11,5),path));Assert.Equal(original,File.ReadAllBytes(path));
    }
    public void Dispose(){if(Directory.Exists(root))Directory.Delete(root,true);}
}
