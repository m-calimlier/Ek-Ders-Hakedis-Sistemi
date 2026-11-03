# Bordro ve çıktı düzenlemesi uygulama planı

**Amaç:** Kullanıcının 14 maddelik isteğini mevcut WPF/PDFsharp çıktısına uygulamak.

**Mimari:** Dondurulmuş hakediş sonuçları korunur. Ortak başlık sözlüğü güncellenir, tek rapor tablosunda sütun sırası açıkça tanımlanır. PDF ve yazdırma aynı döndürülmüş başlık geometrisini kullanır. Sayısal çekirdek alanlarının JSON adları değiştirilmez; eski kayıt uyumluluğu korunur.

**Kararlar:** Sayısal başlıklar örnekteki gibi 90° döndürülür; ad/ünvan/kimlik yataydır. Gündüz brüt/birim ücret, GV sabiti ve DV sabiti bordrodan çıkarılır. Damga kalan istisna = Max(0, kayıttaki brüt asgari ücret − kayıttaki brüt ücret). GV kalan istisnası mevcut sıfır alt sınırıyla kalır. SGK oranları arşivdeki gerçek oranlardır; emekli oranları korunur. Ortak istisnaların tek düzenleme noktası normal ortak sabitler kartıdır; emekli kartı bunları salt okunur gösterir. Eski hakedişlerin sabitleri değişmez.

- [x] Sütun sırası/adları, kaldırılan sütunlar, sıfır alt sınırı, beş modülde ortak istisna, arşiv değişmezliği için kırmızı testler.
- [x] Ortak başlıklar ve rapor sütunları; gereksiz yinelenen işveren primi kaldırılır.
- [x] Ortak sabitlerin depolama/ekran senkronizasyonu; SGK ve diğer modüle özel oranlar ayrı kalır.
- [x] PDF ve FixedDocument yazdırmada döndürülmüş mali başlıklar; tek satır/tek sayfa koruması.
- [x] Detayda talep edilecek ödenekler kartı: 01.04 kişi ücreti = brüt, 02.04 SGK = işveren primi; toplam = tahakkuk.
- [x] PDF başarı bildirimi kapandıktan sonra tam çıktı klasörünü aç; hata halinde klasör açılmaz. Sentetik WPF akış testi.
- [x] Birim testleri ve WPF akışları; sentetik DYK/icra kombinasyonları ve gerçek arşivle PDF üretip bağımsız render incelemesi.
- [x] Tam veri/EXE yedeği, yerel güncel program, gerçek verilerle açılış ve hash kontrolü. GitHub sürüm yayını bu istekte yoktur.

Doğrulama: 305 Debug ve Release birim testi; tüm modüllerde WPF akışları; PDF bildirimi/klasör sırası ve ödenek kartı; altı bordronun bağımsız PDFium/pdfplumber incelemesi (tek sayfa, düşey başlıklar, taşmayan karakterler); gerçek 10 kişilik arşivden üç PDF ve kaynak hash eşitliği geçti.
