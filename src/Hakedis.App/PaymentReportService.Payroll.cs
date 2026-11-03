using Hakedis.Contracts;
using Hakedis.TemelEgitim.Core;

namespace Hakedis.App;

public sealed partial class PaymentReportService
{
    // Tek birleşik tablo; öğretmenler veya mali tutarlar sığdırma amacıyla kesilmez.
    private static LaidOutReport LayoutPayroll(PaymentReport report,ReportDetails details,DateTimeOffset date)
    {
        const double width=842,height=595,margin=7,available=width-margin*2;
        static bool RotateHeader(int column)=>column is not (1 or 2 or 3);
        const double rotatedTextWidth=180;
        for(var font=8d;font>=6; font-=.25)
        {
            // Her veri hücresinin tek metin satırına sığacağı asgari genişliği ölç.
            var tableWidths=new List<double[]>();var fits=true;
            foreach(var table in report.Tables)
            {
                var minimum=table.Headers.Select((header,i)=>Math.Max(RotateHeader(i)?Wrap(header,rotatedTextWidth,font,true).Length*font*1.22+6:12,
                    table.Rows.Select(row=>Measure(row[i],font,false)+7)
                    .Append(table.Total is null?0:Measure(table.Total[i],font+.3,true)+7).Max())).ToArray();
                if(minimum.Sum()>available){fits=false;break;}
                var spare=available-minimum.Sum();
                // Önce başlıklardaki kelimelerin harf harf bölünmesini azalt.
                var deficits=table.Headers.Select((header,i)=>RotateHeader(i)?0:Math.Max(0,
                    header.Split((char[]?)null,StringSplitOptions.RemoveEmptyEntries).Select(word=>Measure(word,font,true)+7).DefaultIfEmpty(0).Max()-minimum[i])).ToArray();
                var needed=deficits.Sum();var fraction=needed>0?Math.Min(1,spare/needed):0;
                minimum=minimum.Select((w,i)=>w+deficits[i]*fraction).ToArray();
                spare=available-minimum.Sum();var weight=table.Widths.Sum();
                tableWidths.Add(minimum.Select((w,i)=>w+spare*table.Widths[i]/weight).ToArray());
            }
            if(!fits)continue;
            var page=new ReportPage(width,height);var y=margin;
            void Text(string text,double size,bool bold=false,bool left=false)
            {
                var lines=Wrap(text,available-6,size,bold);var h=lines.Length*size*1.22+3;
                page.Cells.Add(new(margin,y,available,h,lines,size,bold,Left:left));y+=h;
            }
            Text(report.Title,10,true);
            foreach(var info in report.Information)Text(info,7,left:true);
            var tableIndex=0;
            foreach(var table in report.Tables)
            {
                if(!string.IsNullOrWhiteSpace(table.Title))Text(table.Title,8,true);
                var widths=tableWidths[tableIndex++];
                void Draw(string[] values,bool header,bool total)
                {
                    var size=total?font+.3:font;
                    var wrapped=values.Select((v,i)=>Wrap(v,header && RotateHeader(i)?rotatedTextWidth:widths[i]-6,size,header||total)).ToArray();
                    var h=Math.Max(size*1.22+3,wrapped.Select((lines,i)=>header && RotateHeader(i)?lines.Max(line=>Measure(line,size,true))+8:lines.Length*size*1.22+3).Max());
                    var x=margin;
                    for(var i=0;i<widths.Length;i++)
                    {page.Cells.Add(new(x,y,widths[i],h,wrapped[i],size,header||total,header||total,true,RotateText:header && RotateHeader(i)));x+=widths[i];}
                    y+=h;
                }
                Draw(table.Headers,true,false);
                foreach(var row in table.Rows)Draw(row,false,false);
                if(table.Total is not null)Draw(table.Total,false,true);
                y+=4;
            }
            Text("Bordroda yazılı alacaklar kayıtlara uygundur.",7);
            var signatures=new[]{("Gerçekleştirme Görevlisi",details.PreparedBy,details.PreparedTitle),("Harcama Yetkilisi",details.AuthorizedBy,details.AuthorizedTitle)};
            var signatureLines=signatures.Select(s=>Wrap($"{s.Item1}\n{date.LocalDateTime:dd.MM.yyyy}\n{s.Item2}\n{s.Item3}",available/2-6,8,false)).ToArray();
            var signatureHeight=signatureLines.Max(l=>l.Length)*8*1.22+4;
            if(y+signatureHeight>height-margin)continue;
            for(var i=0;i<2;i++)page.Cells.Add(new(margin+i*available/2,y,available/2,signatureHeight,signatureLines[i],8));
            return new(report.FileName,[page]);
        }
        throw new ArgumentException("Bordrodaki bütün satırlar okunaklı yazıyla tek yatay A4 sayfasına sığmıyor. PDF oluşturulmadı; satır veya tutar kesilmedi. Öğretmen sayısını ve uzun alanları kontrol edin.");
    }
}
