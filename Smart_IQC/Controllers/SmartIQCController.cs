using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using P1F_IQC.Function;
using P1F_IQC.Models;

namespace P1F_IQC.Controllers
{
    [Authorize]
    public class SmartIQCController : Controller
    {
        private readonly DatabaseAccessLayer _dal;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SmartIQCController(DatabaseAccessLayer dal, IWebHostEnvironment webHostEnvironment)
        {
            _dal = dal;
            _webHostEnvironment = webHostEnvironment;
        }

       [Authorize(Policy = "RequireAny")]
        public IActionResult Dashboard()
        {
            return View();
        }

       [Authorize(Policy = "RequireAny")]
        public IActionResult Checklist()
        {
            var viewModel = new ViewModel();

            viewModel.AllDepartments = _dal.GetAllDepartments();
            viewModel.AllPICs = _dal.GetAllPICs();

            return View(viewModel);
        }

       [Authorize(Policy = "RequireAny")]
        public IActionResult AllPoints()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ShowAllPoints(string date_from, string date_to, string cell_name, string family)
        {
            var viewModel = new ViewModel();
            viewModel.OpenPointsDetails = _dal.GetAllPoints(date_from, date_to, cell_name, family);

            return PartialView("~/Views/SmartIQC/_TableAllPoints.cshtml", viewModel);
        }

        public IActionResult WorkInstruction()
        {
            var allWorkInstructions = _dal.GetWorkInstructions().Where(x => x.wi_type == "IQC").ToList();
            return View(allWorkInstructions);
        }

       [Authorize(Policy = "RequireAny")]
        public IActionResult Capability()
        {
            var model = _dal.GetCapability();
            return View(model);
        }

        [Authorize(Policy = "RequireAny")]
        public IActionResult DefectLibrary()
        {
            var defects = _dal.GetDefectLibrary();
            return View(defects);
        }

        [Authorize(Policy = "RequireAny")]
        public IActionResult QualityPolicy()
        {
            return View();
        }

        [HttpGet]
        [Route("SmartIQC/api/GetIqcStatusByCommodity")]
        public IActionResult GetIqcStatusByCommodity(string period, string dateFrom, string dateTo)
        {
            if (string.IsNullOrEmpty(period) || (period.ToLower() != "weekly" && period.ToLower() != "monthly" && period.ToLower() != "yearly"))
            {
                return BadRequest(new { success = false, message = "The filter period is invalid. Please use 'weekly', 'monthly', atau 'yearly'." });
            }

            try
            {
                ChartData chartData = _dal.GetIqcChartData(period, dateFrom, dateTo);

                if (chartData == null)
                {
                    chartData = new ChartData { Categories = new List<string>(), SeriesData = new List<ChartSeries>() };
                }

                return Json(chartData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetIqcStatusByCommodity: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving chart data: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult ShowLayerCheck(string partNumber)
        {
            var viewModel = new ViewModel();
            viewModel.CheckLayerDetails = _dal.GetFilteredChecklist(partNumber);
            return PartialView("~/Views/SmartIQC/_LayerCheckTable.cshtml", viewModel);
        }

       [Authorize(Policy = "RequireAny")]
        public IActionResult OpenPoints()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ShowOpenPoints0(string date_from, string date_to, string cell_name, string family)
        {
            var viewModel = new ViewModel();
            viewModel.OpenPointsDetails = _dal.GetOpenPoints();

            return PartialView("~/Views/SmartIQC/_TableOpenPoints.cshtml", viewModel);
        }

        [HttpPost]
        public IActionResult UpdateStatus(string unique_reff, string status)
        {
            try
            {
                _dal.UpdateSampleResultStatus(unique_reff, status);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetSuppliers(string term)
        {
            var suppliers = _dal.SearchSupplier(term);
            return Json(suppliers);
        }

        [HttpGet]
        public IActionResult GetCategories(string term) 
        {
            var categories = _dal.SearchCategory(term);

            var results = categories.Select(c => new
            {
                id = c.Commodity,      
                text = c.Commodity_Name
            }).ToList();

            return Json(new { results = results });
        }

        [HttpGet]
        public JsonResult GetIssueCategories(string term)
        {
            var issueCategories = _dal.SearchIssueCategory(term);
            return Json(issueCategories);
        }

        [HttpGet]
        public JsonResult GetPartNumbers(string term)
        {
            var partNumbers = _dal.SearchPartNumber(term);
            return Json(partNumbers);
        }

        [HttpPost]
        public JsonResult SaveChecklist([FromBody] ChecklistDataModel data)
        {

            if (data == null)
            {
                return Json(new { success = false, message = "The data sent is invalid." });
            }

            if (data.Checklist_Details == null || !data.Checklist_Details.Any())
            {
                return Json(new { success = false, message = "No questions to save." });
            }

            bool result = _dal.SaveFullChecklist(data);

            if (result)
            {
                return Json(new { success = true, message = "Checklist successfully saved!" });
            }
            else
            {
                return Json(new { success = false, message = "Failed to save checklist." });
            }
        }

        [HttpGet]
        public JsonResult GetDepartmentFilter(string term)
        {
            var departments = _dal.SearchDepartment(term);
            return Json(departments);
        }

        [HttpPost]
        public JsonResult UploadNOKImage()
        {
            try
            {
                var file = Request.Form.Files[0];

                if (file != null && file.Length > 0)
                {
                    var fileExtension = Path.GetExtension(file.FileName);

                    var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                    var fileName = $"image_nok_{timeStamp}{fileExtension}";

                    var directoryName = "wwwroot/uploads/nok_images";
                    var savePath = Path.Combine(Directory.GetCurrentDirectory(), directoryName, fileName);

                    var saveDir = Path.GetDirectoryName(savePath);
                    if (!Directory.Exists(saveDir))
                    {
                        Directory.CreateDirectory(saveDir);
                    }

                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    var relativePath = fileName; 

                    return Json(new { success = true, filePath = relativePath });
                }
                return Json(new { success = false, message = "No file received." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "File upload failed: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateTemporaryStatus([FromBody] ChecklistDataModel data)
        {
            try
            {
                _dal.SaveTemporaryStatus(data);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("TEMP SAVE FAILED: " + ex.ToString());
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetPICs(string term)
        {
            try
            {
                var picList = _dal.SearchAllPICs(term);
                return Json(new { results = picList });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost]
        public IActionResult UpdatePIC(string reportID, string newPicSesaId)
        {
            try
            {
                //bool success = _dal.UpdatePIC(partNumber, newPicSesaId);
                bool success = _dal.UpdatePIC(reportID, newPicSesaId);

                if (success)
                {
                    return Json(new { success = true, message = "PIC successfully updated." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to find or update the record." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred on the server: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateAction(string reportID, string picAction, string dueDate, IFormFile imageFile)
        {
            try
            {
                string imagePath = null;
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileExtension = Path.GetExtension(imageFile.FileName);
                    var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                    // Nama file default untuk file_action_image
                    var fileName = $"image_action_{timeStamp}{fileExtension}";

                    var directoryName = "wwwroot/uploads/actions";
                    var savePath = Path.Combine(Directory.GetCurrentDirectory(), directoryName, fileName);

                    var saveDir = Path.GetDirectoryName(savePath);
                    if (!Directory.Exists(saveDir))
                    {
                        Directory.CreateDirectory(saveDir);
                    }

                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }

                    imagePath = fileName;
                }

                bool success = _dal.SaveAction(reportID, picAction, dueDate, imagePath);

                if (success)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update data in database." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred on the server: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult VerifyOpenPoint(string reportID)
        {
            if (string.IsNullOrEmpty(reportID))
            {
                return Json(new { success = false, message = "Report ID cannot be empty." });
            }

            try
            {
                bool isVerified = _dal.VerifyOpenPoint(reportID);

                if (isVerified)
                {
                    return Json(new { success = true, message = "Open Point verified successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to verify the Open Point in the database. Please check server logs." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Error during VerifyOpenPoint: {ex.Message}");
                return Json(new { success = false, message = "An internal error occurred." });
            }
        }

        [Authorize(Policy = "RequireAdmin")]
        [HttpPost]
        public IActionResult UploadWorkInstruction(IFormFile workInstructionFile, string type)
        {
            if (workInstructionFile == null || workInstructionFile.Length == 0)
            {
                return Json(new { success = false, message = "No files selected." });
            }

            var extension = Path.GetExtension(workInstructionFile.FileName).ToLowerInvariant();
            if (extension != ".pdf")
            {
                return Json(new { success = false, message = "Only PDF files are permitted." });
            }

            try
            {
                var uploadedBy = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                var storedFileName = $"WI-{type}-{timestamp}{extension}";
                var targetFolder = "uploads/wi";
                var targetPath = Path.Combine(_webHostEnvironment.WebRootPath, targetFolder);

                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }

                var fullPathToSave = Path.Combine(targetPath, storedFileName);
                using (var stream = new FileStream(fullPathToSave, FileMode.Create))
                {
                    workInstructionFile.CopyTo(stream);
                }

                bool success = _dal.SaveWorkInstruction(storedFileName, type, uploadedBy);

                if (success)
                {
                    return Json(new { success = true, message = "Work Instruction successfully uploaded." });
                }
                else
                {
                    if (System.IO.File.Exists(fullPathToSave))
                    {
                        System.IO.File.Delete(fullPathToSave);
                    }
                    return Json(new { success = false, message = "Failed to save file." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred on the server: " + ex.Message });
            }
        }

        // Logic for Modal Data Details in Open Points
        [HttpGet]
        public IActionResult GetOpenPointDetails(string reportID)
        {
            if (string.IsNullOrEmpty(reportID))
            {
                return Json(new { success = false, message = "Report ID parameter is missing." });
            }

            try
            {
                var detail = _dal.GetOpenPointDetailByReportID(reportID);

                if (detail != null)
                {
                    if (detail.PIC_Status >= 1)
                    {
                        return Json(new
                        {
                            success = false,
                            isCompleted = true, 
                            message = "This open point has already been successfully completed by PIC."
                        });
                    }

                    return Json(new { success = true, data = detail });
                }
                else
                {
                    return Json(new { success = false, message = $"Open Point ID {reportID} not found or data structure is null." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in GetOpenPointDetails for ID {reportID}: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Failed to retrieve Open Point details. Check server logs for database error." });
            }
        }
    }
}