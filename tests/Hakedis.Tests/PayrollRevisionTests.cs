using System.Globalization;
using System.Text.Json;
using Hakedis.App;
using Hakedis.Contracts;
using Hakedis.Storage;
using Hakedis.TemelEgitim.Core;
namespace Hakedis.Tests;

public sealed class PayrollRevisionTests : IDisposable
{
    private readonly string root=Path.Combine(Path.GetTempPath(),"PayrollRevision",Guid.NewGuid().ToString("N"));
    private static PaymentSnapshot Data()
    {
        var teacher=new TeacherRecord(Guid.NewGuid(),"Deneme Öğretmeni","10000000146","TR330006100519786457841326","Okul","2341.04",null,null);
        var input=new MonthlyInput(teacher.Id,150,10,50);
        var result=new TeacherPayrollModule().Calculate(new(150,10,50),TeacherParameters.SourceSeptember2026);
        return new(2026,9,PeriodValues.Source,[new(teacher,input,result)],"");
    }
    [Fact] public void Requested_payroll_columns_are_ordered_without_duplicate_employer_or_fixed_exemptions()
    {
        var data=Data();var original=JsonSerializer.Serialize(data);var table=PaymentReports.Build(data,new())[0].Tables.Single();
        var gross=Array.IndexOf(table.Headers,"Brüt Ücret Tutarı");Assert.True(gross>=0);
        Assert.Equal(new[]{"Brüt Ücret Tutarı","SGK İşveren Devlet Katkısı (% 21,75)","Tahakkuk Toplamı","SGK Kişi Primi (% 14)","Aylık Gelir Vergisi Matrahı"},table.Headers.Skip(gross).Take(5));
        Assert.Single(table.Headers,h=>h.Contains("SGK İşveren"));
        Assert.DoesNotContain(ExcelHeadings.BordroJ5,table.Headers);Assert.DoesNotContain(ExcelHeadings.BordroM5,table.Headers);
        Assert.DoesNotContain(ExcelHeadings.BordroS6,table.Headers);Assert.DoesNotContain(ExcelHeadings.BordroU6,table.Headers);
        Assert.Contains("Damga Vergisi İstisna Kalan",table.Headers);Assert.Contains("Damga Vergisi Tutarı (% 0,759)",table.Headers);Assert.Contains("Kesintiler Toplamı",table.Headers);
        Assert.Equal(original,JsonSerializer.Serialize(data));
    }
    [Theory][InlineData(-1,1)][InlineData(0,0)][InlineData(1,0)]
    public void Stamp_remaining_exemption_has_zero_floor_and_does_not_change_results(int delta,int expected)
    {
        var data=Data();data=data with {Rows=[data.Rows[0] with {Result=data.Rows[0].Result with {Gross=data.Constants.StampExemption+delta}}]};
        var original=JsonSerializer.Serialize(data);var table=PaymentReports.Build(data,new())[0].Tables.Single();
        var column=Array.IndexOf(table.Headers,"Damga Vergisi İstisna Kalan");Assert.True(column>=0);
        Assert.Equal(expected.ToString("N2",CultureInfo.GetCultureInfo("tr-TR")),table.Rows[0][column]);Assert.Equal(table.Rows[0][column],table.Total![column]);
        Assert.Equal(original,JsonSerializer.Serialize(data));
    }
    [Fact] public void Exemptions_are_shared_with_retired_module_and_reloaded_live_without_changing_archive()
    {
        var data=Data();var original=JsonSerializer.Serialize(data);var shell=new ShellViewModel(Path.Combine(root,"ogretmenler.json"));
        shell.CommonConstants.ToggleSave();shell.CommonConstants.Editor.IncomeExemption="29000";shell.CommonConstants.Editor.StampExemption="34000";shell.CommonConstants.ToggleSave();
        foreach(var module in PayrollCores.Catalog.Where(m=>m.IsAvailable))
        {var values=new CommonConstantsStore(root,module.Id).Read();Assert.Equal(29000,values.IncomeExemption);Assert.Equal(34000,values.StampExemption);}
        Assert.Equal(29000,shell.RetiredConstants.Editor.Read().IncomeExemption);Assert.Equal(34000,shell.RetiredConstants.Editor.Read().StampExemption);
        Assert.Equal(.075m,new CommonConstantsStore(root,ModuleId.HalkEgitimEmekliUstaOgretici).Read().EmployeeRate);
        Assert.Equal(original,JsonSerializer.Serialize(data));
    }
    public void Dispose(){if(Directory.Exists(root))Directory.Delete(root,true);}
}
