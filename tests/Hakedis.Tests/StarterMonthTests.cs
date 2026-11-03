using Hakedis.App;
using Hakedis.Contracts;
using Hakedis.TemelEgitim.Core;
namespace Hakedis.Tests;

public sealed class StarterMonthTests
{
    [Theory][InlineData(2019,9,true)][InlineData(2026,9,true)][InlineData(2027,9,true)][InlineData(2026,8,false)][InlineData(2026,10,false)]
    public void Marker_compares_only_start_month_independent_of_year_or_hours(int year,int month,bool expected)
    {
        var teacher=new TeacherRecord(Guid.NewGuid(),"Deneme Öğretmeni","10000000146","TR330006100519786457841326","Okul","2341.04",new DateTime(year,month,1),null);
        var row=new PayrollRow(teacher,null,()=>TeacherParameters.SourceSeptember2026,period:()=> (2026,9));
        Assert.Equal(expected,row.StartedInSelectedMonth);Assert.False(row.HasInput);
        row.DayHours="20";Assert.Equal(expected,row.StartedInSelectedMonth);
    }
    [Fact] public void Marker_clears_in_next_month_and_responds_to_start_date_changes()
    {
        var month=9;var teacher=new TeacherRecord(Guid.NewGuid(),"Deneme Öğretmeni","10000000146","TR330006100519786457841326","Okul","2341.04",new DateTime(2020,9,5),null);
        var row=new PayrollRow(teacher,null,()=>TeacherParameters.SourceSeptember2026,period:()=> (2026,month));
        Assert.True(row.StartedInSelectedMonth);month=10;row.Recalculate();Assert.False(row.StartedInSelectedMonth);
        var notified=false;row.PropertyChanged+=(_,e)=>notified|=e.PropertyName==nameof(PayrollRow.StartedInSelectedMonth);
        row.StartDate=new DateTime(2018,10,1);Assert.True(row.StartedInSelectedMonth);Assert.True(notified);
        row.StartDate=null;Assert.False(row.StartedInSelectedMonth);
    }
}
