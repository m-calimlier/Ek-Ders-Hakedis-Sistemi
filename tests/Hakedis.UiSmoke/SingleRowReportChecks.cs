using System.IO;
using System.Windows;
using Hakedis.App;
using Hakedis.Contracts;
using Hakedis.Storage;
using Hakedis.TemelEgitim.Core;
internal static class SingleRowReportChecks
{
    public static void Run(string output,Action<FrameworkElement,string> capture)
    {
        foreach(var (dyk,icra) in new[]{(0m,0m),(5m,0m),(0m,100m),(5m,100m)})
        {
            var p=PeriodValues.Source;var core=new TeacherPayrollModule();
            var people=new[]{"Ayşe Deniz YILMAZ","Mehmet Ali DEMİR","Hatice Alev ŞİMŞEK"};
            var rows=people.Select((name,index)=>
            {
                var t=new TeacherRecord(Guid.NewGuid(),name,"1000000014"+index,"TR330006100519786457841326","Deneme Okulu","2341.04",null,null);
                var input=new MonthlyInput(t.Id,144+index,index==1?dyk:0,index==2?icra:0);
                var result=core.Calculate(new(input.DayHours,input.DykHours,input.Garnishment),TeacherParameters.SourceSeptember2026);
                return new PaymentRow(t,input,result);
            }).ToArray();
            var details=new ReportDetails("Deneme İlçe Milli Eğitim Müdürlüğü","Deneme Bankası","Görevli","Şube Müdürü","Yetkili","İlçe Milli Eğitim Müdürü");
            var data=new PaymentSnapshot(2026,10,p,rows,"",Details:details,Movement:new(0,0,0,RuleVersion:"employment-dates.v2"));
            var record=new ArchivedPayment(Path.Combine(output,"synthetic.json"),"12345678",new(1,ModuleId.TemelEgitim,core.RuleVersion,DateTimeOffset.Now,data));
            var service=new PaymentReportService();var report=service.Prepare(record,details)[0];var page=report.Pages.Single();
            var body=page.Cells.Where(c=>c.Border && !c.Header && !c.Bold).ToArray();
            if(body.GroupBy(c=>c.Y).Count()!=rows.Length || body.Any(c=>c.Lines.Length!=1))throw new Exception("Tek satır koşulu sağlanmadı.");
            if(page.Cells.Any(c=>c.X<0 || c.Y<0 || c.X+c.Width>page.Width+.1 || c.Y+c.Height>page.Height+.1))throw new Exception("Bordro sayfa sınırını aştı.");
            if(!page.Cells.Any(c=>c.RotateText && c.Lines.Contains("Brüt Ücret Tutarı")) || page.Cells.Any(c=>c.RotateText && !c.Header))throw new Exception("Düşey başlıklar yalnız başlık satırına uygulanmalı.");
            var doc=service.PrintDocument(report);var visual=doc.Pages[0].GetPageRoot(false);visual.Measure(new Size(visual.Width,visual.Height));visual.Arrange(new Rect(0,0,visual.Width,visual.Height));visual.UpdateLayout();
            capture(visual,Path.Combine(output,$"single-row-dyk-{dyk}-icra-{icra}.png"));
            service.Export(record,details,Path.Combine(output,$"pdf-dyk-{dyk}-icra-{icra}"));
        }
        Console.WriteLine("Tek satırlı bordro: dört DYK/icra kombinasyonu; PDF/yazdırma tek sayfa ve kişi başına tek satır; hiçbir hücre taşmıyor.");
    }
}
