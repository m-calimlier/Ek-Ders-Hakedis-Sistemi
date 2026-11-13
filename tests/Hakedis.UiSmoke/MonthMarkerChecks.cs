using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Hakedis.App;
using Hakedis.Contracts;
using Hakedis.Storage;

internal static class MonthMarkerChecks
{
    public static void Run(string output,Action<FrameworkElement,string> capture)
    {
        CheckColors(output,capture);CheckSchoolHours(output,capture);
    }
    private static void CheckColors(string output,Action<FrameworkElement,string> capture)
    {
        var path=Path.Combine(output,"colors","ogretmenler.json");var directory=new TeacherDirectory(path);
        directory.Save(new(Guid.NewGuid(),"Eylül Başlayan","10000000146","TR330006100519786457841326","Okul","2341.04",new(2020,9,5),null));
        directory.Save(new(Guid.NewGuid(),"Ağustos Başlayan","10000000224","TR330006100519786457841327","Okul","2341.04",new(2026,8,5),null));
        var shell=new ShellViewModel(path);var other=shell.Calculation.Rows.Single(r=>r.StartDate!.Value.Month==8);other.DayHours="20";shell.Calculation.SaveHybridChanges();
        shell.Navigate("Calculation");var window=new MainWindow(shell,_=>{},(_,_)=>true);window.Show();Idle(window);
        var grid=(DataGrid)window.FindName("HybridGrid");grid.SelectedItem=null;Idle(window);
        if(grid.Columns.Any(c=>c.Header as string==ExcelHeadings.IbanCheck))throw new Exception("IBAN kontrol sütunu kaldırılmadı.");
        DataGridRow Container(PayrollRow row){grid.ScrollIntoView(row);Idle(window);return (DataGridRow)grid.ItemContainerGenerator.ContainerFromItem(row);}
        var starter=shell.Calculation.Rows.Single(r=>r.StartDate!.Value.Month==9);var colored=Container(starter);
        if(starter.HasInput || ((SolidColorBrush)colored.Background).Color!=Color.FromRgb(23,71,55))throw new Exception("Eski yılda Eylül başlangıcı, ders girişi olmadan Eylül'de yeşil olmalı.");
        if(((SolidColorBrush)Container(other).Background).Color==Color.FromRgb(23,71,55))throw new Exception("Saat girilmiş olması satırı yeşil yapmamalı.");
        foreach(var entry in new[]{starter,other})
        {
            foreach(var selected in new[]{false,true})
            {
                grid.SelectedItem=selected?entry:null;Idle(window);var row=Container(entry);
                var cells=Descendants<DataGridCell>(row).Where(c=>c.IsVisible).ToArray();
                if(cells.Length==0 || cells.Any(c=>MidnightChecks.Contrast(c.Foreground,row.Background)<4.5))throw new Exception("Hibrit tablo hücre yazısı normal/yeşil/seçili satırda okunaklı değil.");
            }
        }
        grid.SelectedItem=null;Idle(window);
        capture(window,Path.Combine(output,"september-starter-green.png"));
        shell.Calculation.Month=10;Idle(window);
        if(shell.Calculation.Rows.Any(r=>r.StartedInSelectedMonth) || shell.Calculation.Rows.Any(r=>((SolidColorBrush)Container(r).Background).Color==Color.FromRgb(23,71,55)))throw new Exception("Ekim'e geçince Eylül başlangıcının rengi normale dönmedi.");
        capture(window,Path.Combine(output,"october-starter-normal.png"));
        shell.ClearCurrentWork();window.Close();
        Console.WriteLine("Satır rengi: yalnız işe başlama ayı; farklı yıl ve saat girişinden bağımsız; sonraki ay normal. IBAN kontrol sütunu yok.");
    }
    private static void CheckSchoolHours(string output,Action<FrameworkElement,string> capture)
    {
        var path=Path.Combine(output,"school-hours","ogretmenler.json");
        new TeacherDirectory(path).Save(new(Guid.NewGuid(),"Kurum Geçişi Denemesi","10000000146","TR330006100519786457841326","İlk Okul","2341.04",new(2026,9,1),null,ModuleId.TemelEgitim,
            new(ModuleId.TemelEgitim,"İlk Okul",[new(new(2026,9,15),ModuleId.TemelEgitim,"İlk Okul",ModuleId.DinOgretimi,"Yeni Okul")],[])));
        var shell=new ShellViewModel(path);shell.Monthly.EntryModule=shell.Monthly.SelectedModule;shell.Monthly.EntryTeacher=shell.Monthly.EntryTeachers.Single();
        shell.Calculation.SelectedModule=shell.Calculation.Modules.Single(m=>m.Id==ModuleId.DinOgretimi);
        var window=new MainWindow(shell,_=>{},(_,_)=>true);window.Show();Idle(window);
        TextBox Entry(string property)=>Descendants<TextBox>(window).Single(t=>t.IsVisible && t.GetBindingExpression(TextBox.TextProperty)?.ParentBinding.Path?.Path==property);
        Entry("PreviousSchoolDayHours").Text="40";Entry("NewSchoolDayHours").Text="35";Idle(window);
        shell.Monthly.Save(false);shell.Navigate("Calculation");Idle(window);
        var row=shell.Calculation.Rows.Single();var grid=(DataGrid)window.FindName("HybridGrid");
        foreach(var (heading,expected) in new[]{("ÖNCEKİ KURUM\nGÜNDÜZ","40"),("YENİ KURUM\nGÜNDÜZ","35")})
        {
            var column=grid.Columns.Single(c=>c.Header as string==heading);grid.ScrollIntoView(row,column);Idle(window);
            var content=column.GetCellContent(row)??throw new Exception("Kurum saati hücresi oluşmadı.");var input=content as TextBox??Descendants<TextBox>(content).Single();
            if(!input.IsVisible || input.Text!=expected)throw new Exception(heading+": kurum saati hibritte boş veya yanlış.");
        }
        if(row.Read().DayHours!=75 || row.Teacher.ModuleId!=ModuleId.DinOgretimi)throw new Exception("Kurum toplamı veya modül izdüşümü bozuldu.");
        capture(window,Path.Combine(output,"both-institution-hours.png"));shell.ClearCurrentWork();window.Close();
        Console.WriteLine("Kurum saatleri: giriş ve hesaplama modülleri farklıyken 40 + 35 = 75; iki değer hücrede görünür, ödeme modülü korunur.");
    }
    private static void Idle(Window window){window.Dispatcher.Invoke(()=>{},DispatcherPriority.ApplicationIdle);window.UpdateLayout();}
    private static IEnumerable<T> Descendants<T>(DependencyObject root) where T:DependencyObject
    {for(var i=0;i<VisualTreeHelper.GetChildrenCount(root);i++){var child=VisualTreeHelper.GetChild(root,i);if(child is T value)yield return value;foreach(var next in Descendants<T>(child))yield return next;}}
}
