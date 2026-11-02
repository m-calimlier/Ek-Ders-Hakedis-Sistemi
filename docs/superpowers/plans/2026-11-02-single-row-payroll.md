# Tek satırlı bordro uygulama planı
- [x] Tek tablo, isteğe bağlı sütunlar, GV eşiği ve toplamları için başarısız testler.
- [x] PaymentReports tablo yapısını ve LayoutPayroll bilgi hizasını güncelle.
- [x] PDF/yazdırma sınırları, DYK/icra kombinasyonları ve gerçek kayıt kopyalarının görsel kontrolü.
- [x] Birim ve WPF testleri, canlı veri yedeği, EXE güncellemesi, içerik özetleri ve açılış doğrulaması.

Doğrulama: 297 birim testi; tam WPF smoke; dört DYK/icra kombinasyonunda tek sayfa/tek satır ve hücre sınırları; 11 kişilik gerçek kayıt kopyasında PDF render/görsel kontrol. Canlı 71 dosyanın özeti değişmedi; veri ve EXE yedeği alındı; 1.0.8 yerinde kuruldu.

1.0.9 düzeltmesi: GV matrahı geri getirildi; kalan istisna max(0,istisna-matrah). Gündüz saat ücreti daima, gündüz saat toplamı DYK yoksa gizli. 299 Release testi; dört DYK/icra PDF/yazdırma senaryosu; 11 kişilik gerçek kopyada görsel kontrol. 74 canlı dosya özeti değişmedi; veri/EXE yedeği ve yerinde güncelleme tamamlandı.
