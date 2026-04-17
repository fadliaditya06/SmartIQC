using Microsoft.EntityFrameworkCore.Metadata;

namespace Smart_IQC.Models
{
    public class ViewModel
    {
        public List<LayerCheckModel> CheckLayerDetails { get; set; }
        public List<DepartmentModel> AllDepartments { get; set; }
        public List<PICModel> AllPICs { get; set; }
        public List<OpenPointsModel> OpenPointsDetails { get; set; }
    }
}
