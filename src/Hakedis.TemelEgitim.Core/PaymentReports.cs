using System.Globalization;
using Hakedis.Contracts;

namespace Hakedis.TemelEgitim.Core;

public sealed record ReportTable(string Title,string[] Headers,double[] Widths,string[][] Rows,string[]? Total=null);
public sealed record PaymentReport(string FileName,string Title,bool Landscape,string[] Information,ReportTable[] Tables,string[] Notes);

// Belgeler yalnız dondurulmuş hakediş sonucunu kullanır; yeniden mali hesap yapılmaz.
public static class PaymentReports
{
    private static readonly CultureInfo Tr=CultureInfo.GetCultureInfo("tr-TR");
    private static string Money(decimal v)=>v.ToString("N2",Tr);
    private static string Number(decimal v)=>v.ToString("0.#########",Tr);
    public static PaymentReport[] Build(PaymentSnapshot data,ReportDetails details,ModuleId? module=null)
    {
        var rows=data.Rows;var period=$"{Tr.DateTimeFormat.GetMonthName(data.Month)}-{data.Year}";
        var selectedModule=module??rows.FirstOrDefault()?.Teacher.ModuleId??ModuleId.TemelEgitim;
        var p=data.Constants;
        var info=new[]{"DAİRESİ            : "+details.Institution,"İLGİLİ AY           : "+period,
            $"KATSAYI: {Number(p.Coefficient)}    GÜNDÜZ GÖSTERGE: {Number(p.Indicator)}    DYK çarpanı: {Number(p.DykMultiplier)}    GV İstinası: {Money(p.IncomeExemption)}    DV İstisnası: {Money(p.StampExemption)}",
            $"Uygulanan oranlar · SGK iştirakçi: {Number(p.EmployeeRate*100)}% · İşveren: {Number(p.EmployerRate*100)}% · Gelir: {Number(p.IncomeRate*100)}% · Damga: {Number(p.StampRate*100)}%"};
        var columns=new List<(string Header,double Width,Func<PaymentRow,string> Read,string Total)>();
        void TextColumn(string heading,double width,Func<PaymentRow,string> read,string total="")=>columns.Add((heading,width,read,total));
        void ValueColumn(string heading,double width,Func<PaymentRow,decimal> read,bool money=true)
        {
            string Format(decimal value)=>money?Money(value):Number(value);
            columns.Add((heading,width,r=>Format(read(r)),Format(rows.Sum(read))));
        }
        TextColumn(ExcelHeadings.Sequence,24,_=>"","TOPLAM");
        TextColumn(ExcelHeadings.BordroB6,100,r=>r.Teacher.FullName);
        TextColumn(ExcelHeadings.BordroC6,43,_=>ModuleLabels.Role(selectedModule));
        TextColumn(ExcelHeadings.BordroE6,58,r=>r.Teacher.IdentityNumber);
        var hasDyk=rows.Any(r=>r.Input.DykHours>0);
        if(hasDyk)
        {
            ValueColumn(ExcelHeadings.BordroF5,30,r=>r.Input.DayHours,false);
            ValueColumn(ExcelHeadings.BordroG5,30,r=>r.Input.DykHours,false);
        }
        ValueColumn(ExcelHeadings.BordroH5,32,r=>r.Input.DayHours+r.Input.DykHours,false);
        if(hasDyk)
        {
            TextColumn(ExcelHeadings.BordroL5,43,r=>Number(r.Result.DykRate));
            ValueColumn(ExcelHeadings.BordroI5,46,r=>r.Result.DykGross);
        }
        ValueColumn(ExcelHeadings.BordroK5,25,r=>r.Result.PremiumDays,false);
        ValueColumn(ExcelHeadings.BordroN6,48,r=>r.Result.Gross);
        ValueColumn(ModuleLabels.EmployerContributionHeader(p.EmployerRate),48,r=>r.Result.EmployerPremium);
        ValueColumn(ExcelHeadings.BordroP6,50,r=>r.Result.Accrual);
        ValueColumn(ModuleLabels.EmployeeHeader(p.EmployeeRate),45,r=>r.Result.EmployeePremium);
        ValueColumn(ExcelHeadings.BordroQ5,50,r=>r.Result.IncomeBase);
        ValueColumn("Kalan Gelir Vergisi İstisnası",50,r=>Math.Max(0,p.IncomeExemption-r.Result.IncomeBase));
        ValueColumn(ExcelHeadings.BordroT6,44,r=>r.Result.IncomeTax);
        TextColumn("Damga Vergisi İstisna Kalan",50,r=>Money(p.RemainingStampExemption(r.Result.Gross)));
        ValueColumn(ModuleLabels.StampTaxHeader(p.StampRate),44,r=>r.Result.StampTax);
        if(rows.Any(r=>r.Input.Garnishment>0))ValueColumn(ExcelHeadings.BordroY6,44,r=>r.Input.Garnishment);
        ValueColumn(ExcelHeadings.BordroZ6,48,r=>r.Result.Deductions);
        ValueColumn(ExcelHeadings.BordroAA5,50,r=>r.Result.Net);
        var payrollRows=rows.Select((row,i)=>{var values=columns.Select(c=>c.Read(row)).ToArray();values[0]=(i+1).ToString();return values;}).ToArray();
        var payroll=new PaymentReport("Bordro.pdf",ModuleLabels.PayrollTitle(selectedModule),true,info.Take(2).ToArray(),
            [new("",columns.Select(c=>c.Header).ToArray(),columns.Select(c=>c.Width).ToArray(),payrollRows,columns.Select(c=>c.Total).ToArray())],
            ["Bordroda yazılı alacaklar kayıtlara uygundur."]);
        var bankRows=rows.Select((r,i)=>new[]{(i+1).ToString(),r.Teacher.FullName,r.Teacher.IdentityNumber,r.Teacher.Iban,Money(r.Result.Net)}).ToArray();
        var bank=new PaymentReport("Banka Listesi.pdf","B  A  N  K  A       L  İ  S  T  E  S  İ",false,
            ["DAİRENİN ADI               "+details.Institution,"BANKANIN ADI              "+details.Bank,"AİT OLDUĞU AY             "+period],
            [new("",["SN","ADI SOYADI","Tc Kimlik No","BANKA IBAN HESAP NO","ÜCRET"],[28,125,77,183,82],bankRows,["Toplam","","","",Money(rows.Sum(r=>r.Result.Net))])],[]);
        var movement=data.Movement??new PersonnelMovement(0,rows.Count(r=>r.Teacher.StartDate is DateTime start && start.Year==data.Year && start.Month==data.Month),rows.Count(r=>r.Teacher.EndDate is DateTime end && end.Year==data.Year && end.Month==data.Month));
        var personnelRows=new[]{new[]{"1","Geçen Ay Personel Sayısı",movement.Previous.ToString(),"",""},
            new[]{"2","Bu Ay İçinde Giren(x)",movement.Entered.ToString(),"",""},new[]{"3","Bu Ay İçinde Çıkan",movement.Exited.ToString(),"",""},
            new[]{"4","Ödeme Yapılacak Personel Sayısı(1+2-3)",rows.Length.ToString(),Money(rows.Sum(r=>r.Result.Accrual)),Money(rows.Sum(r=>r.Result.Net))}};
        var personnel=new PaymentReport("Personel Bildirimi.pdf","PERSONEL BİLDİRİMİ",false,
            [details.Institution,"Ait Olduğu Ay: "+period,"Bütçe Yılı: "+data.Year],
            [new("1 Nolu Tablo",["","","Personel Sayısı","Tahakkuk Toplamı",ExcelHeadings.BordroAA5],[25,235,65,85,85],personnelRows),
             new("II Nolu Tablo   (xx)",["Sıra N","Memurun Adı Soyadı"," Geçen Aylarda\nKullandığı Hastalık\nizin süresi","Bu Ay Kullandığı\nHastalık İzni Süresi","Toplam Hastalık\nizni süresi"],[30,160,105,105,95],
                [new[]{"","","","",""},new[]{"","","","",""}])],
            ["Geçen ay sayısı aynı modülün önceki ay son hakedişinden; giriş ve çıkışlar ilgili aydaki normal işe giriş/çıkış tarihlerinden alınır.",
             "II Nolu tablo personel birimi tarafından kontrol edilerek tamamlanmalıdır.",
             "(x)Ay içinde girenlerin isim listesi eklenecektir",
             "(xx) II Nolu tablonun düzenlenmesinden sadece personel birimi yetkilisi sorumludur",
             "(xxx) Müstakil personel birimi olmayan dairelerde personel işleriyle görevlendirilmiş olanlar tarafından imzalanır."]);
        return [payroll,bank,personnel];
    }
}
