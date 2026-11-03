using Hakedis.Contracts;
using Hakedis.TemelEgitim.Core;
namespace Hakedis.Tests;
public sealed class SingleRowPayrollReportTests
{
    private static PaymentSnapshot Data(decimal dyk=0,decimal icra=0)
    {
        var p=PeriodValues.Source;
        PaymentRow Row(decimal day,decimal course,decimal garnishment)
        {
            var teacher=new TeacherRecord(Guid.NewGuid(),"Deneme Öğretmeni","10000000146","TR330006100519786457841326","Okul","2341.04",null,null);
            var result=new TeacherPayrollModule().Calculate(new(day,course,garnishment),TeacherParameters.SourceSeptember2026);
            return new(teacher,new(teacher.Id,day,course,garnishment),result);
        }
        return new(2026,10,p,[Row(100,0,0),Row(160,dyk,icra)],"");
    }
    [Theory][InlineData(0,0,false,false)][InlineData(1,0,true,false)][InlineData(0,1,false,true)][InlineData(1,1,true,true)]
    public void One_table_hides_only_unused_optional_columns(int dyk,int icra,bool hasDyk,bool hasIcra)
    {
        var data=Data(dyk,icra);var report=PaymentReports.Build(data,new("Kurum","Banka","Görevli","Ünvan","Yetkili"))[0];
        var table=Assert.Single(report.Tables);Assert.Equal(2,table.Rows.Length);Assert.Equal(2,report.Information.Length);
        Assert.Equal(hasDyk,table.Headers.Contains(ExcelHeadings.BordroG5));Assert.Equal(hasDyk,table.Headers.Contains(ExcelHeadings.BordroL5));Assert.Equal(hasDyk,table.Headers.Contains(ExcelHeadings.BordroI5));
        Assert.DoesNotContain(ExcelHeadings.BordroM5,table.Headers);Assert.Equal(hasDyk,table.Headers.Contains(ExcelHeadings.BordroF5));Assert.Contains(ExcelHeadings.BordroQ5,table.Headers);
        Assert.Equal(hasIcra,table.Headers.Contains(ExcelHeadings.BordroY6));Assert.DoesNotContain(ExcelHeadings.BordroR5,table.Headers);
        Assert.Contains(ExcelHeadings.BordroN6,table.Headers);
        var excess=Array.IndexOf(table.Headers,"Kalan Gelir Vergisi İstisnası");Assert.True(excess>=0);
        Assert.Equal(Math.Max(0,data.Constants.IncomeExemption-data.Rows[0].Result.IncomeBase).ToString("N2",System.Globalization.CultureInfo.GetCultureInfo("tr-TR")),table.Rows[0][excess]);Assert.Equal(Math.Max(0,data.Constants.IncomeExemption-data.Rows[1].Result.IncomeBase).ToString("N2",System.Globalization.CultureInfo.GetCultureInfo("tr-TR")),table.Rows[1][excess]);
        Assert.Equal(data.Rows.Sum(r=>Math.Max(0,data.Constants.IncomeExemption-r.Result.IncomeBase)).ToString("N2",System.Globalization.CultureInfo.GetCultureInfo("tr-TR")),table.Total![excess]);
        var income=Array.IndexOf(table.Headers,ExcelHeadings.BordroQ5);Assert.Equal(data.Rows[0].Result.IncomeBase.ToString("N2",System.Globalization.CultureInfo.GetCultureInfo("tr-TR")),table.Rows[0][income]);
        Assert.All(table.Rows,r=>Assert.Equal(table.Headers.Length,r.Length));Assert.Equal(table.Headers.Length,table.Widths.Length);Assert.Equal(table.Headers.Length,table.Total.Length);
        var net=Array.IndexOf(table.Headers,ExcelHeadings.BordroAA5);Assert.Equal(data.Rows.Sum(r=>r.Result.Net).ToString("N2",System.Globalization.CultureInfo.GetCultureInfo("tr-TR")),table.Total[net]);
    }
    [Theory][InlineData(-1,1)][InlineData(0,0)][InlineData(1,0)]
    public void Remaining_exemption_is_nonnegative_below_at_and_above_threshold_without_changing_archived_financial_results(int delta,int expected)
    {
        var data=Data();var original=data.Rows[0];data=data with {Rows=[original with {Result=original.Result with {IncomeBase=data.Constants.IncomeExemption+delta}}]};
        var table=Assert.Single(PaymentReports.Build(data,new())[0].Tables);
        Assert.Equal(expected.ToString("N2",System.Globalization.CultureInfo.GetCultureInfo("tr-TR")),table.Rows[0][Array.IndexOf(table.Headers,"Kalan Gelir Vergisi İstisnası")]);Assert.Equal(original.Result.Net,data.Rows[0].Result.Net);
    }
}
