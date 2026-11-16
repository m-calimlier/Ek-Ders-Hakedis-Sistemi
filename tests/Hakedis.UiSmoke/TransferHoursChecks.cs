using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Hakedis.App;
using Hakedis.Contracts;
using Hakedis.Storage;
internal static class TransferHoursChecks
{
    internal static void Run(string output,Action<FrameworkElement,string> capture)
    {
        var root=Path.Combine(output,"split-data");var path=Path.Combine(root,"ogretmenler.json");
        var t=new TeacherRecord(Guid.NewGuid(),"Kurum Saat Denemesi","10000000146","TR330006100519786457841326","İlk Okul","2341.04",new(2026,9,1),null,ModuleId.TemelEgitim,
            new(ModuleId.TemelEgitim,"İlk Okul",[new(new(2026,9,15),ModuleId.TemelEgitim,"İlk Okul",ModuleId.DinOgretimi,"Yeni Okul")],[]));
        new TeacherDirectory(path).Save(t);var model=new ShellViewModel(path);model.Monthly.EntryModule=model.Monthly.SelectedModule;model.Monthly.EntryTeacher=model.Monthly.EntryTeachers.Single();
        var window=new MainWindow(model,_=>{},(_,_)=>true){ShowActivated=false,ShowInTaskbar=false};window.Show();Idle(window);
        TextBox Input(string property)=>Descendants<TextBox>(window).Single(b=>b.IsVisible && b.GetBindingExpression(TextBox.TextProperty)?.ParentBinding.Path.Path==property);
        if(Descendants<TextBox>(window).Any(b=>b.IsVisible && b.GetBindingExpression(TextBox.TextProperty)?.ParentBinding.Path.Path=="EntryDayHours"))throw new Exception("Geçiş ayında tek gündüz alanı gizlenmedi.");
        Input("PreviousSchoolDayHours").Text="40";Idle(window);
        if(model.Calculation.Rows.Single().Read().DayHours!=40 || model.Calculation.Rows.Single().Read().NewSchoolDayHours is not null)throw new Exception("Boş taraf sıfır/uyarı ya da canlı paylaşım çalışmadı.");
        Input("NewSchoolDayHours").Text="35";Idle(window);
        if(model.Calculation.Rows.Single().Read().DayHours!=75)throw new Exception("İki kurum toplamı güncellenmedi.");
        capture(window,Path.Combine(output,"Transfer-hours-entry.png"));
        Input("NewSchoolDayHours").Text="";Idle(window);model.Monthly.Save(false);model.Navigate("Calculation");Idle(window);
        var grid=(DataGrid)window.FindName("HybridGrid");
        var row=model.Calculation.Rows.Single();
        var column=grid.Columns.Single(c=>c.Header as string=="YENİ KURUM\nGÜNDÜZ");
        grid.ScrollIntoView(row,column);Idle(window);
        var editor=Descendants<TextBox>(column.GetCellContent(row)).Single();
        var zero=(TextBlock)editor.Template.FindName("DefaultZero",editor);
        var frame=(Border)editor.Template.FindName("Frame",editor);
        if(zero.Visibility!=Visibility.Visible || zero.Text!="0" || frame.BorderThickness!=new Thickness(1))
            throw new Exception("Boş kurum saati altın çerçeveli sıfır göstermiyor.");
        if(model.Calculation.ReviewItems.Any(item=>!item.BlocksPayment))
            throw new Exception("Boş kurum saati kontrol listesine uyarı ekledi.");
        capture(window,Path.Combine(output,"Transfer-hours-default-zero.png"));
        editor.Text="0";Idle(window);
        if(zero.Visibility!=Visibility.Collapsed || frame.BorderThickness!=new Thickness(0) || row.Read().NewSchoolDayHours!=0)
            throw new Exception("Elle yazılan sıfır varsayılan sıfırdan ayırt edilmedi.");
        var iban=row.Iban;
        row.Iban="";Idle(window);
        var reviewButton=(Button)window.FindName("ReviewButton");
        reviewButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Idle(window);
        var popup=(System.Windows.Controls.Primitives.Popup)window.FindName("ReviewPopup");
        if(!popup.IsOpen)throw new Exception("Hakediş Kontrolleri açılmadı.");
        var review=(ItemsControl)window.FindName("ReviewItemsList");
        var item=Descendants<Button>(review).Single(button=>button.DataContext is PeriodWorkspace.ReviewItem entry && entry.ColumnHeader==ExcelHeadings.Iban);
        item.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Idle(window);
        if(!Equals(grid.CurrentCell.Item,row) || !Equals(grid.CurrentCell.Column?.Header,ExcelHeadings.Iban))
            throw new Exception("IBAN kontrolü ilgili hücreyi açmadı.");
        var ibanCell=(DataGridCell)grid.CurrentCell.Column.GetCellContent(row).Parent;
        var cellBounds=ibanCell.TransformToAncestor(grid).TransformBounds(new Rect(0,0,ibanCell.ActualWidth,ibanCell.ActualHeight));
        var frozenWidth=grid.Columns.Take(4).Sum(c=>c.ActualWidth);
        if(popup.IsOpen || grid.FrozenColumnCount!=4 || cellBounds.Left<frozenWidth-2 || cellBounds.Right>grid.ActualWidth)
            throw new Exception("IBAN hücresi sabit sütunların sağında görünür değil.");
        if(ibanCell.Content is not TextBox ibanEditor || !ibanCell.IsEditing)
            throw new Exception("IBAN hücresi düzenlemeye açılmadı.");
        ibanEditor.Text=iban;
        grid.CommitEdit(DataGridEditingUnit.Cell,true);
        grid.CommitEdit(DataGridEditingUnit.Row,true);
        Idle(window);
        model.Calculation.SaveHybridChanges();model.Monthly.Month=10;model.Monthly.SelectedModule=model.Monthly.Modules.Single(m=>m.Id==ModuleId.DinOgretimi);model.Monthly.EntryModule=model.Monthly.SelectedModule;model.Monthly.EntryTeacher=model.Monthly.EntryTeachers.Single();model.Navigate("Entry");Idle(window);
        if(model.Monthly.HasSchoolHourSplit || !Input("EntryDayHours").IsVisible)throw new Exception("Sonraki ay tek alana dönmedi.");
        capture(window,Path.Combine(output,"Transfer-hours-next-month.png"));model.ClearCurrentWork();window.Close();
        Console.WriteLine("Kurum saatleri WPF: iki ayrı alan, toplam, varsayılan sıfır ve IBAN kontrolüne geçiş, kayıt ve sonraki ay tek alan doğrulandı.");
    }
    private static void Idle(Window w){w.Dispatcher.Invoke(()=>{},DispatcherPriority.ApplicationIdle);w.UpdateLayout();}
    private static IEnumerable<T> Descendants<T>(DependencyObject root) where T:DependencyObject
    {for(var i=0;i<VisualTreeHelper.GetChildrenCount(root);i++){var child=VisualTreeHelper.GetChild(root,i);if(child is T item)yield return item;foreach(var nested in Descendants<T>(child))yield return nested;}}
}
