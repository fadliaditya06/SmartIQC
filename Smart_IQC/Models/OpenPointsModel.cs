using System;

namespace Smart_IQC.Models
{
    public class OpenPointsModel
    {
        public string Report_ID { get; set; }
        public string Category { get; set; }
        public string Supplier_Name { get; set; }
        public string Part_Number { get; set; }
        public string Critical_Part_Status { get; set; }
        public DateTime Record_Date { get; set; }
        public string Question { get; set; }
        public string PicSesaId { get; set; }
        public string PIC_Name { get; set; }
        public int PIC_Status { get; set; }
        public string Comment { get; set; }
        public string PIC_Action { get; set; }
        public string File_Action_Image { get; set; }
        public string Due_Date_Status { get; set; }
        public DateTime? Due_Date { get; set; }
        public string Requirement { get; set; } 
        public string File_Image { get; set; }
        public string Dept_Name { get; set; }
        public string Status { get; set; }

    }
}
