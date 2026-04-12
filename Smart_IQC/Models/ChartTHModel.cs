namespace P1F_IQC.Models
{
    public class ChartTHModel
    {
        public string? insp_date { get; set; }
        public string? location { get; set; }
        public int? ok { get; set; }
        public int? nok { get; set; }
        public string? temperature_value { get; set; }
        public string? humidity_value { get; set; }
        public string? temperature_status { get; set; }
        public string? humidity_status { get; set; }
    }
}
