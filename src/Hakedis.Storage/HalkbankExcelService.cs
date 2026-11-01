using System.Globalization;
using System.Text.RegularExpressions;
using Hakedis.Contracts;
using NPOI.HSSF.Record;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.POIFS.FileSystem;

namespace Hakedis.Storage;

public static class HalkbankExcelService
{
    private const int FirstDataRow=12;
    private const int MaxBytes=16*1024*1024;
    private static readonly CultureInfo Turkish=CultureInfo.GetCultureInfo("tr-TR");
    public static BankFirmSettings Inspect(byte[] template)
    {
        using var book=Open(template);var sheet=book.GetSheetAt(0);
        string Value(int row)=>CellText(sheet.GetRow(row)?.GetCell(1));
        return new(Value(1),Value(2),Value(3));
    }
    public static byte[] ReadTemplate(string path)
    {
        using var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite);
        if(stream.Length is <8 or >MaxBytes)throw new InvalidDataException("Halkbank şablon dosyasının boyutu geçersiz.");
        var bytes=new byte[(int)stream.Length];stream.ReadExactly(bytes);return bytes;
    }
    private static HSSFWorkbook Open(byte[] bytes)
    {
        if(bytes.Length is <8 or >MaxBytes || !bytes.AsSpan(0,8).SequenceEqual(new byte[]{0xD0,0xCF,0x11,0xE0,0xA1,0xB1,0x1A,0xE1}))
            throw new InvalidDataException("Excel 97–2003 biçimindeki orijinal Halkbank .xls şablonunu seçin.");
        HSSFWorkbook? book=null;
        try
        {
            book=new HSSFWorkbook(new MemoryStream(bytes),true);
            if(book.NumberOfSheets!=2 || book.GetSheetName(0)!="HalkbankDTO")throw new InvalidDataException("Şablonun HalkbankDTO sayfası ve ikinci açıklama sayfası korunmalıdır.");
            var sheet=book.GetSheetAt(0);
            for(var c=0;c<5;c++)if(string.IsNullOrWhiteSpace(CellText(sheet.GetRow(11)?.GetCell(c))))throw new InvalidDataException("Halkbank şablonunun 12. satırındaki sütun başlıkları eksik.");
            if(CellText(sheet.GetRow(11).GetCell(2))!="Tutar")throw new InvalidDataException("Halkbank şablonunun tutar sütunu beklenen konumda değil.");
            var payment=CellText(sheet.GetRow(5)?.GetCell(1));
            if(payment.Length==0 || payment!=CellText(sheet.GetRow(0)?.GetCell(6)))throw new InvalidDataException("Banka ödeme dosyası için Ödeme seçili olan şablonu kullanın.");
            return book;
        }
        catch(InvalidDataException){book?.Dispose();throw;}
        catch(Exception ex) when(ex is not OutOfMemoryException && ex is not StackOverflowException)
        {book?.Dispose();throw new InvalidDataException("Halkbank .xls şablonu okunamadı veya desteklenmeyen biçimde.",ex);}
    }
    private static string CellText(ICell? cell)=>cell?.CellType switch
    {
        CellType.String=>cell.StringCellValue,
        CellType.Numeric=>cell.NumericCellValue.ToString("0",CultureInfo.InvariantCulture),
        _=>""
    };
    public static byte[] Build(byte[] template,ArchivedPayment record,DateOnly paymentDate,BankFirmSettings? firm=null)
    {
        if(paymentDate.Year<1900)throw new ArgumentException("Ödeme tarihini seçin.");
        var data=record.Document.Data;var rows=data.Rows;
        if(rows.Length is 0 or >65524)throw new ArgumentException("Banka dosyası için kişi sayısı 1–65524 aralığında olmalıdır.");
        foreach(var row in rows)
        {
            if(!TeacherDirectory.IsBankAccountFormatValid(row.Teacher.Iban))throw new ArgumentException(row.Teacher.FullName+": banka aktarımı için IBAN bilgisi eksik veya geçersiz.");
            if(!Regex.IsMatch(row.Teacher.IdentityNumber,"^[0-9]{11}$"))throw new ArgumentException(row.Teacher.FullName+": banka aktarımı için 11 rakamlı kimlik numarası gereklidir.");
            if(string.IsNullOrWhiteSpace(row.Teacher.FullName))throw new ArgumentException("Banka dosyasında personel adı ve soyadı boş olamaz.");
            if(row.Result.Net<=0 || decimal.Round(row.Result.Net,2)!=row.Result.Net)throw new ArgumentException(row.Teacher.FullName+": banka ödeme tutarı sıfırdan büyük ve kuruş hassasiyetinde olmalıdır.");
        }
        if(rows.Select(r=>r.Teacher.IdentityNumber).Distinct(StringComparer.Ordinal).Count()!=rows.Length)throw new ArgumentException("Hakedişte yinelenen kimlik numarası var; banka dosyası oluşturulmadı.");
        var normalized=firm?.Validate();
        using var book=Open(template);var sheet=book.GetSheetAt(0);
        var modelRow=sheet.GetRow(FirstDataRow)??throw new InvalidDataException("Şablonun ilk personel satırı bulunamadı.");
        var styles=Enumerable.Range(0,5).Select(c=>modelRow.GetCell(c)?.CellStyle).ToArray();
        var types=Enumerable.Range(0,5).Select(c=>modelRow.GetCell(c)?.CellType??CellType.Blank).ToArray();
        var height=modelRow.Height;
        // Preserve missing cells as missing and existing blanks as true blanks.
        for(var r=FirstDataRow;r<=sheet.LastRowNum;r++)for(var c=1;c<=4;c++)sheet.GetRow(r)?.GetCell(c)?.SetBlank();
        sheet.GetRow(0).GetCell(1).SetCellValue(int.Parse(paymentDate.ToString("yyyyMMdd",CultureInfo.InvariantCulture),CultureInfo.InvariantCulture));
        if(normalized is not null)
        {
            sheet.GetRow(1).GetCell(1).SetCellValue(double.Parse(normalized.CustomerNumber,CultureInfo.InvariantCulture));
            if(normalized.BranchCode.Length==0)sheet.GetRow(2).GetCell(1).SetBlank();else sheet.GetRow(2).GetCell(1).SetCellValue(double.Parse(normalized.BranchCode,CultureInfo.InvariantCulture));
            sheet.GetRow(3).GetCell(1).SetCellValue(normalized.Account);
        }
        var month=Turkish.DateTimeFormat.GetMonthName(data.Month);
        var label=record.Document.ModuleId switch {ModuleId.MeslekiEgitim=>"Mesleki Eğitim Ücretli Öğretmen",ModuleId.HalkEgitimUstaOgretici=>"Usta Öğretici",ModuleId.HalkEgitimEmekliUstaOgretici=>"Emekli Usta Öğretici",_=>record.Module};
        var description=$"{month} {data.Year} {label} Ödeme";
        if(description.Length>50)throw new ArgumentException("Banka dosyası açıklaması 50 karakteri aşıyor.");
        sheet.GetRow(4).GetCell(1).SetCellValue(description);
        sheet.GetRow(8).GetCell(1).SetCellValue($"{data.Year} {month}");
        for(var i=0;i<rows.Length;i++)
        {
            var row=sheet.GetRow(FirstDataRow+i);
            if(row is null){row=sheet.CreateRow(FirstDataRow+i);row.Height=height;}
            ICell Cell(int c){var cell=row.GetCell(c);if(cell is null){cell=row.CreateCell(c,CellType.Blank);if(styles[c] is { } style)cell.CellStyle=style;}return cell;}
            Cell(1).SetCellValue(rows[i].Teacher.Iban);
            Cell(2).SetCellValue((double)rows[i].Result.Net);
            if(types[3]==CellType.String)Cell(3).SetCellValue(rows[i].Teacher.IdentityNumber);else Cell(3).SetCellValue(double.Parse(rows[i].Teacher.IdentityNumber,CultureInfo.InvariantCulture));
            Cell(4).SetCellValue(rows[i].Teacher.FullName);
        }
        var evaluator=book.GetCreationHelper().CreateFormulaEvaluator();
        void Total(int r,decimal expected)
        {
            var cell=sheet.GetRow(r)?.GetCell(4)??throw new InvalidDataException("Şablonun toplam hücresi eksik.");
            if(cell.CellType==CellType.Formula)evaluator.EvaluateFormulaCell(cell);else cell.SetCellValue((double)expected);
            if((cell.CellType==CellType.Formula && cell.CachedFormulaResultType==CellType.Error) || decimal.Round((decimal)cell.NumericCellValue,2)!=expected)throw new InvalidDataException("Şablon toplam formülü seçilen hakedişin tüm satırlarını kapsamıyor.");
        }
        Total(7,record.Net);Total(8,rows.Length);
        // Remove unreferenced old recipient text from the SST without changing cell indices or static template strings.
        var used=new HashSet<string>(StringComparer.Ordinal);
        for(var s=0;s<book.NumberOfSheets;s++)foreach(IRow row in book.GetSheetAt(s))foreach(var cell in row.Cells)
            if(cell.CellType==CellType.String)used.Add(cell.StringCellValue);
        var strings=book.Workbook.Records.OfType<SSTRecord>().SingleOrDefault();
        if(strings is not null)for(var i=0;i<strings.NumUniqueStrings;i++){var value=strings.GetString(i);if(!used.Contains(value.String))value.String="";}
        using var output=new MemoryStream();book.Write(output,true);
        return PreserveContainerStreams(template,output.ToArray());
    }
    private static byte[] PreserveContainerStreams(byte[] template,byte[] edited)
    {
        var source=new POIFSFileSystem(new MemoryStream(template));
        var changed=new POIFSFileSystem(new MemoryStream(edited));
        var result=new POIFSFileSystem();
        void Copy(DirectoryNode from,DirectoryNode to,bool root)
        {
            to.StorageClsid=from.StorageClsid;
            using var entries=from.Entries;
            while(entries.MoveNext())
            {
                var entry=entries.Current;
                if(entry.IsDirectoryEntry)Copy((DirectoryNode)entry,(DirectoryNode)to.CreateDirectory(entry.Name),false);
                else
                {
                    using var input=root && entry.Name=="Workbook"?changed.Root.CreateDocumentInputStream("Workbook"):from.CreateDocumentInputStream(entry.Name);
                    to.CreateDocument(entry.Name,input);
                }
            }
        }
        // A fresh container avoids retaining deleted recipient bytes in unused OLE sectors.
        try
        {
            Copy(source.Root,result.Root,true);
            using var output=new MemoryStream();result.WriteFileSystem(output);return output.ToArray();
        }
        finally {source.Close();changed.Close();result.Close();}
    }
    public static void Export(string templatePath,ArchivedPayment record,DateOnly date,string outputPath,BankFirmSettings? firm=null)
    {
        var target=Path.GetFullPath(outputPath);
        if(!target.EndsWith(".xls",StringComparison.OrdinalIgnoreCase))throw new ArgumentException("Banka dosyası .xls uzantısıyla kaydedilmelidir.");
        if(string.Equals(target,Path.GetFullPath(templatePath),StringComparison.OrdinalIgnoreCase))throw new ArgumentException("Orijinal banka şablonunun üzerine yazılamaz; farklı dosya adı seçin.");
        var bytes=Build(ReadTemplate(templatePath),record,date,firm);
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        var temp=target+"."+Guid.NewGuid().ToString("N")+".tmp";
        try{using(var stream=new FileStream(temp,FileMode.CreateNew,FileAccess.Write,FileShare.None)){stream.Write(bytes);stream.Flush(true);}File.Move(temp,target,true);}
        finally{if(File.Exists(temp))File.Delete(temp);}
    }
}
