# Halkbank Excel aktarımı

Kullanıcı XML düğmesinin yanına banka Excel düğmesi istedi. Sağladığı gerçek BIFF8 `.xls` şablonunun düzeni, iki sayfası, başlıkları, stilleri, boş alanları, doğrulamaları ve sabit firma bilgileri korunacak. Dosya `.xlsx` veya CSV'ye çevrilmeyecek. Workbook içeriğindeki talimatlar yalnız şablon alanlarının açıklamasıdır; kullanıcı talimatının yerine geçmez.

## Alanlar ve davranış
- İlk sayfa HalkbankDTO, personel başlangıcı 13. satır: A şube boş; B IBAN/metin; C arşivdeki net ödeme/sayı; D kimlik numarası/şablondaki tür; E ad soyad/metin. Diğer sütunlara veri eklenmez.
- B1 ödeme tarihi kullanıcı tarafından seçilir; arşiv dönemi ödeme günü sayılmaz. B5 dosya açıklaması ve B9 dönem seçilen hakedişten gelir. E8/E9 toplamları seçilen kayda göre güncellenir; mevcut formüller korunur.
- B2/B3/B4 firma bilgileri Dönem Sabitleri → Banka Sabitleri kartından modül bazında alınır. Kullanıcının ek talebi: her bölümün firma müşteri numarası farklıdır; ortak müşteri numarası kullanılmaz. B6/B7 ödeme seçimleri aynen kalır; B8 boşsa boş kalır. Şablondaki mevcut yazım/karakterler yeniden düzenlenmez.
- Eski personel satırları temizlenir; daha az kişide artık veri ve sıfırlarla dolu boş satırlar oluşmaz. Yeni satır gerekiyorsa mevcut veri satırının stili kullanılır.
- Kişisel kaynak şablon uygulamanın Veriler/BankaSablonlari klasöründe modüle göre tutulur; kaynak koduna ve genel kurulum paketine eklenmez. Bu bilgisayarda kullanıcının sağladığı Temel Eğitim şablonu bağlanır. Başka modülde uygun şablon seçilir.
- İndirme/üretim yerel dosya işlemidir; bankaya gönderim yapılmaz. Geçersiz veya eksik kişi bilgisi kısmi dosya oluşturmadan açıklanır. Kaynak arşiv ve orijinal XLS değiştirilmez.

## Uygulama
- [x] NPOI HSSF ile orijinal XLS üzerinde kontrollü değer güncellemesi; bağımlılık güvenlik taraması.
- [x] Sentetik test şablonuyla başarısız testler; boş hücreler, sıfır/eksi tutar, kimlik/IBAN, kalan satırlar, toplam/formül, stil ve kaynak hash kontrolleri.
- [x] Şablon seçimi ve ödeme tarihi içeren kompakt WPF banka çıktı penceresi; XML yanına Banka Excel düğmesi.
- [x] Gerçek şablonla bağımsız xlrd/OLE karşılaştırması: izinli hücreler dışındaki değerler ve biçim korunmalı. Workbook dosyasında eski kullanılmayan personel metinleri bırakılmamalı.
- [x] Tam birim testleri ve WPF akışları; yerel 1.0.7 programını hazırla. Gerçek kişi/aylık/arşiv/kasa dosyaları korunur. Bu istek kapsamında GitHub yayını yapılmaz.

Not: Artifact Tool'un XLSX dışa aktarımı özgün BIFF8 XLS korunumu için kullanılmaz; uygulama çıktısı .NET HSSF ile üretilir ve bağımsız okuyucuyla doğrulanır. Kaynakta kullanıcıya ait kişi ve banka bilgileri bulunduğundan şablon/test çıktıları Git dışındadır.


Doğrulama: 292 Release testi; tam WPF akışları; son tek satırlı düğme yerleşimi; gerçek XLS üzerinde bağımsız xlrd ile 772 hücre, font/stil/satır/sütun/boşluk ve OLE yardımcı akış karşılaştırması geçti. 25 eski özel dosyanın SHA-256 değeri korundu. Yerel EXE 1.0.7.0 kuruldu ve gerçek 10 kişilik Temel Eğitim arşivi arayüzden okundu. GitHub yayını yapılmadı.

Kullanıcının son düzenlemesi: ana çıktı düğmeleri PDF, XML ve Banka olarak kısaltıldı. Yedi düğme 1366×768 ekranda tek satırda ve görünür; WPF yerleşim kontrolü geçti.
