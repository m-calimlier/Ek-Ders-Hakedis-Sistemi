# Tek satırlı bordro
Her öğretmen tek birleşik tabloda bir satırdır. DYK saatleri tüm satırlarda sıfırsa DYK saat toplamı, saat ücreti ve brüt sütunları gizlenir. Tüm icra girdileri sıfırsa icra sütunu gizlenir. SGK brüt başlığı “SGK’ya Esas Brüt Ücret”; süregelen matrah kaldırılır. Eski GV matrahı yerine “Kalan Gelir Vergisi İstisnası” başlığında max(0, kayıtlı matrah - hakedişteki GV istisnası) gösterilir. Bu, kullanıcının istediği isim/formüldür; mali hesap yeniden yazılmaz. Ana başlık altında yalnız Dairesi ve İlgili Ay, sola yaslıdır. PDF ve yazdırma aynı düzeni kullanır. Veriler korunur; yedek ve özet doğrulaması sonrası uygulama açılır.

Son kullanıcı düzeltmesi: gündüz birim ücret sütunu her zaman gizlenir, DYK yoksa gündüz saat toplamı da gizlenir. GV Matrahı ayrı sütun olarak korunur. Kalan GV İstisnası ayrı sütunda min(0, matrah - kayıtlı istisna) olarak gösterilir (negatif değerler dahil). Önceki max formülü geçersizdir.

Son düzeltme (negatif gösterilmez): Kalan GV İstisnası max(0, kayıtlı istisna - matrah). Üstteki negatif/min formülü artık geçersizdir.
