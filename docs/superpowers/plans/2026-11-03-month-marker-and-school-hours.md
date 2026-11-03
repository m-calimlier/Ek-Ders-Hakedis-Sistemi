# Aylık satır işareti ve kurum saatleri

Kullanıcının netleştirdiği kural: satır rengi yalnız işe başlama tarihinin AYI ile seçili AY eşleştiğinde açık yeşildir. Yıl ve ders/kesinti girişi koşul değildir. Eylül başlangıcı Eylül'de yeşil, Ekim'de normaldir. İşaret yalnız HibritGrid ekranındadır, rapora taşınmaz.

Kalan Gelir Vergisi İstisnası toplam hücresi, Damga Vergisi İstisna Kalan gibi boş bırakılır. IBAN Kontrol sütunu yalnız hibrit tablodan kaldırılır; doğrulama ve IBAN alanı korunur.

Yeni kurum saatinin boş görünmesi için tekrarlanabilen hata: veri girişi ve hesaplama farklı uygun modüllerdeyken mevcut önizleme senkronizasyonu yalnız aynı modülü kabul ediyordu. Aynı ödeme döneminde açık bir kurum geçişi varsa yalnız kurum saatleri paylaşılır; hedef öğretmenin modül izdüşümü, kayıt sürümü, DYK ve icra girdileri korunur. Önizleme dosya yazmaz ve başka döneme taşınmaz. Hedefin kendi kayıtlı girdisi varsa başka modülün kaydı onu kaydedilmiş saymaz.

- [x] Ay/yıl ve saat girişinden bağımsız renk kuralı, sonraki ay sıfırlanma ve tarih güncellemesi testleri.
- [x] İki kurum saatinin canlı ve kayıt sonrası paylaşılması; modül, kayıt sürümü, diğer girdiler ve dönem ayrımı testleri.
- [x] 314 birim testi; yeşil/beyaz ekranlar, IBAN kontrol sütununun yokluğu ve iki kurum hücresinin görünümü.
- [x] Tüm WPF akışları ve gerçek arşiv PDF kontrolü.
- [x] Güncel program, özel veri yedeği ve gerçek kayıtlarla açılış.

Son doğrulama: 314 birim testi, tüm WPF akışları, gerçek 10 kişilik arşivden üç PDF ve kaynak hash eşitliği geçti. PDF'de ekranın yeşil rengi yok; iki kalan istisna sütununun toplamı boş. Yerel paket 1.0.12.
