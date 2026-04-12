using System.ComponentModel.DataAnnotations;
namespace P1F_IQC.Models
{
    public class UserDetailModel
    {
        [Key]
        public string? sesa_id { get; set; }
        public string? name { get; set; }
        public string? level { get; set; }
        public string? role { get; set; }
        public string? email { get; set; }
        public string? apps_id { get; set; }
        public string? dept_id { get; set; }
        public string? manager_sesa_id { get; set; }
        public string? manager_name { get; set; }
        public string? manager_email { get; set; }
        //public string? apps_name { get; set; }
    }
}
