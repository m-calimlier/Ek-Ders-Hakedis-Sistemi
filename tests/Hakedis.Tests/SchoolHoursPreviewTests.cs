using Hakedis.App;
using Hakedis.Contracts;
using Hakedis.Storage;
namespace Hakedis.Tests;
public sealed class SchoolHoursPreviewTests : IDisposable
{
    private readonly string root=Path.Combine(Path.GetTempPath(),"SchoolHoursPreview",Guid.NewGuid().ToString("N"));
    private ShellViewModel Setup()
    {
        var path=Path.Combine(root,"ogretmenler.json");
        var teacher=new TeacherRecord(Guid.NewGuid(),"Kurum Geçişi Denemesi","10000000146","TR330006100519786457841326","İlk Okul","2341.04",new(2026,9,1),null,ModuleId.TemelEgitim,
            new(ModuleId.TemelEgitim,"İlk Okul",[new(new(2026,9,15),ModuleId.TemelEgitim,"İlk Okul",ModuleId.DinOgretimi,"Yeni Okul")],[]));
        new TeacherDirectory(path).Save(teacher);var shell=new ShellViewModel(path);
        shell.Monthly.EntryModule=shell.Monthly.SelectedModule;shell.Monthly.EntryTeacher=shell.Monthly.EntryTeachers.Single();
        shell.Calculation.SelectedModule=shell.Calculation.Modules.Single(m=>m.Id==ModuleId.DinOgretimi);
        return shell;
    }
    [Fact] public void Institution_hours_reach_other_eligible_module_without_replacing_its_teacher_projection()
    {
        var shell=Setup();shell.Monthly.PreviousSchoolDayHours="40";shell.Monthly.NewSchoolDayHours="35";
        var target=shell.Calculation.Rows.Single();Assert.Equal("40",target.PreviousSchoolDayHours);Assert.Equal("35",target.NewSchoolDayHours);
        Assert.Equal(75,target.Read().DayHours);Assert.Equal(ModuleId.DinOgretimi,target.Teacher.ModuleId);Assert.Null(target.SavedInput);
        shell.Monthly.Save(false);Assert.Equal("35",target.NewSchoolDayHours);Assert.False(target.HasChanges);
        Assert.Empty(new MonthlyPayrollStore(root).Read(ModuleId.DinOgretimi,2026,9).Entries);
        Assert.Equal(35,new MonthlyPayrollStore(root).Read(ModuleId.TemelEgitim,2026,9).Entries.Single().NewSchoolDayHours);
    }
    [Fact] public void Institution_hours_do_not_leak_to_another_month()
    {
        var shell=Setup();shell.Calculation.Month=10;shell.Monthly.PreviousSchoolDayHours="40";shell.Monthly.NewSchoolDayHours="35";
        Assert.Empty(shell.Calculation.Rows.Single().NewSchoolDayHours);Assert.False(shell.Calculation.Rows.Single().HasInput);
    }
    [Fact] public void Existing_destination_input_is_not_marked_saved_by_a_source_module_save()
    {
        var shell=Setup();var teacher=shell.Calculation.Rows.Single().Teacher;
        new MonthlyPayrollStore(root).Save(ModuleId.DinOgretimi,new(2026,9,PeriodValues.Source,[new(teacher.Id,70,0,0,40,30,true)]));
        shell.Calculation.Reload();var target=shell.Calculation.Rows.Single();
        target.DykHours="5";target.Garnishment="20";
        shell.Monthly.PreviousSchoolDayHours="40";shell.Monthly.NewSchoolDayHours="35";shell.Monthly.Save(false);
        Assert.Equal("35",target.NewSchoolDayHours);Assert.True(target.HasChanges);
        Assert.Equal(5,target.Read().DykHours);Assert.Equal(20,target.Read().Garnishment);
        Assert.Equal(70,target.SavedInput!.DayHours);
        Assert.Equal(70,new MonthlyPayrollStore(root).Read(ModuleId.DinOgretimi,2026,9).Entries.Single().DayHours);
    }
    public void Dispose(){if(Directory.Exists(root))Directory.Delete(root,true);}
}
