# Gece Mavisi + Fluent arayüz denemesi

Bu dal, onaylanan 1440×900 koyu lacivert ekran örneğini baz alır. Ana dalın (`main`) görünümü değiştirilmez.

## Neler uygulandı?

- WPF UI 4.3.0 NuGet paketi eklendi; WPF UI koyu tema ve kontrolleri temel tema kaynaklarından önce yüklenir.
- Ana pencere: 52 px üst başlık çubuğu, 238 px sol menü, kompakt bilgi alanı ve sayfa başlığı.
- Mevcut dokuz ana panel aynı ortak görsel kaynağı kullanır: koyu kartlar, ölçülü altın sarısı vurgular, büyütülmüş alanlar ve kontrol aralıkları.
- Hakediş Hesapla: üç canlı özet kartı (brüt, kesintiler, net), mevcut düzenlenebilir hibrit tablo, resmî çıktılara yönlendirme. Önceden gösterilen altı mali ayrıntı kartı açılır bölümde korunur.
- Dört yardımcı pencerenin koyu arka planları ve form kontrolleri yeni palete uyarlanmıştır.
- Mevcut navigasyon, veri bağlamları, kayıt, hesaplama, PDF, banka ve SGK XML olay işleyicileri korunmuştur.

## Kapsam dışı / dikkat edilmesi gerekenler

- Bu dal henüz gerçek bir Windows masaüstünde kullanıcı tarafından görsel onay almış **nihai sürüm değildir**.
- Kodda tüm eski WPF kontrolleri bir anda WPF UI kontrollerine dönüştürülmedi; tema ve kontrollü görünüm entegrasyonu yapıldı. Tam kontrol geçişi ileride ayrı risk değerlendirmesi gerektirir.
- Önerilen kep + üç kişi + hesap makinesi + belge simgesi için çok çözünürlüklü ICO taslağı ayrıca hazırlandı. Bu dalda mevcut Hakedis.ico henüz değiştirilmedi. Kaynak simgeyi geri dönüşü kolay olacak şekilde, Windows 16/24/32/48/64/128/256 px okunabilirlik onayından sonra entegre etmek gerekir.
- 'Resmî çıktılar' kısayolları doğrudan PDF/XML üretmez, **Çıktı paneline geçer**. Kayıtlı hakediş zorunluluğu korunur.

## Windows kabul testi

1. GitHub Actions > Windows EXE: Windows x64 yayınlama, birim testleri ve paketleme sonucu başarılı olmalı.
2. Windows 10 ve 11 üzerinde 100/125/150% ekran ölçeği, 1366×768 ve 1920×1080 kontrol edilmeli.
3. Dokuz panel arasında geçiş, ESC, klavye odağı ve ekran okuyucu etiketleri denenmeli.
4. Hakediş: hücre düzenleme, anlık yenileme, kaydedilmemiş değişiklik koruması, hakediş oluşturma, geçmiş düzenleme.
5. Banka Excel, PDF çıktı, SGK XML ve kurumsal bilgiler kaydı, eski sürümle aynı sonuçları vermeli.
6. 16 ve 24 piksel ikonların okunabilirliği sınanmalı.

## GitHub Desktop

**Fetch origin** > **Current Branch** > **design/fluent-gece-mavisi-onizleme** > Windows için derle/çalıştır. Tasarım onayından sonra PR birleştirilebilir.
