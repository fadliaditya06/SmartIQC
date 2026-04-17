namespace Smart_IQC.Models
{
    public class ChecklistDataModel
    {
        public string? Commodity_ID { get; set; }
        public string? Supplier_ID { get; set; }
        public string? Location { get; set; }
        public string? Defect_Description { get; set; }
        public string? Part_Number { get; set; }
        public string? Critical_Part_Status { get; set; }
        public string? Audit_By { get; set; }
        public string? Id_Issue { get; set; }
        public List<ChecklistDetailModel>? Checklist_Details { get; set; }
    }
}
