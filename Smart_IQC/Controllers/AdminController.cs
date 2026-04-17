using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Smart_IQC.Function;
using Smart_IQC.Models;
using System.Data;
using System.Security.Claims;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Smart_IQC.Controllers
{
    [Authorize(Policy = "RequireAdmin")]
    public class AdminController : Controller
    {
        // Fungsi untuk mengambil hak akses level user
        private string GetUserLevel()
        {
            return User.FindFirst("Smart_IQC_level")?.Value?.ToLower();
        }

        private string DbConnection()
        {
            var dbAccess = new DatabaseAccessLayer();
            string dbString = dbAccess.ConnectionString;
            return dbString;
        }
        private readonly IHostingEnvironment hostingEnvironment;
        public AdminController(ILogger<AdminController> logger, IHostingEnvironment environment)
        {
            hostingEnvironment = environment;
        }

        // Fungsi ini untuk menampilkan halaman user management
        public IActionResult UserManagement()
        {
            return this.CheckSession(() =>
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return View();
            });
        }

        // Fungsi ini untuk mengambil list data user dari database dan dapat difilter berdasarkan departemen
        [HttpGet]
        public IActionResult GetUserData(string dept)
        {
            var db = new DatabaseAccessLayer();
            List<UserManagementModel> datalist = db.GetUserData(dept);
            return PartialView("_tableusermanagement", datalist);
        }

        // Fungsi ini untuk mengambil daftar nama departemen
        [HttpGet]
        public IActionResult GetDeptName(string deptname)
        {
            List<UserManagementModel> data = new List<UserManagementModel>();
            string query = "SELECT DISTINCT dept_name FROM mst_department WHERE dept_name LIKE '%" + deptname + "%' ORDER BY dept_name ASC ";
            using (SqlConnection conn = new SqlConnection(DbConnection()))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = conn;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var data_list = new UserManagementModel();
                            data_list.Text = reader["dept_name"].ToString();
                            data_list.Id = reader["dept_name"].ToString();
                            data.Add(data_list);
                        }
                    }
                    conn.Close();
                }
            }

            return Json(new { items = data });
        }

        // Fungsi ini untuk mengambil daftar nama departemen dari tabel mst_user
        [HttpGet]
        public IActionResult GET_DEPT_USER(string dept)
        {
            List<UserManagementModel> data = new List<UserManagementModel>();
            string query = "SELECT DISTINCT dept_id FROM mst_users WHERE dept_id LIKE '%" + dept + "%' ORDER BY dept_id ASC ";
            using (SqlConnection conn = new SqlConnection(DbConnection()))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = conn;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var data_list = new UserManagementModel();
                            data_list.Text = reader["dept_id"].ToString();
                            data_list.Id = reader["dept_id"].ToString();
                            data.Add(data_list);
                        }
                    }
                    conn.Close();
                }
            }

            return Json(new { items = data });
        }

        // Fungsi ini untuk mengunduh seluruh data user ke dalam format file Excel
        //public IActionResult DownloadUserManagement()
        //{
        //    using (XLWorkbook wb = new XLWorkbook())
        //    {
        //        var ws = wb.Worksheets.Add("User Data");
        //        DateTime currentDateTime = DateTime.Now;
        //        string formattedDateTime = currentDateTime.ToString("yyyyMMdd_HHmmss");

        //        ws.Cell(1, 1).Value = "No";
        //        ws.Cell(1, 2).Value = "SESA ID";
        //        ws.Cell(1, 3).Value = "Name";
        //        ws.Cell(1, 4).Value = "Email";
        //        ws.Cell(1, 5).Value = "Department";
        //        ws.Cell(1, 6).Value = "Level";
        //        ws.Cell(1, 7).Value = "Role";
        //        ws.Cell(1, 8).Value = "Access";

        //        // Style Header 
        //        var headerRange = ws.Range(1, 1, 1, 8);
        //        headerRange.Style.Font.Bold = true;
        //        headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen;

        //        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //        headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        //        DataSet ds = GetUserManagementDownload();
        //        DataTable dt = ds.Tables[0];

        //        for (int i = 0; i < dt.Rows.Count; i++)
        //        {
        //            ws.Cell(i + 2, 1).Value = i + 1;
        //            ws.Cell(i + 2, 2).Value = dt.Rows[i]["sesa_id"].ToString();
        //            ws.Cell(i + 2, 3).Value = dt.Rows[i]["name"].ToString();
        //            ws.Cell(i + 2, 4).Value = dt.Rows[i]["email"].ToString();
        //            ws.Cell(i + 2, 5).Value = dt.Rows[i]["dept_id"].ToString();
        //            ws.Cell(i + 2, 6).Value = dt.Rows[i]["level"].ToString();
        //            ws.Cell(i + 2, 7).Value = dt.Rows[i]["roles"].ToString();
        //            ws.Cell(i + 2, 8).Value = dt.Rows[i]["apps_id"].ToString();
        //        }

        //        ws.Columns().AdjustToContents(); 

        //        using (MemoryStream stream = new MemoryStream())
        //        {
        //            wb.SaveAs(stream);
        //            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "User_Management_" + formattedDateTime + ".xlsx");
        //        }
        //    }
        //}

        //private DataSet GetUserManagementDownload()
        //{
        //    DataSet ds = new DataSet();

        //    using (SqlConnection conn = new SqlConnection(DbConnection()))
        //    {
        //        string query = @"
        //    SELECT * from mst_users";

        //        using (SqlCommand cmd = new SqlCommand(query))
        //        {
        //            cmd.Connection = conn;
        //            using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
        //            {
        //                sda.Fill(ds);
        //            }
        //        }
        //    }

        //    return ds;
        //}

        // Fungsi ini untuk menghapus banyak data user sekaligus
        //[HttpPost]
        //[Route("DeleteSelectData")]
        //public IActionResult DeleteSelectData([FromBody] UserManagementModel[] input)
        //{
        //    string level = GetUserLevel();
        //    if (level == "inspector" || level == "admin")
        //    {
        //        int totalDeleted = 0;

        //        using (SqlConnection conn = new SqlConnection(DbConnection()))
        //        {
        //            conn.Open();
        //            string query = "DELETE FROM mst_users WHERE id_user = @id_user";

        //            foreach (var item in input)
        //            {
        //                using (SqlCommand cmd = new SqlCommand(query, conn))
        //                {
        //                    cmd.Parameters.Add(new SqlParameter("@id_user", item.id_user));
        //                    try
        //                    {
        //                        totalDeleted += cmd.ExecuteNonQuery();
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        Console.Error.WriteLine(ex.Message);
        //                    }
        //                }
        //            }
        //        }

        //        return Json(totalDeleted); 
        //    }
        //    else
        //    {
        //        return RedirectToAction("Logout", "Home");
        //    }
        //}

        // Fungsi untuk menambah data department
        [HttpPost]
        public IActionResult AddDepartment(string id_dept, string dept_name)
        {
            if (string.IsNullOrEmpty(id_dept) || string.IsNullOrEmpty(dept_name))
            {
                return Json(new { success = false, message = "ID and Name are required." });
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnection()))
                {
                    conn.Open();
             
                    string checkQuery = "SELECT COUNT(1) FROM mst_department WHERE id_dept = @id";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@id", id_dept.Trim());
                        if ((int)checkCmd.ExecuteScalar() > 0)
                            return Json(new { success = false, message = "Department ID already exists!" });
                    }

                    string query = "INSERT INTO mst_department (id_dept, dept_name) VALUES (@id, @name)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id_dept.Trim().ToUpper()); 
                        cmd.Parameters.AddWithValue("@name", dept_name.Trim());
                        cmd.ExecuteNonQuery();
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Fungsi untuk menghapus satu data user
        [HttpPost]
        public IActionResult DeleteData(string id_user)
        {
            string level = GetUserLevel();
            if (level == "inspector" || level == "admin")
            {
                int rowsAffected = 0;

                if (string.IsNullOrEmpty(id_user))
                {
                    return Json(new { success = false, message = "User ID cannot be empty." });
                }

                if (int.TryParse(id_user, out int userId))
                {
                    using (SqlConnection conn = new SqlConnection(DbConnection()))
                    {
                        conn.Open();
                        string query = @"DELETE FROM mst_users WHERE id_user = @id_user";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@id_user", userId);

                        rowsAffected = cmd.ExecuteNonQuery();
                        conn.Close();
                    }

                    return Json(new { success = true, rowsAffected });
                }
                else
                {
                    return Json(new { success = false, message = "User ID not valid." });
                }
            }
            else
            {
                return RedirectToAction("Logout", "Home");    
            }
        }

        // Fungsi untuk memperbarui data user
        [HttpPost]
        public IActionResult UpdateData(string sesa_id, string name, string email, string password, string department, string level, string apps_id, string roles)
        {
            if (GetUserLevel() == "admin")
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(DbConnection()))
                    {
                        con.Open();

                        // Ambil password lama dari database terlebih dahulu
                        string existingPassword = "";
                        string getPassQuery = "SELECT password FROM mst_users WHERE sesa_id = @sesa_id";
                        using (SqlCommand cmdGet = new SqlCommand(getPassQuery, con))
                        {
                            cmdGet.Parameters.AddWithValue("@sesa_id", sesa_id);
                            existingPassword = cmdGet.ExecuteScalar()?.ToString();
                        }

                        // Tentukan password yang akan disimpan
                        string passwordToSave;
                        if (string.IsNullOrEmpty(password))
                        {
                            passwordToSave = existingPassword;
                        }

                        else
                        {
                            Authentication auth = new Authentication();
                            passwordToSave = auth.MD5Hash(password);
                        }

                        string query = @"UPDATE mst_users 
                                 SET name = @name, 
                                     email = @email, 
                                     password = @password, 
                                     dept_id = @dept_id, 
                                     level = @level, 
                                     roles = @roles,
                                     apps_id = @apps_id
                                 WHERE sesa_id = @sesa_id";

                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                            cmd.Parameters.AddWithValue("@name", name);
                            cmd.Parameters.AddWithValue("@email", email);
                            cmd.Parameters.AddWithValue("@password", passwordToSave);
                            cmd.Parameters.AddWithValue("@dept_id", department);
                            cmd.Parameters.AddWithValue("@level", level);
                            cmd.Parameters.AddWithValue("@apps_id", apps_id ?? "");
                            cmd.Parameters.AddWithValue("@roles", roles);

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                return Json(1);
                            }
                            else
                            {
                                return Json(0);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }
            return RedirectToAction("Logout", "Home");
        }

        // Fungsi untuk mendaftarkan user baru ke dalam database
        [HttpPost]
        [Route("AddUserManagement")]
        public JsonResult AddUserManagement(string sesa_id, string name, string email, string password, string department, string level, string apps_id, string roles)
        {
            // Verify user access level
            string userLevel = GetUserLevel();
            if (userLevel == "inspector" || userLevel == "admin")
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(DbConnection()))
                    {
                        con.Open();

                        string checkQuery = "SELECT COUNT(1) FROM mst_users WHERE sesa_id = @sesa";
                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                        {
                            checkCmd.Parameters.AddWithValue("@sesa", sesa_id.Trim());
                            int count = (int)checkCmd.ExecuteScalar();
                            if (count > 0)
                            {
                                return Json(new { success = false, message = "User with SESA ID '" + sesa_id + "' already exists!" });
                            }
                        }

                        Authentication hashpass = new Authentication();
                        string pass = hashpass.MD5Hash(password);

                        using (SqlCommand cmd = new SqlCommand("ADD_USER_MANAGEMENT", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            // Map the parameters
                            cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                            cmd.Parameters.AddWithValue("@name", name);
                            cmd.Parameters.AddWithValue("@email", email);
                            cmd.Parameters.AddWithValue("@password", pass);
                            cmd.Parameters.AddWithValue("@dept_id", department);
                            cmd.Parameters.AddWithValue("@level", level);
                            cmd.Parameters.AddWithValue("@apps_id", apps_id);
                            cmd.Parameters.AddWithValue("@roles", roles);

                            var returnParam = cmd.Parameters.Add("@ReturnValue", SqlDbType.Int);
                            returnParam.Direction = ParameterDirection.ReturnValue;

                            cmd.ExecuteNonQuery();

                            int result = (int)returnParam.Value;

                            // Interpret the result
                            if (result == 1)
                            {
                                return Json(new { success = true, message = "Data added successfully." });
                            }
                            else if (result == -1)
                            {
                                return Json(new { success = false, message = "The data already exists." });
                            }
                            else
                            {
                                return Json(new { success = false, message = "An error occurred while adding data." });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "There is an error: " + ex.Message });
                }
            }
            else
            {
                return Json(new { success = false, message = "You don't have permission." });
            }
        }

        // Fungsi untuk menampilkan halaman supplier
        public IActionResult MasterSupplier()
        {
            return this.CheckSession(() =>
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return View();
            });
        }

        // Fungsi untuk mendapatkan data supplier
        [HttpGet]
        public IActionResult GetSupplierData()
        {
            var db = new DatabaseAccessLayer();
            List<SupplierModel> datalist = db.GetSupplierData();
            return PartialView("_tablemastersupplier", datalist);
        }

        // Fungsi untuk menambah data supplier
        [HttpPost]
        public IActionResult AddMasterSupplier(string supplier_name, string vendor_code)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnection()))
                {
                    conn.Open();
                    string checkQuery = "SELECT COUNT(1) FROM mst_supplier WHERE vendor_code = @code";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@code", vendor_code.Trim());
                        int count = (int)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            return Json(new { success = false, message = "Vendor Code '" + vendor_code + "' already exists!" });
                        }
                    }

                    string query = "INSERT INTO mst_supplier (supplier_name, vendor_code) VALUES (@name, @code)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@name", supplier_name);
                    cmd.Parameters.AddWithValue("@code", vendor_code);
                    cmd.ExecuteNonQuery();
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Fungsi untuk mengubah data supplier
        [HttpPost]
        public IActionResult UpdateDataSupplier(int id_supplier, string supplier_name, string vendor_code)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnection()))
                {
                    conn.Open();
                    string queryCheck = "SELECT COUNT(1) FROM mst_supplier WHERE vendor_code = @code AND supplier_id != @id";
                    using (SqlCommand cmdCheck = new SqlCommand(queryCheck, conn))
                    {
                        cmdCheck.Parameters.AddWithValue("@code", vendor_code.Trim());
                        cmdCheck.Parameters.AddWithValue("@id", id_supplier);
                        if ((int)cmdCheck.ExecuteScalar() > 0)
                        {
                            return Json(new { success = false, message = "Vendor Code '" + vendor_code + "' already exists!" });
                        }
                    }
                }

                var db = new DatabaseAccessLayer();
                bool result = db.UpdateSupplier(id_supplier, supplier_name, vendor_code);

                if (result)
                {
                    return Json(new { success = true, message = "Data supplier successfully updated." });
                }
                else
                {
                    return Json(new { success = false, message = "Update failed. Data not found or no changes made." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Fungsi untuk menghapus data supplier
        [HttpPost]
        public IActionResult DeleteDataSupplier(string supplier_id)
        {
            string level = GetUserLevel();
            if (level == "inspector" || level == "admin")
            {
                int rowsAffected = 0;

                if (string.IsNullOrEmpty(supplier_id))
                {
                    return Json(new { success = false, message = "Supplier ID cannot be empty." });
                }

                if (int.TryParse(supplier_id, out int id_supplier))
                {
                    using (SqlConnection conn = new SqlConnection(DbConnection()))
                    {
                        conn.Open();
                        string query = @"DELETE FROM mst_supplier WHERE supplier_id = @id_supplier";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@id_supplier", id_supplier);

                        rowsAffected = cmd.ExecuteNonQuery();
                        conn.Close();
                    }

                    return Json(new { success = true, rowsAffected });
                }
                else
                {
                    return Json(new { success = false, message = "Supplier ID not valid." });
                }
            }
            else
            {
                return RedirectToAction("Logout", "Home");
            }
        }

        // Fungsi untuk menampilkan halaman master category
        public IActionResult MasterCategory()
        {
            return this.CheckSession(() =>
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return View();
            });
        }

        // Fungsi untuk mendapatkan data category
        [HttpGet]
        public IActionResult GetCategoryData()
        {
            var db = new DatabaseAccessLayer();
            List<CommodityModel> datalist = db.GetCategoryData();
            return PartialView("_tablemastercategory", datalist);
        }

        // Fungsi untuk menambah data category
        [HttpPost]
        public IActionResult AddMasterCategory(string commodity, string commodity_name)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnection()))
                {
                    conn.Open();

                    string checkQuery = "SELECT COUNT(1) FROM mst_commodity WHERE commodity = @id";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@id", commodity.Trim());
                        if ((int)checkCmd.ExecuteScalar() > 0)
                        {
                            return Json(new { success = false, message = "Category ID '" + commodity + "' already exists!" });
                        }
                    }

                    string query = "INSERT INTO mst_commodity (commodity, commodity_name) VALUES (@id, @name)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", commodity.Trim());
                        cmd.Parameters.AddWithValue("@name", commodity_name.Trim());
                        cmd.ExecuteNonQuery();
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Fungsi untuk mengubah data category
        [HttpPost]
        public IActionResult UpdateDataCategory(string commodity_old, string commodity_new, string commodity_name)
        {
            var db = new DatabaseAccessLayer();

            bool result = db.UpdateCategory(commodity_old, commodity_new, commodity_name);

            if (result)
            {
                return Json(new { success = true, message = "Category updated successfully!" });
            }
            else
            {
                return Json(new { success = false, message = "Category ID '" + commodity_new + "' already exists!" });
            }
        }

        // Fungsi untuk menghapus data category
        [HttpPost]
        public IActionResult DeleteDataCategory(string commodity)
        {
            string level = GetUserLevel();

            if (level == "inspector" || level == "admin")
            {
                int rowsAffected = 0;

                if (string.IsNullOrWhiteSpace(commodity))
                {
                    return Json(new { success = false, message = "Category ID cannot be empty." });
                }

                try
                {
                    using (SqlConnection conn = new SqlConnection(DbConnection()))
                    {
                        conn.Open();
                        string query = @"DELETE FROM mst_commodity WHERE commodity = @id";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", commodity);
                            rowsAffected = cmd.ExecuteNonQuery();
                        }
                        conn.Close();
                    }

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, rowsAffected = rowsAffected });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Data not found or already deleted." });
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Error: " + ex.Message });
                }
            }
            else
            {
                return RedirectToAction("Logout", "Home");
            }
        }

        // Fungsi untuk menampilkan halaman master data part number
        public IActionResult MasterPartNumber()
        {
            return this.CheckSession(() =>
            {
                string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return View();
            });
        }

        // Fungsi untuk mendapatkan data part number
        [HttpGet]
        public IActionResult GetPartNumberData()
        {
            var db = new DatabaseAccessLayer();
            List<PartListModel> datalist = db.GetPartNumberData();
            return PartialView("_tablemasterpartnumber", datalist);
        }

        // Fungsi untuk menambah data part number
        [HttpPost]
        public IActionResult AddMasterPartNumber(string part_number)
        {
            if (string.IsNullOrWhiteSpace(part_number))
            {
                return Json(new { success = false, message = "Part Number cannot be empty!" });
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnection()))
                {
                    conn.Open();

                    string checkQuery = "SELECT COUNT(1) FROM mst_part_number WHERE part_number = @number";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@number", part_number.Trim());
                        int count = (int)checkCmd.ExecuteScalar();

                        if (count > 0)
                        {
                            return Json(new { success = false, message = "Part Number '" + part_number + "' already exists!" });
                        }
                    }

                    string query = "INSERT INTO mst_part_number (part_number) VALUES (@number)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@number", part_number.Trim());
                        cmd.ExecuteNonQuery();
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Fungsi untuk mengubah data part number
        [HttpPost]
        public IActionResult UpdateDataPartNumber(int id_part, string part_number)
        {
            if (string.IsNullOrWhiteSpace(part_number))
            {
                return Json(new { success = false, message = "Part Number cannot be empty!" });
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnection()))
                {
                    conn.Open();

                    string checkQuery = "SELECT COUNT(1) FROM mst_part_number WHERE part_number = @number AND id_part != @id";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@number", part_number.Trim());
                        checkCmd.Parameters.AddWithValue("@id", id_part);
                        int count = (int)checkCmd.ExecuteScalar();

                        if (count > 0)
                        {
                            return Json(new { success = false, message = "Part Number '" + part_number + "' already exists!" });
                        }
                    }
                }

                var db = new DatabaseAccessLayer();
                bool result = db.UpdatePartNumber(id_part, part_number.Trim());

                if (result)
                {
                    return Json(new { success = true, message = "Updated successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update or no changes detected." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // Fungsi untuk menghapus data part number
        [HttpPost]
        public IActionResult DeleteDataPartNumber(string id_part)
        {
            string level = GetUserLevel();

            if (level == "inspector" || level == "admin")
            {
                int rowsAffected = 0;

                if (string.IsNullOrWhiteSpace(id_part))
                {
                    return Json(new { success = false, message = "Part Number ID cannot be empty." });
                }

                try
                {
                    using (SqlConnection conn = new SqlConnection(DbConnection()))
                    {
                        conn.Open();
                        string query = @"DELETE FROM mst_part_number WHERE id_part = @id";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", id_part);
                            rowsAffected = cmd.ExecuteNonQuery();
                        }
                        conn.Close();
                    }

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, rowsAffected = rowsAffected });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Data not found or already deleted." });
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Error: " + ex.Message });
                }
            }
            else
            {
                return RedirectToAction("Logout", "Home");
            }
        }
    }
}