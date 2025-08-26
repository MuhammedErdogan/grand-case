Özet
Branch’ler: Stabil, ek optimizasyonsuz sürüm main; optimizasyonlu sürüm version/opt-1.

Hedef: İnternet varsa “remote” indirme simülasyonu; yoksa sırasıyla cache → gömülü Resources geri dönüş zinciriyle seviyeyi oynatmak.
Yaklaşım: Mimariyi minimal tuttum; ekstra pattern yok. Dış bağımlılık olarak yalnızca UniTask kullanıldı.
Kullanıcı deneyimi: Yükleme ekranı titremesini (flicker) önlemek ve demo akışını yumuşatmak için +250 ms kontrollü gecikme eklendi.
Veri boyutu optimizasyonu: Level JSON’larında alan adları kısaltıldı; board maskesi hexe çevrilip saklandı. levelId yoksa otomatik üretiliyor.
Build-time taşıma: Lokal simulasyon için gerekli dosyalar AssetPostprocessor ile build aşamasında StreamingAssets altına taşınıyor.

Sürümleme
main: Referans temel uygulama (ek optimizasyonlar yok).
version/opt-1: JSON kısaltma + board hex mask + build-time taşıma + offline/online zinciri + 250 ms UX gecikmesi.

Çalışma Akışı
Remote dene: İnternet erişimi varsa UnityWebRequest ile simüle “remote”’dan JSON çekilir.
Cache’e yaz/oku: Başarılı indirmede veri Application.persistentDataPath’e yazılır; sonraki açılışlarda tekrar indirilmez.
Resources fallback: İnternet yoksa ve cache de yoksa Resources’taki gömülü kopya ile oyun başlatılır.
Döngü: Oyun kazanıldığında ana ekrana dönülür; bir sonraki level hazır edilir. (Bu akış, case study gereksinimleri ile uyumludur ek olarak optimize edilmis versiyonda oynanmış leveller manifest dosyaları silinir.
Veri Formatı ve Optimizasyon
Yaklaşım: Dokumandaki ”Files within ProjectRoot/Levels and Resources can be modified in any way or optimized to improve memory usage and overall performance.” maddesinden yola çıkılarak json verisi uzerinde bazı optimizasyonlar uygulandı.

Kısaltılmış JSON alanları:
L (level), Li (levelId), D (difficulty), G (gridSize), Mh (mask-hex).
Board → Hex: 8×8 gibi kare ızgaralarda bitmask üretip hex string olarak saklıyorum; parse’da tek geçişte bozulmadan geri açılıyor.
levelId politikası: JSON’da Li yoksa level_{L}_updated biçiminde deterministik ID atanıyor. Bu sayede istenirse jsonlardan level id silinebilir ve ek optimizasyon sağlar.

Not 1: Zorluk (D) tek harfe indirgenebilir (ör. e/h), fakat şu aşamada gereksiz parse karmaşıklığı eklememek için string tutuldu; next step olarak listelenebilir.	
Not 2: Veriler 25’li bölümlere ayrılıp toplu indirme ve dağıtım yapılabilirdi alternatif yaklaşım olarak ancak case isterlerinin çok dışına çıkma şüphesiyle tercih edilmedi.

Örnek (optimize edilmiş) :
{ "L": 12, "Li": "level_12_updated", "D": "hard", "G": 8, "Mh": "F3CC_0A7E_..." }

Bağımlılıklar
Cysharp/UniTask: Asenkron iş akışını sadeleştiriyor (tek paket).

Build & Dosya Organizasyonu
Kaynak yerleşimi:
Development sırasında test verileri ProjectRoot/Levels ve Assets/Resources/... altında.
Case dokümanındaki “Files within ProjectRoot/Levels and Resources can be modified/optimized” gereğine paralel olarak veriler optimize edilmiştir.

AssetPostprocessor kuralı:
Build sırasında Resources altındaki belirli klasör(ler) StreamingAssets’e taşınır (lokal indirme simulasyonu için zorunlu).
Assets/Resources/resources_levels_1_500_legacy haricen build’e alınmaz; yalnızca gerekli setler taşınır.

Bütünlük: Dosya kaydında bozulmayı önlemek için atomik yazma ve mevcutsa yeniden indirmeme ilkesi korunur (case gereksinimi). 
Test Senaryoları (Hızlı Doğrulama)
Online senaryo: Ağ açıkken level yükle; kapanışta tekrar gir—yeniden indirme yapılmamalı, cache’den gelmeli.

Offline senaryo (cache var): İnterneti kapat, önceden indirilmiş level’ı cache’den oynat.
Offline senaryo (cache yok): İnternet kapalı ve cache yokken Resources fallback devreye girmeli.

Görsel akış: Yükleme ekranında flicker yok; +250 ms simülasyon devrede.
Veri bütünlüğü: JSON → mask-hex → board dönüşümü ardışık yüklemelerde aynı sonucu vermeli.

Performans Notları
Mobil düşük seviye cihaz hedefi gözetildi (GC baskısını azaltan kısa JSON anahtarları, tek parse geçişi, gereksiz tahsis yok).

Ağ trafiği minimizasyonu: Cache-once yapısı; gereksiz tekrar istek yok (case hedefiyle uyumlu).
Manifest Tabanlı İndirme Takibi
İndirilen dosyaların takibi manifest ile yapılıyor. Per-file “var mı/yok mu” kontrolü yerine tek bir kontrol noktası (manifest) kullanmak hem alışkanlığım hem de aşağıdaki potansiyel geliştirilebilirlik sunan sebeplerden dolayı tercih edildi:
Tutarlılık (consistency): Aynı sürüme ait dosyalar tek sürüm etiketi (örn. contentVersion) altında gruplanır; “karışık sürüm” riski kalkar.

Az network çağrısı: Level bazlı versiyon takibi de yapılması durumunda, CDN’e de manifest klenmesi gerektiği durumda: N adet dosya için N adet HEAD/GET yerine 1 manifest GET + fark kadar indirme. Mobilde RTT ve bağlantıyı uyandırma maliyeti düşer.
Diferansiyel güncelleme: Versiyon veya tarih bazlı takip durumlarında Eski manifest ile yenisi diff’lenip sadece değişen dosyalar indirilebilir.
Rollback ve pinleme: İstenirse istemci belirli bir manifest sürümüne pinlenebilir; sorun çıkarsa önceki sürüme hızlı dönüş.
Depolama yönetimi: Manifest, dosya boyutları ve son erişim zamanlarına göre prune stratejisini yönlendirebilir (oynanmış level kayıtlarının temizlenmesi gibi).
Lokal Erişim: Lokalde de her leveli var olup olmadığını kontrol etmek için bir allocation oluşturmka yerine direkt tek seferde manifest üzerinden gerekli bilgi sağlanabilir.
