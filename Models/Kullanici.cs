using System.Security.Cryptography;
using System.Text;

namespace HastaneRandevu.Models
{
    // Tüm kullanıcıların ortak özellikleri bu sınıfta.
    // abstract olduğu için doğrudan "new Kullanici()" yapılamaz, alt sınıflar kullanılır.
    public abstract class Kullanici
    {
        public int Id { get; set; }
        public string Ad { get; set; } = "";
        public string Soyad { get; set; } = "";
        public string KullaniciAdi { get; set; } = "";

        // Şifre açık şekilde değil, özet (hash) olarak saklanır
        public string SifreHash { get; private set; } = "";

        // Her alt sınıf kendi rolünü söyler (çok biçimlilik)
        public abstract string Rol { get; }

        // Ekranda gösterilecek isim. Çalışan sınıfı bunu değiştiriyor (override).
        public virtual string GorunenAd
        {
            get { return Ad + " " + Soyad; }
        }

        // Bu kullanıcı verilen kurumu yönetebilir mi? Normalde hayır.
        // Yonetici ve SistemYoneticisi sınıfları bunu kendine göre değiştirir (override).
        public virtual bool KurumuYonetebilirMi(int kurumId)
        {
            return false;
        }

        public void SifreBelirle(string sifre)
        {
            SifreHash = Hashle(sifre);
        }

        public bool SifreDogruMu(string sifre)
        {
            return SifreHash == Hashle(sifre);
        }

        private static string Hashle(string sifre)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(sifre));
            return Convert.ToHexString(bytes);
        }
    }

    public class Hasta : Kullanici
    {
        public string Telefon { get; set; } = "";

        public override string Rol
        {
            get { return "Hasta"; }
        }
    }

    // Doktor ve diğer sağlık personeli (hemşire, laborant...) bu sınıfla tutulur.
    // Unvan alanı "Dr.", "Hemşire" gibi bilgiyi taşır.
    public class Calisan : Kullanici
    {
        public string Unvan { get; set; } = "";
        public string Uzmanlik { get; set; } = "";

        public int SubeId { get; set; }
        public Sube? Sube { get; set; }

        // Çalışanın verdiği hizmetler (çoka çok ilişki)
        public List<Hizmet> Hizmetler { get; set; } = new List<Hizmet>();

        public override string Rol
        {
            get { return "Calisan"; }
        }

        public override string GorunenAd
        {
            get { return Unvan + " " + Ad + " " + Soyad; }
        }
    }

    // Bir kurumun (hastane veya klinik) yöneticisi. Sadece kendi kurumunu yönetir.
    public class Yonetici : Kullanici
    {
        public int KurumId { get; set; }
        public Kurum? Kurum { get; set; }

        public override string Rol
        {
            get { return "Yonetici"; }
        }

        public override bool KurumuYonetebilirMi(int kurumId)
        {
            return KurumId == kurumId;
        }
    }

    // Üst düzey yönetici (admin). Bütün kurumları yönetir, her şeyi ekleyip silebilir.
    public class SistemYoneticisi : Kullanici
    {
        public override string Rol
        {
            get { return "Admin"; }
        }

        public override bool KurumuYonetebilirMi(int kurumId)
        {
            return true;
        }
    }
}
