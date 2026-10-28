namespace HastaneRandevu.Models
{
    public enum RandevuDurumu
    {
        OnayBekliyor,
        Onaylandi,
        Reddedildi,
        IptalEdildi
    }

    public class Randevu
    {
        public int Id { get; set; }

        public int HastaId { get; set; }
        public Hasta? Hasta { get; set; }

        public int CalisanId { get; set; }
        public Calisan? Calisan { get; set; }

        public int HizmetId { get; set; }
        public Hizmet? Hizmet { get; set; }

        public DateTime Baslangic { get; set; }
        public DateTime Bitis { get; set; }
        public int Ucret { get; set; }
        public string Not { get; set; } = "";

        // Durum dışarıdan doğrudan değiştirilemez, aşağıdaki metotlar kullanılır (kapsülleme)
        public RandevuDurumu Durum { get; private set; } = RandevuDurumu.OnayBekliyor;

        public bool Onayla()
        {
            if (Durum != RandevuDurumu.OnayBekliyor)
            {
                return false;
            }
            Durum = RandevuDurumu.Onaylandi;
            return true;
        }

        public bool Reddet()
        {
            if (Durum != RandevuDurumu.OnayBekliyor)
            {
                return false;
            }
            Durum = RandevuDurumu.Reddedildi;
            return true;
        }

        public bool IptalEt()
        {
            if (Durum == RandevuDurumu.Reddedildi || Durum == RandevuDurumu.IptalEdildi)
            {
                return false;
            }
            Durum = RandevuDurumu.IptalEdildi;
            return true;
        }

        // İptal edilen veya reddedilen randevu saati boşaltır
        public bool AktifMi()
        {
            return Durum == RandevuDurumu.OnayBekliyor || Durum == RandevuDurumu.Onaylandi;
        }

        // Verilen saat aralığı bu randevuyla çakışıyor mu?
        public bool CakisiyorMu(DateTime baslangic, DateTime bitis)
        {
            return AktifMi() && baslangic < Bitis && Baslangic < bitis;
        }

        public string DurumYazisi()
        {
            switch (Durum)
            {
                case RandevuDurumu.OnayBekliyor: return "Onay Bekliyor";
                case RandevuDurumu.Onaylandi: return "Onaylandı";
                case RandevuDurumu.Reddedildi: return "Reddedildi";
                default: return "İptal Edildi";
            }
        }
    }
}
