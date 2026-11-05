# Ek Ders Hakediş Sistemi

Türkçe kurulum EXE dosyası programı kullanıcı uygulama klasörüne kurar; masaüstü ve Başlat menüsünde özgün simgeli kısayollar oluşturur.
Tüm EXE kopyaları aynı `Masaüstü/Ek Ders Hakediş Sistemi` klasörünü kullanır:
`Veriler`, `Çıktılar`, `Yedekler`. Programın kaldırılması bu klasörleri silmez.
Kayıtlı hakedişler açılışta gösterilir; okuma sorunları sessizce boş listeye dönüştürülmez.
Açılış/kapanış yedekleri aynı gün değişmeyen içerik için tekrarlanmaz.
TCMB biçimine göre son 16 karakterinde harf içeren TR banka hesapları saklanabilir;
biçim kontrolü banka doğrulaması anlamına gelmez.

## 1.0.14 kararlı sürüm — Gece Mavisi

- Ana ekranlar, giriş alanları, tablolar, açılır listeler, takvim ve yardımcı pencereler ortak Gece Mavisi tasarımındadır.
- Programa özel simgelerle Türkçe koyu mavi kurulum ekranı; açık mavi işlem düğmeleri ve altın menü vurguları.
- Hesaplamalar, kayıt biçimi, kurum geçişi mantığı, kasa davranışı ve resmî çıktı düzenleri korunur.
- İşe başlama ayı yeşil işaretlenir; normal ve seçili satırlarda metin kontrastı doğrulanmıştır. Eksik IBAN uyarısı korunur.
- Doğrulama: 315 birim testi, tam WPF/PDF/XML akışları, gerçek tablo hücreleri ve takvim gün/ay/yıl/klavye testleri.
- Dağıtımda yalnız program, kullanım belgesi ve lisans vardır; kişisel kayıtlar, banka bilgileri, kasa, yedek ve çıktılar bulunmaz.
- [Güncel kurulum ve taşınabilir paket](https://github.com/mcalimlier/Ek-Ders-Hakedis-Sistemi/releases/tag/v1.0.14)

## 1.0.13 sürümü

- Boş Halkbank Excel şablonu programa gömülüdür; kullanıcıdan şablon dosyası seçmesi istenmez.
- Banka çıktısı, ödeme tarihi seçildikten sonra bölüm/dönem klasörüne otomatik kaydedilir; başarı bildiriminden sonra klasör açılır.
- Şablonun düzeni ve sabit hücreleri korunur. Firma bilgileri bölümün banka sabitlerinden, personel bilgileri seçilen kayıtlı hakedişten alınır.
- Kişisel kayıtlar, banka sabitleri, kasa ve kurtarma anahtarları dağıtılmaz. Gömülü şablon boş ve kişisel verilerden temizlenmiştir.
- Doğrulama: 315 birim testi ve şablon dosyası bulunmayan temiz kurulumda banka çıktı akışı.
- [Güncel kurulum ve taşınabilir paket](https://github.com/mcalimlier/Ek-Ders-Hakedis-Sistemi/releases/tag/v1.0.13)

## 1.0.12 sürümü

- Bordroda mali başlıklar düşeydir; uzun başlıklar uygun genişlikte iki satıra bölünür. Sütun adları ve sırası düzenlendi, gereksiz sabit/birim ücret sütunları kaldırıldı.
- Kalan gelir ve damga vergisi istisnalarının kişi değerleri gösterilir; toplam hücreleri boş bırakılır. Damga istisna kalanı sıfırın altına düşmez. GV istisnası ve brüt asgari ücret bütün modüllerde ortak düzenlenir.
- PDF başarı bildirimi kapatılınca oluşturulan klasör açılır. Detay penceresinde talep edilecek ödenekler kartı bulunur.
- Hibrit tabloda işe başlama ayı seçili ayla eşleşen satır açık yeşildir; yıl ve saat girişi koşul değildir. Ay değişince renk yeniden değerlendirilir; bu işaret bordroya taşınmaz.
- IBAN Kontrol sütunu kaldırıldı; doğrulama sürer. Kurum geçişindeki iki gündüz saati uygun modüller arasında önizlemeye birlikte yansır; ödeme modülü ve kayıt sürümü korunur.
- Doğrulama: 314 birim testi, tüm WPF akışları ve PDF/XML kontrolleri geçti.
- Dağıtım yalnız program, kullanım belgesi ve lisansı içerir. Öğretmenler, hakedişler, banka sabitleri/şablonları, kişisel kasa ve kurtarma anahtarları dağıtılmaz.
- [Güncel kurulum ve taşınabilir paket](https://github.com/mcalimlier/Ek-Ders-Hakedis-Sistemi/releases/tag/v1.0.12)

## 1.0.10 sürümü

- Tek satırlı bordro: kullanılmayan DYK/icra/gündüz saat sütunları gizlenir. GV matrahı ve pozitif kalan istisna ayrı sütunlardır; kayıtlı mali sonuçlar korunur.
- İşe başlama ve ayrılış tarihleri yan yana; hakediş Detay penceresinde özetler ve salt okunur liste korunarak gereksiz iki kart kaldırıldı.
- Kasa başka uygulamaya geçince kilitlenmez; gösterilen şifre maskelenir. Beş dakika hareketsizlik ve Windows kilidi koruması sürer.
- Kurtarma kodu onaya kadar görünür. Açık kasada onayla yeni kurtarma kodu üretilebilir; işyeri bilgileri ve ana parola korunur.
- Doğrulama: 300 Release birim testi ve tam WPF/PDF/XML/kasa testleri geçti.
- Kaynak ve dağıtım kişisel veri, kasa, kurtarma kodu veya çıktı içermez. Yerel güncelleme öncesi veri/kasa ve EXE yedeği alınır.

## 1.0.6 birleşik sürüm

- İki bilgisayarda geliştirilen özellikler bir aradadır: şifre kasası, bordro ayrıntıları, modül filtreleme, güvenli aylık giriş ve okul geçişi saatleri.
- Şifre Kasası iki sabit işyeri grubunu içerir; ana parola en az 10 karakter, bir harf, bir rakam ve bir sembol içermelidir. Şifreli taşıma, kurtarma ve otomatik kilitleme desteklenir.
- `Kasa` klasörü yalnız kullanıcı kendi kasasını oluşturduğunda açılır. Kişisel kasa ve kurtarma anahtarı kaynak koduna veya dağıtım paketine dahil edilmez. Kasa, normal veri yedeğinden ayrı dışa aktarılmalıdır.
- [Kasa kullanımı ve güvenlik sınırları](docs/SIFRE_KASASI.md)
- Doğrulama: 281 Release birim testi ve birleşik WPF akışları (kasa, kurum saatleri, modül filtresi, Detay, PDF/XML) geçti.

## 1.0.5 güncellemesi

- Çıktılar modüle göre filtrelenir; Detay düğmesi kayıtlı hakedişin özet kartlarını ve salt okunur bordro listesini açar.
- Aylık ders kartı modül ve öğretmen seçilmeden boş açılır. Öğretmen Listesi → Veri Girişine Al, açık seçimi karta taşır.
- IBAN olmadan öğretmen kaydedilebilir. Hakediş oluşturulurken eksik IBAN açıklayıcı bildirimle engellenir; öğretmen listesinde eksikler kırmızı gösterilir.
- Sayısal girişlerde sıfıra tıklanınca alan temizlenir; değer yazılmadan çıkılırsa sıfır geri gelir.
- Mevcut kayıtlar ve dondurulmuş hakedişler korunur; güncelleme yalnız uygulama dosyasına yapılır. Veri ve program yedeği alınmalıdır.
- Doğrulama: 265 birim testi ve tüm WPF arayüz akışları (PDF, SGK XML, kayıt, modül filtresi, Detay/Kapat) geçti.

C# / WPF Windows masaüstü projesi; OEHS düğme, kart ve panel şablonlarını kullanır. Öğretmen kaydı yalnız Veri Girişi'nde yapılır. Aylık ders girişi, dönem sabitleri ve canlı hibrit hakediş tablosu JSON'a kaydedilir. Aylık kayıtlar ve hakedişler modül/yıl/ay kapsamında ayrılır; sabitler ortaktır. SQL, son kullanıcıda Excel veya ayrıca .NET kurulumu gerektirmez. Kaynak Excel başlıkları aynen korunur. 7.190 meslek kodu aramalı açılır listeden seçilir. Eksik Gün Sayısı SGK yan bilgisidir; parasal hakedişi etkilemez. Kayıtlı hakedişten üç PDF, doğrudan yazdırma ve özgün XSD ile doğrulanan SGK XML çıktısı alınır.

Her öğretmenin tek kişi kartı vardır; aynı TC ile ikinci kişi açılmaz. İsteğe bağlı okul/modül değişikliği alanında okul, tarih ve hedef modül kaydedilir. Geçiş tarihi yeni okulda başlama tarihidir. Geçiş ayında iki kurumun gündüz saatleri ayrı girilir, toplam seçilen tek ödeme modülünde hesaplanır; boş taraf sıfır kabul edilip hibrit tabloda uyarılır. Sonraki aylarda tek alan kullanılır; yeni geçiş tanımlanan ayda iki alan yeniden açılır. Kişi iki modül bordrosuna bölünmez. Görev geçmişi normal SGK işe giriş/çıkış tarihlerini veya kayıtlı bordroları değiştirmez. Eski modülsüz veriler ilk sürümün Temel Eğitim Ücretli Öğretmen kapsamına göre, orijinali yedeklenerek yeni biçime geçirilir.

## Kullanıcının belirlediği kapsam

- Windows için kendi .NET çalışma ortamını içeren EXE/ZIP yayınlanır. Yayın betiği taşınabilir paket, `scripts/Build-Installer.ps1` ise Türkçe kurulum sihirbazı üretir.
- SQL/SQLite ve veri tabanı sunucusu kullanılmaz.
- Temel Eğitim Ücretli Öğretmen, Din Öğretimi Ücretli Öğretmen, Mesleki Eğitim Merkezi Ücretli Öğretmen, Halk Eğitim Merkezi Usta Öğretici ve Halk Eğitim Merkezi Emekli Usta Öğretici ayrı hesaplama çekirdekleriyle kullanılabilir. Diğer eğitim modülleri daha sonra kendi kurallarıyla eklenir.
- OEHS projesinin `feature/official-payment-document-templates` dalındaki renk ve kart düzeni referans alınır.
- Bordro, Banka Listesi ve Personel Bildirimi çıktı akışındadır. Bordro tek yatay A4'e okunaklı sığmıyorsa açık hata verilir; satırlar düşürülmez.
- Kişisel bordro talep üzerine; diğer bilgi amaçlı Excel çıktıları isteğe bağlıdır.
- Halkbank yükleme dosyası ayrı bir aktarım çıktısıdır, yazdırılacak belge değildir.

## Belgeler

- [Güncel saklama, taşınabilirlik ve çıktı tasarımı](docs/TASARIM_KARARLARI.md)
- [Excel incelemesi ve güncellenmiş yol haritası](docs/INCELEME_VE_YOL_HARITASI.md)
- [Ayrı hesaplama çekirdekleri ve geliştirme komutları](docs/MODUL_MIMARISI.md)
- [İlk sürüm kullanım notu](docs/KULLANIM.txt)
- [Taşınabilir EXE/ZIP hazırlama ve GitHub Releases](docs/TASINABILIR_YAYIN.md)
- [Arayüz referansı ve yeni kayıt akışı](docs/ARAYUZ_REFERANSI.md)
- [Modüle ait öğretmen kayıtları ve veri geçişi](docs/MODUL_OGRETMEN_KAYITLARI.md)

Gerçek personel Excel'leri, kullanıcı verileri, yedekler ve ödeme dosyaları bu depoya dahil edilmez.


[Excel denetimi, 50 senaryo karşılaştırması ve yeni ekran akışı](docs/EXCEL_DENETIMI.md).

[Yeni çekirdekler öncesi uygulama denetimi](docs/CEKIRDEK_ONCESI_DENETIM_2026-10-24.md).

[Din Öğretimi Ücretli Öğretmen çekirdeği, Excel formülleri ve kaynakta tespit edilen sapmalar](docs/DIN_OGRETIMI.md).

[Halk Eğitim Merkezi Usta Öğretici çekirdeği ve Excel doğrulaması](docs/HALK_EGITIMI.md).

[Emekli Usta Öğretici çekirdeği, ayrı dönem sabitleri ve SGK belge 02](docs/EMEKLI_HALK_EGITIMI.md).

[Mesleki Eğitim Merkezi Ücretli Öğretmen çekirdeği ve eski/güncel sabit ayrımı](docs/MESLEKI_EGITIMI.md).

Kişisel Bordro ekranında modül ve ay/yıl seçilerek kayıtlı hakedişten tek öğretmen için A4 PDF alınır; Klasör düğmesi kaydedilen konumu açar. Sol menüde temel işlemler ve Kişisel Bordro/Dönem Sabitleri grupları ayrıdır. Öğretmen Listesi adı, sabitlerin hizaları ve simgelerden ayrı tutar alanları günceldir; sol altta kullanıcı imzası bulunur.

Program adı Ek Ders Hakediş Sistemi'dir; güncel derleme `Ek Ders Hakediş Sistemi.exe` üretir. Diğer İşlemler grubunun sonundaki Ayarlar paneli 1280×720, 1366×768 ve 1920×1080 pencere tercihlerini JSON'da saklar. Pencere kullanılabilir ekran alanına sığdırılır; tercih sonraki açılışta uygulanır. Kilitli dönem sabitleri gri, Güncelle ile açılan alanlar beyazdır. Hibrit başlıklar değiştirilmeden satırlara bölünür; toplam sütun genişliği azaltılmıştır. Yeni PDF/XML çıktılarının kökü Masaüstü/Ek Ders Hakediş Sistemi/Çıktılar'dır; daha önceki çıktı dosyaları taşınmaz.
