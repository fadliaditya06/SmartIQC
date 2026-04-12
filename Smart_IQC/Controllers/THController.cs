using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient; 
using P1F_IQC.Function; 
using P1F_IQC.Models; 
using System;
using System.Data;
using System.Security.Claims;

namespace P1F_IQC.Controllers
{
    [Authorize]
    public class THController : Controller
    {
        private string DbConnection()
        {
            var dbAccess = new DatabaseAccessLayer();
            string dbString = dbAccess.ConnectionString;
            return dbString;
        }

       [Authorize(Policy = "RequireAny")]
        public IActionResult DashboardTH()
        {
            var db = new DatabaseAccessLayer();
            ViewBag.LocationList = db.GetThLocations();
            return View();
        }

       [Authorize(Policy = "RequireAny")]
        public IActionResult THCheck()
        {
            var db = new DatabaseAccessLayer();
            ViewBag.LocationList = db.GetThLocations();
            return View();
        }

       [Authorize(Policy = "RequireAny")]
        public IActionResult THStatus()
        {
            var d = DateTime.Now;
            int currentWeek = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                d, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            ViewBag.CurrentWeek = $"{d.Year}-W{currentWeek:D2}";

            var db = new DatabaseAccessLayer();
            ViewBag.LocationList = db.GetThLocations();
            return View();
        }

        // Fungsi untuk mengambil data pengecekan berdasarkan lokasi dan tanggal tertentu untuk ditampilkan ke dalam menu TH Status
        [HttpPost] 
        public IActionResult GetDataTHCheck(string location, string insp_date)
        {
            var db = new DatabaseAccessLayer();
            List<THCheckModel> dataList = db.GetDataTHCheck(location, insp_date);

            return PartialView("_tableDataTHCheckDetail", dataList);
        }

        // Fungsi untuk mengambil data mingguan yang akan ditampilkan pada detail tabel TH Status
        public IActionResult GetDashTHCheck(string week_filter)
        {
            if (string.IsNullOrEmpty(week_filter))
            {
                int currentWeek = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                    DateTime.Now, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

                week_filter = $"{DateTime.Now.Year}-W{currentWeek:D2}";
            }

            var db = new DatabaseAccessLayer();

            DataSet ds1 = db.GetDaysOfWeek(week_filter);
            DataSet ds2 = db.GetDashTHCheck(week_filter);

            ViewBag.DataSet1 = ds1;
            ViewBag.DataSet2 = ds2;

            return PartialView("_tableTHCheck");
        }

        // Fungsi untuk memvalidasi apakah data sudah diinput atau belum serta mengambil standar batasan min/max dari database
        [HttpGet]
        public IActionResult CheckData(string location)
        {
            int rowsAffected = 0;
            string temperatureValue = "";
            string humidityValue = "";

            decimal tempMinDB = 0, tempMaxDB = 0, humMinDB = 0, humMaxDB = 0;

            string currentDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
            TimeSpan currentTime = DateTime.Now.TimeOfDay;

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnection()))
                {
                    conn.Open();

                    string masterQuery = "SELECT tempmin, tempmax, hummin, hummax FROM mst_dc_th WHERE location = @loc";
                    using (SqlCommand masterCmd = new SqlCommand(masterQuery, conn))
                    {
                        masterCmd.Parameters.AddWithValue("@loc", location);
                        using (SqlDataReader masterReader = masterCmd.ExecuteReader())
                        {
                            if (masterReader.Read())
                            {
                                tempMinDB = Convert.ToDecimal(masterReader["tempmin"]);
                                tempMaxDB = Convert.ToDecimal(masterReader["tempmax"]);
                                humMinDB = Convert.ToDecimal(masterReader["hummin"]);
                                humMaxDB = Convert.ToDecimal(masterReader["hummax"]);
                            }
                        }
                    }

                    TimeSpan targetTime = new TimeSpan(15, 0, 0);
                    int insp_trip = (currentTime >= targetTime) ? 2 : 1;

                    string query = "SELECT temperature_value, humidity_value FROM iqc_dc_temperature " +
                                   "WHERE insp_date = @date AND location = @loc AND insp_trip = @trip";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@date", currentDate);
                        cmd.Parameters.AddWithValue("@loc", location);
                        cmd.Parameters.AddWithValue("@trip", insp_trip);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                rowsAffected = 1;
                                temperatureValue = reader["temperature_value"].ToString();
                                humidityValue = reader["humidity_value"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error CheckData: " + ex.Message);
                return StatusCode(500, "Internal server error: " + ex.Message);
            }

            return Json(new
            {
                rowsAffected,
                temperatureValue,
                humidityValue,
                tempMinDB,
                tempMaxDB,
                humMinDB,
                humMaxDB
            });
        }

        // Fungsi untuk menyimpan hasil inputan temperature dan humidity ke dalam database
        [HttpPost]
        public IActionResult AddTH(string temp, string hum, string status, string temp_status, string hum_status, string location)
        {
            int rowsAffected = 0;
            var user = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnection()))
                {
                    using (SqlCommand cmd = new SqlCommand("ADD_TH_RECORD", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@TemperatureValue", temp ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@TemperatureStatus", temp_status ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@HumidityValue", hum ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@HumidityStatus", hum_status ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@AuditStatus", status ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@UserID", user ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Location", location ?? (object)DBNull.Value);

                        conn.Open();
                        rowsAffected = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }

                return Json(new { rowsAffected, message = location, redirectTo = Url.Action("THStatus", "TH") });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }

        // Fungsi untuk mengambil data OK dan NOK yang akan ditampilkan dalam bentuk grafik (pie chart)
        [HttpGet]
        public IActionResult CHART_IQCROOM(string date_from, string date_to, string loc)
        {
            var data = new List<ChartTHModel>();
            using (SqlConnection conn = new SqlConnection(DbConnection()))
            {
                var command = new SqlCommand("CHART_ALLTH", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@date_from", (object)date_from ?? DBNull.Value);
                command.Parameters.AddWithValue("@date_to", (object)date_to ?? DBNull.Value);
                command.Parameters.AddWithValue("@loc", (object)loc ?? DBNull.Value);

                conn.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        data.Add(new ChartTHModel
                        {
                            ok = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader[1]),
                            nok = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader[2])
                        });
                    }
                }
            }
            return Json(data);
        }

        // Fungsi untuk mengambil nilai angka temperature dan humidity secara historis untuk menampilkan tren pada grafik (line chart)
        [HttpGet]
        public IActionResult CHART_VALUETH(string date_from, string date_to, string loc)
        {
            var data = new List<ChartTHModel>();
            using (SqlConnection conn = new SqlConnection(DbConnection()))
            {
                var command = new SqlCommand("CHART_VALUETH", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@date_from", (object)date_from ?? DBNull.Value);
                command.Parameters.AddWithValue("@date_to", (object)date_to ?? DBNull.Value);
                command.Parameters.AddWithValue("@location", loc);

                conn.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string datePart = reader.IsDBNull(0) ? string.Empty : reader.GetDateTime(0).ToString("dd MMM yyyy");
                        string shiftPart = reader.IsDBNull(3) ? "0" : reader[3].ToString();

                        string inspDate = $"{datePart} Shift {shiftPart}";

                        data.Add(new ChartTHModel
                        {
                            insp_date = inspDate,
                            temperature_value = reader.IsDBNull(1) ? "0" : reader[1].ToString(),
                            humidity_value = reader.IsDBNull(2) ? "0" : reader[2].ToString(),
                            temperature_status = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                            humidity_status = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                        });
                    }
                }
            }
            return Json(data);
        }

        // Fungsi untuk mengambil data detail laporan berdasarkan filter tanggal, lokasi, dan status (OK/NOK) pada tabel di halaman dashboard.
        [HttpGet]
        public IActionResult GET_TH_DETAIL(string date_from, string date_to, string loc, string audit_status)
        {
            var db = new DatabaseAccessLayer();
            List<THCheckModel> dataList = db.GetTHDetail(date_from, date_to, loc, audit_status);

            ViewBag.loc = loc;
            ViewBag.audit_status = audit_status;

            return PartialView("_TableTHDetail", dataList);
        }

        // Fungsi untuk menyimpan catatan (remark) tambahan dari metrology jika ditemukan hasil yang tidak sesuai
        [HttpPost]
        public IActionResult INSERT_REMARK_TH(string id_th, string location_remark, string checking_date_remark, string remark_metrology)
        {
            try
            {
                string sesa_id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                 ?? HttpContext.Session.GetString("sesa_id")
                                 ?? "System";

                using (SqlConnection conn = new SqlConnection(DbConnection()))
                {
                    var command = new SqlCommand("INSERT_REMARK_TH", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    command.Parameters.AddWithValue("@id_th", Convert.ToInt32(id_th));
                    command.Parameters.AddWithValue("@loc_remark", location_remark);
                    command.Parameters.AddWithValue("@checking_date", DateTime.Parse(checking_date_remark));
                    command.Parameters.AddWithValue("@remark_metrology", remark_metrology);
                    command.Parameters.AddWithValue("@sesa_id", sesa_id);

                    conn.Open();
                    command.ExecuteNonQuery();
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Fungsi untuk menampilkan dan mengambil data WI
        public IActionResult WorkInstruction()
        {
            var db = new DatabaseAccessLayer();
            var allWorkInstructions = db.GetWorkInstructions().Where(x => x.wi_type == "TH").ToList();
            return View(allWorkInstructions);
        }

        // Fungsi untuk mengupload dokumen WI baru ke dalam database
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