# Hastane / Klinik Randevu Sistemi

Nesneye Dayalı Programlama dersi proje ödevi (Seçenek A).
ASP.NET Core MVC ve Entity Framework Core (SQLite) ile yapıldı.

## Çalıştırma

.NET 10 SDK kurulu olmalı.

```bash
dotnet run
```

Sonra tarayıcıda `http://localhost:5282` adresine gidin.

İlk çalıştırmada `hastane.db` dosyası oluşturulur ve örnek veriler eklenir.
Verileri sıfırlamak için uygulamayı kapatıp `hastane.db` dosyasını silin.

## Deneme hesapları

| Rol | Kullanıcı adı | Şifre |
|---|---|---|
| Üst düzey yönetici (Admin) | admin | admin123 |
| Sakarya Şifa Hastanesi yöneticisi | yonetici | yonetici123 |
| Gülüş Diş Kliniği yöneticisi | klinik | klinik123 |
| Doktor / Personel | dr.ayse, dr.mehmet, dr.burak, dt.selin, zeynep, can, omer | calisan123 |
| Hasta | hasta, hasta2 ... hasta15 | hasta123 |

## Roller ve yetkiler

| İşlem | Admin | Kurum yöneticisi | Doktor / Personel | Hasta |
|---|---|---|---|---|
| Kurum ekleme, düzenleme, silme | ✓ | | | |
| Kurum yöneticisi ekleme, silme | ✓ | | | |
| Hasta silme, randevu silme | ✓ | | | |
| Şube, bölüm, hizmet ekleme / düzenleme / silme | ✓ (tüm kurumlar) | ✓ (sadece kendi kurumu) | | |
| Çalışan ekleme / düzenleme / silme, çalışma saatleri | ✓ (tüm kurumlar) | ✓ (sadece kendi kurumu) | | |
| Randevu onaylama / reddetme | ✓ (tüm kurumlar) | ✓ (sadece kendi kurumu) | ✓ (kendi randevuları) | |
| Randevu alma ve iptal | | | | ✓ |

Her kurumun kendi yöneticisi vardır. Kurum yöneticisi başka kurumların bilgilerini göremez.
Bu kontrol `Kullanici.KurumuYonetebilirMi()` metoduyla yapılır: `Yonetici` sınıfı sadece kendi
kurumu için, `SistemYoneticisi` (admin) sınıfı her kurum için `true` döndürür.

Bir kaydı silmek ona bağlı kayıtları da siler (örneğin şube silinirse bölümleri, hizmetleri,
çalışanları ve randevuları da silinir). Silmeden önce onay sorulur.

## Özellikler

- **Admin:** Kurum ekler, düzenler, siler. Kurumlara yönetici atar. Hasta ve randevu silebilir.
- **Kurum yöneticisi:** Kendi kurumuna şube, bölüm ve hizmet ekler, düzenler, siler. Çalışan ekler,
  düzenler, siler; çalışanın hizmetlerini ve çalışma saatlerini girer. Kurumunun randevularını onaylar
  veya reddeder.
- **Doktor / Personel:** Kendi randevularını görür, onaylar veya reddeder.
- **Hasta:** Kayıt olur, hizmet ve doktor seçip boş saatlerden randevu alır, randevusunu iptal eder.

Çalışma saati eklenirken kontrol edilenler: saatler şubenin açık olduğu saatler içinde olmalı ve
çalışanın aynı gün girilmiş başka bir çalışma saatiyle çakışmamalı.

Randevu alınırken kontrol edilenler (`Services/RandevuServisi.cs`):
- Geçmiş bir saate randevu alınamaz
- Çalışan seçilen hizmeti vermeli
- Şube o saatte açık olmalı
- Çalışan o saatte çalışıyor olmalı
- Çalışanın veya hastanın aynı saatte başka randevusu olmamalı (çakışma kontrolü)

Yeni randevu "Onay Bekliyor" durumunda oluşur, doktor veya yönetici onaylar.

## Klasörler

- `Models` : Sınıflar (Kullanici, Hasta, Calisan, Yonetici, SistemYoneticisi, Kurum, Sube, Bolum, Hizmet, UygunlukSaati, Randevu)
- `Data` : Veritabanı bağlantısı (UygulamaDbContext) ve örnek veriler (OrnekVeri)
- `Services` : Randevu kuralları (RandevuServisi)
- `Controllers` : Sayfaların işlemleri (Home, Hesap, Hasta, Calisan, Yonetim, Admin)
- `Views` : Sayfalar (cshtml)

## Kullanılan NDP kavramları

- **Sınıf ve nesne:** Her tablo için bir sınıf var (Kurum, Sube, Hizmet, Randevu...).
- **Kalıtım:** `Hasta`, `Calisan`, `Yonetici` ve `SistemYoneticisi` sınıfları `Kullanici` sınıfından türetildi.
- **Soyut sınıf:** `Kullanici` abstract bir sınıf, doğrudan nesnesi oluşturulamaz.
- **Çok biçimlilik:** `Rol` özelliği her alt sınıfta farklı değer döndürür. `KurumuYonetebilirMi()`
  metodu kurum yöneticisinde ve admin'de farklı çalışır. `GorunenAd` özelliği
  `Calisan` sınıfında override edilip unvan eklenir (örn. "Uzm. Dr. Ayşe Yılmaz").
- **Kapsülleme:** Randevunun `Durum` bilgisi dışarıdan değiştirilemez (`private set`), sadece
  `Onayla()`, `Reddet()`, `IptalEt()` metotlarıyla değişir. Şifre açık tutulmaz, `SifreBelirle()` ile
  hash'lenip saklanır.
