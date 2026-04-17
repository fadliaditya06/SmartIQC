namespace Smart_IQC.Models
{
    public class WorkInstructionModel
    {
        public int id_wi { get; set; }
        public string? file_name { get; set; }
        public string? upload_by { get; set; }
        public DateTime record_date { get; set; }
        public string? wi_type { get; set; }
    }
}
