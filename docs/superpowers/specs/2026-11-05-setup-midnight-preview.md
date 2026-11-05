# Gece Mavisi kurulum ve arayüz önizlemesi

## Uygulama onayı

Kullanıcı 2026-11-05 tarihinde “tamamdır uygula” diyerek ana arayüz ve kurulum uyarlamasını onayladı. Aşağıdaki önizleme notları ilk tasarım aşamasını kaydeder; uygulama durumu ve kontroller ilgili uygulama planında tutulur.

Kullanıcı, TEHYS kurulum tasarımının Ek Ders Hakediş Sistemi'ne uygun simgelerle aktarılmasını; ana program arayüzünün ise önce sohbet içinde görselleştirilmesini istedi.

Referans: https://github.com/mcalimlier/OzelEgitimHakedisSistemi/tree/v0.1.5

## Kurulum

TEHYS installer/TEHYS.iss ile aynı modern dark polar, başlık çubuğu, %115 boyut ve #10263d arka plan kullanıldı. Programın kendi Hakedis simgesiyle yan görsel ve küçük görsel üretildi. Türkçe karşılama ve bitiş metinleri Ek Ders için düzenlendi. Yeniden üretim: installer/Generate-SetupArtwork.ps1.

AppId, kullanıcı başına kurulum konumu, önceki dizin/kısayol tercihleri ve üç dosyalı dağıtım izin listesi korundu. Veri, çıktı, yedek veya kasa dosyaları kurulum paketine dahil edilmez; silme kuralı eklenmedi. Inno Setup 6.6 veya üzeri gerekir.

## Ana arayüz önerisi

Ana program koduna tema uygulanmadı. Etkileşimli, örnek verili önizleme hazırlandı: mevcut sol menü, çift sütunlu giriş, koyu mavi yüzeyler, açık mavi ana eylemler, altın seçili menü vurgusu. Veri girişi, çıktı ve kurulum görünümleri arasında geçiş yapılabilir. Aylık modül ve öğretmen seçimi başlangıçta boş; seçim sonrasında saat alanları açılır. Detay göster/kapat etkileşimi bulunur. Bu örnek gerçek kayıtlara yazmaz.

## Doğrulama ve sınır

Görseller yerel olarak açılıp kontrol edildi. Önizleme tarayıcıda açıldı; modül/öğretmen seçimi sonrasında saat alanlarının etkinleşmesi ve çıktı detayı aç/kapat akışı doğrulandı. Kurulum önizlemesi görsel olarak kontrol edildi.

Inno Setup derleyicisini edinme/kurma komutu otomatik onay denetimi tarafından engellendi. Kurulum EXE'si bu değişikliklerle derlenmedi ve yayımlanmadı. Kurulu kullanıcı programı değiştirilmedi. Sonraki aşama: uygun mevcut derleyiciyle kurulum derlemesi ve gerçek sihirbazın kontrolü; ana arayüz temasını uygulamak ayrı kullanıcı kararıdır.
