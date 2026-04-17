using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Smart_IQC.Function;
using Smart_IQC.Models;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Security.Claims;

namespace Smart_IQC.Controllers
{
    [Authorize]
    public class WristrapController : Controller
    {
        private readonly IConfiguration _configuration;

        public WristrapController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string DbConnection()
        {
            var dbAccess = new DatabaseAccessLayer();
            return dbAccess.ConnectionString;
        }

       [Authorize(Policy = "RequireAny")]
        public IActionResult WristrapCheck()
        {
            return this.CheckSession(() =>
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return View();
            });
        }

        public IActionResult GetWristrap(string week_filter)
        {
            if (string.IsNullOrEmpty(week_filter))
            {
                int currentWeek = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                week_filter = $"{DateTime.Now.Year}-W{currentWeek:D2}";
            }

            var db = new DatabaseAccessLayer();
            DataSet ds1 = db.GetDaysOfWeek(week_filter);
            DataSet ds2 = db.GetWristrapCheck(week_filter); 

            ViewBag.DataSet1 = ds1;
            ViewBag.DataSet2 = ds2;

            return PartialView("_tablewristrap");
        }

        [Authorize(Policy = "RequireAny")]
        [HttpPost]
        public async Task<JsonResult> SaveWristrapCheck(string inspector_sesa_id, int id_wristrap, string result, string remark, string shift, string date, string location)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(DbConnection()))
                {
                    using (SqlCommand cmd = new SqlCommand("SAVE_WRISTRAP_CHECK", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Inspector", inspector_sesa_id ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@IDWristrap", id_wristrap);
                        cmd.Parameters.AddWithValue("@Result", result ?? "OK");
                        cmd.Parameters.AddWithValue("@CheckDate", date);
                        cmd.Parameters.AddWithValue("@Remark", remark ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Shift", shift);
                        cmd.Parameters.AddWithValue("@Location", location);

                        await con.OpenAsync();

                        var executionResult = await cmd.ExecuteScalarAsync();
                        int rowsAffected = executionResult != null ? Convert.ToInt32(executionResult) : 0;

                        return Json(new { status = rowsAffected > 0 ? 1 : 0, message = rowsAffected > 0 ? "Data saved successfully" : "Failed to save data" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = 0, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult GetWeekData()
        {
            List<DateDataModel> data = new List<DateDataModel>();
            string query = "GET_WEEK_DATA";

            using (SqlConnection conn = new SqlConnection(DbConnection()))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new DateDataModel
                            {
                                First_Week_Of_Year = reader["firstWeekOfYear"].ToString(),
                                Current_Week = reader["CurrentWeek"].ToString()
                            });
                        }
                    }
                }
            }
            return Json(data);
        }

        [HttpGet]
        public JsonResult GetSesaId()
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(sesa_id))
            {
                return Json(new { success = true, sesaId = sesa_id });
            }
            return Json(new { success = false, message = "Session expired" });
        }

        [HttpPost]
        public IActionResult GetDataWristrapOK(string inspector, string shift, string check_date)
        {
            if (string.IsNullOrEmpty(inspector) || string.IsNullOrEmpty(shift) || string.IsNullOrEmpty(check_date))
            {
                return BadRequest("The inspector, check_date, and shift parameters cannot be empty");
            }

            var db = new DatabaseAccessLayer();
            List<WristrapCheckModel> datalist = db.GetDataWristrapOK(inspector, shift, check_date);
            return PartialView("_tabledetailOK", datalist);
        }

        [HttpPost]
        public IActionResult GetDataWristrapNG(string location, string shift, string check_date) 
        {
            if (string.IsNullOrEmpty(location) || string.IsNullOrEmpty(shift) || string.IsNullOrEmpty(check_date))
            {
                return BadRequest("The location, check_date, and shift parameters cannot be empty");
            }

            var db = new DatabaseAccessLayer();
            List<WristrapCheckModel> datalist = db.GetDataWristrapNG(location, shift, check_date);
            return PartialView("_tabledetailNG", datalist);
        }

        [HttpPost]
        public async Task<JsonResult> UpdateWristrapCheck(string name, string location, string remark, string result_final, int id_daily)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(DbConnection()))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE_WRISTRAP_CHECK", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ResultFinal", result_final ?? "OK");
                        cmd.Parameters.AddWithValue("@Remark", remark ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@IDDaily", id_daily);

                        await con.OpenAsync();

                        var executionResult = await cmd.ExecuteScalarAsync();
                        int rowsAffected = executionResult != null ? Convert.ToInt32(executionResult) : 0;

                        if (rowsAffected > 0)
                        {
                            return Json(new { success = true, message = "Data updated successfully" });
                        }
                        return Json(new { success = false, message = "No data updated" });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error UpdateWristrap: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult WorkInstruction()
        {
            var db = new DatabaseAccessLayer();
            //var allWorkInstructions = db.GetWorkInstructions();
            var allWorkInstructions = db.GetWorkInstructions().Where(x => x.wi_type == "Wriststrap").ToList();
            return View(allWorkInstructions);
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

                var targetFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "wi");

                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                var fullPathToSave = Path.Combine(targetFolder, storedFileName);

                using (var stream = new FileStream(fullPathToSave, FileMode.Create))
                {
                    workInstructionFile.CopyTo(stream);
                }

                var db = new DatabaseAccessLayer();
                bool success = db.SaveWorkInstruction(storedFileName, type, uploadedBy);

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
                    return Json(new { success = false, message = "Failed to save data to database." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
    }
}