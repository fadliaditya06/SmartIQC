namespace Smart_IQC.Models
{
    public class THCheckModel
    {
        public string? id { get; set; }
        public string? insp_date { get; set; }
        public string? insp_trip { get; set; }
        public string? temperature_value { get; set; }
        public string? temperature_status { get; set; }
        public string? humidity_value { get; set; }
        public string? humidity_status { get; set; }
        public string? audit_status { get; set; }
        public string? user_id { get; set; }
        public string? location { get; set; }
        public string? humminmax { get; set; }
        public string? tempminmax { get; set; }
        public string? remark { get; set; }
    }
}
