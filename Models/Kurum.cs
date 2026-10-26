namespace HastaneRandevu.Models
{
    // Hastane veya klinik. Hastanenin birden çok şubesi olabilir, kliniğin tek şubesi olur.
    public class Kurum
    {
        public int Id { get; set; }
        public string Ad { get; set; } = "";
        public string Tur { get; set; } = "Hastane"; // "Hastane" veya "Klinik"

        public List<Sube> Subeler { get; set; } = new List<Sube>();

        public bool SubeEklenebilirMi()
        {
            if (Tur == "Klinik" && Subeler.Count >= 1)
            {
                return false;
            }
            return true;
        }
    }

    public class Sube
    {
        public int Id { get; set; }
        public string Ad { get; set; } = "";
        public string Adres { get; set; } = "";

        // Şube Pazartesi-Cumartesi bu saatler arasında açık, Pazar kapalı.
        public TimeSpan AcilisSaati { get; set; } = new TimeSpan(8, 0, 0);
        public TimeSpan KapanisSaati { get; set; } = new TimeSpan(17, 0, 0);

        public int KurumId { get; set; }
        public Kurum? Kurum { get; set; }

        public List<Bolum> Bolumler { get; set; } = new List<Bolum>();

        public bool AcikMi(DateTime baslangic, DateTime bitis)
        {
            if (baslangic.DayOfWeek == DayOfWeek.Sunday)
            {
                return false;
            }
            return baslangic.TimeOfDay >= AcilisSaati && bitis.TimeOfDay <= KapanisSaati;
        }
    }

    public class Bolum
    {
        public int Id { get; set; }
        public string Ad { get; set; } = "";

        public int SubeId { get; set; }
        public Sube? Sube { get; set; }

        public List<Hizmet> Hizmetler { get; set; } = new List<Hizmet>();
    }

    public class Hizmet
    {
        public int Id { get; set; }
        public string Ad { get; set; } = "";
        public int SureDakika { get; set; }
        public int Ucret { get; set; } // TL

        public int BolumId { get; set; }
        public Bolum? Bolum { get; set; }
    }
}
