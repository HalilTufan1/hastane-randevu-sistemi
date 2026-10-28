namespace HastaneRandevu.Models
{
    // Görünümlerde kullanılan küçük yardımcı metotlar
    public static class Yardimci
    {
        public static string GunAdi(DayOfWeek gun)
        {
            switch (gun)
            {
                case DayOfWeek.Monday: return "Pazartesi";
                case DayOfWeek.Tuesday: return "Salı";
                case DayOfWeek.Wednesday: return "Çarşamba";
                case DayOfWeek.Thursday: return "Perşembe";
                case DayOfWeek.Friday: return "Cuma";
                case DayOfWeek.Saturday: return "Cumartesi";
                default: return "Pazar";
            }
        }

        public static string DurumRengi(RandevuDurumu durum)
        {
            switch (durum)
            {
                case RandevuDurumu.OnayBekliyor: return "warning";
                case RandevuDurumu.Onaylandi: return "success";
                case RandevuDurumu.Reddedildi: return "danger";
                default: return "secondary";
            }
        }
    }
}
