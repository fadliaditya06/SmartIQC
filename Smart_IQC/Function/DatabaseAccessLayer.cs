using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata;
using Smart_IQC.Models; 
using System.Collections.Generic;
using System.Data;
using System.Diagnostics; 
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.Mail;

namespace Smart_IQC.Function
{
    public class DatabaseAccessLayer
    {
        public string ConnectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=Smart_IQC;Integrated Security=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";

        // Fungsi untuk mengambil detail data user berdasarkan SESA ID
        public List<UserDetailModel> GetUserDetail(string sesa_id)
        {
            List<UserDetailModel> dataList = new List<UserDetailModel>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("GET_USER_DETAIL", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                    using SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            UserDetailModel row = new UserDetailModel();
                            row.sesa_id = reader["sesa_id"].ToString();
                            row.name = reader["name"].ToString();
                            row.email = reader["email"].ToString();
                            row.level = reader["level"].ToString();
                            row.apps_id = reader["apps_id"].ToString();
                            row.dept_id = reader["dept_id"].ToString();
                            row.role = reader["roles"].ToString();
                            dataList.Add(row);
                        }
                    }
                }

                conn.Close();
            }
            return dataList;
        }

        // Fungsi untuk mengambil data checklist inspeksi berdasarkan reference yang dipilih
        public List<LayerCheckModel> GetFilteredChecklist(string selectedUniqueReff)
        {
            var results = new List<LayerCheckModel>();

            string query = "SELECT unique_reff, location, defect_description, inspection_step FROM tbl_inspection_checklist WHERE unique_reff = @uniqueReff";

            if (string.IsNullOrEmpty(selectedUniqueReff))
            {
                return results;
            }

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@uniqueReff", selectedUniqueReff);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new LayerCheckModel
                            {
                                Unique_Reff = reader["unique_reff"]?.ToString(),
                                Location = reader["location"]?.ToString(),
                                Defect_Description = reader["defect_description"]?.ToString(),
                                Inspection_Step = reader["inspection_step"]?.ToString()
                            });
                        }
                    }
                }
            }
            return results;
        }

        // Fungsi untuk memperbarui status pada tabel checklist inspeksi
        public void UpdateSampleResultStatus(string uniqueReff, string status)
        {
            string query = "UPDATE tbl_inspection_checklist SET status = @status WHERE unique_reff = @unique_reff";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@status", status);
                    cmd.Parameters.AddWithValue("@unique_reff", uniqueReff);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Fungsi untuk mencari data supplier di halaman checklist
        public List<SupplierModel> SearchSupplier(string query)
        {
            var suppliers = new List<SupplierModel>();
            string sqlQuery = "SELECT supplier_id, supplier_name, vendor_code FROM mst_supplier WHERE supplier_name LIKE @query OR supplier_id LIKE @query";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                {
                    cmd.Parameters.AddWithValue("@query", "%" + query + "%");
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            suppliers.Add(new SupplierModel
                            {
                                Supplier_ID = Convert.ToInt32(reader["supplier_id"]),
                                Supplier_Name = reader["supplier_name"]?.ToString(),
                                Vendor_Code = reader["vendor_code"]?.ToString()
                            });
                        }
                    }
                }
            }
            return suppliers;
        }

        // Fungsi untuk mencari issue category di halaman checklist
        public List<IssueCategoryModel> SearchIssueCategory(string query)
        {
            var categories = new List<IssueCategoryModel>();
            string sqlQuery = "SELECT id_issue, issue_name FROM mst_issue_category WHERE issue_name LIKE @query OR id_issue LIKE @query";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                {
                    cmd.Parameters.AddWithValue("@query", "%" + query + "%");
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string id = reader["id_issue"]?.ToString();
                            string name = reader["issue_name"]?.ToString();

                            string display = $"{id} - {name}";

                            categories.Add(new IssueCategoryModel
                            {
                                id_issue = id,
                                issue_name = display
                            });
                        }
                    }
                }
            }
            return categories;
        }

        // Fungsi untuk mencari part number di halaman checklist
        public List<PartListModel> SearchPartNumber(string term)
        {
            var partNumbers = new List<PartListModel>();
            string sqlQuery = "SELECT part_number FROM mst_part_number WHERE part_number LIKE @term";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                {
                    cmd.Parameters.AddWithValue("@term", "%" + term + "%");
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var partNumValue = reader["part_number"]?.ToString();

                            partNumbers.Add(new PartListModel
                            {
                                Part_Number = partNumValue
                            });
                        }
                    }
                }
            }
            return partNumbers;
        }

        // Fungsi untuk mencari category di halaman checklist
        public List<CommodityModel> SearchCategory(string query)
        {
            var categories = new List<CommodityModel>();
            string sqlQuery = "SELECT commodity, commodity_name FROM mst_commodity WHERE commodity LIKE @query OR commodity_name LIKE @query";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                {
                    cmd.Parameters.AddWithValue("@query", "%" + query + "%");
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string code = reader["commodity"]?.ToString();
                            string name = reader["commodity_name"]?.ToString();
                            string display = $"{code} - {name}";
                            categories.Add(new CommodityModel
                            {
                                Commodity = code,
                                Commodity_Name = display

                            });
                        }
                    }
                }
            }
            return categories;
        }

        // Fungsi untuk menyimpan seluruh data checklist hasil inspeksi beserta detail dan riwayatnya
        public bool SaveFullChecklist(ChecklistDataModel data)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
                SqlTransaction transaction = con.BeginTransaction();

                try
                {
                    string locationData = data.Checklist_Details.FirstOrDefault()?.Location ?? DBNull.Value.ToString();
                    string defectDescData = data.Checklist_Details.FirstOrDefault()?.Defect_Description ?? DBNull.Value.ToString();

                    string headerSql = "INSERT INTO tbl_result (id_commodity, supplier_id, part_number, critical_part_status, audit_by, record_date, location, defect_desc, id_issue) OUTPUT INSERTED.id_result VALUES (@idCommodity, @idSupplier, @partNumber, @criticalStatus, @auditBy, GETDATE(), @location, @defectDesc, @idIssue)";
                    int recordId;

                    using (SqlCommand cmd = new SqlCommand(headerSql, con, transaction))
                    {
                        cmd.Parameters.AddWithValue("@idCommodity", data.Commodity_ID);
                        cmd.Parameters.AddWithValue("@idSupplier", data.Supplier_ID);
                        cmd.Parameters.AddWithValue("@partNumber", data.Part_Number);
                        cmd.Parameters.AddWithValue("@criticalStatus", data.Critical_Part_Status);
                        cmd.Parameters.AddWithValue("@auditBy", data.Audit_By);
                        cmd.Parameters.AddWithValue("@location", locationData);
                        cmd.Parameters.AddWithValue("@defectDesc", defectDescData);
                        cmd.Parameters.AddWithValue("@idIssue", data.Id_Issue);
                        recordId = (int)cmd.ExecuteScalar();
                    }

                    string storedProcedureName = "[dbo].[GET_CATEGORY_SUPPLIER_NAMES]";
                    string commodityName = "";
                    string supplierName = "";

                    using (SqlCommand cmdName = new SqlCommand(storedProcedureName, con, transaction))
                    {
                        cmdName.CommandType = CommandType.StoredProcedure;

                        cmdName.Parameters.AddWithValue("@idCommodity", data.Commodity_ID);
                        cmdName.Parameters.AddWithValue("@idSupplier", data.Supplier_ID);

                        using (SqlDataReader reader = cmdName.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                commodityName = reader["CategoryName"].ToString();
                                supplierName = reader["SupplierName"].ToString();
                            }
                        }
                    }

                    string reportId = $"{recordId}-{commodityName}-{data.Part_Number}-{supplierName}";

                    string updateReportIdSql = "UPDATE tbl_result SET report_id = @reportId WHERE id_result = @recordId";
                    using (SqlCommand cmdUpdate = new SqlCommand(updateReportIdSql, con, transaction))
                    {
                        cmdUpdate.Parameters.AddWithValue("@reportId", reportId);
                        cmdUpdate.Parameters.AddWithValue("@recordId", recordId);
                        cmdUpdate.ExecuteNonQuery();
                    }

                    string detailSql = "INSERT INTO tbl_result_details (id_result, unique_reff, status, id_dept, pic_sesa_id, comment, file_image) VALUES (@recordId, @reff, @status, @idDept, @picSesaId, @comment, @fileImage)";
                    using (SqlCommand cmd = new SqlCommand(detailSql, con, transaction))
                    {
                        foreach (var item in data.Checklist_Details)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@recordId", recordId);
                            cmd.Parameters.AddWithValue("@reff", item.Unique_Reff);
                            cmd.Parameters.AddWithValue("@status", item.Status);
                            cmd.Parameters.AddWithValue("@idDept", (object)item.IdDept ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@picSesaId", (object)item.PicSesaId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@comment", (object)item.Comment ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@fileImage", (object)item.FileImage ?? DBNull.Value);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();

                    try
                    {
                        using (SqlCommand cmdHistory = new SqlCommand("INSERT_INSPECTION_HISTORY", con))
                        {
                            cmdHistory.CommandType = CommandType.StoredProcedure;
                            cmdHistory.Parameters.AddWithValue("@ResultID", recordId);
                            cmdHistory.ExecuteNonQuery();
                        }
                    }
                    catch (Exception exHistory)
                    {
                        System.Diagnostics.Debug.WriteLine($"WARNING: Failed to insert data into inspection history. {exHistory.Message}");
                    }

                    var auditorDetail = GetPicDetailsBySesaId(data.Audit_By);
                    string auditorEmailCC = auditorDetail.email;

                    foreach (var item in data.Checklist_Details.Where(d => d.Status == "NOK"))
                    {
                        if (!string.IsNullOrEmpty(item.PicSesaId))
                        {
                            var picDetail = GetPicDetailsBySesaId(item.PicSesaId);

                            if (!string.IsNullOrEmpty(picDetail.email))
                            {
                                SendNotificationEmail(
                                    picDetail.email,
                                    picDetail.name,
                                    commodityName,
                                    data.Part_Number,
                                    reportId,
                                    item.Unique_Reff,
                                    item.Comment,
                                    supplierName,
                                    auditorEmailCC
                                );
                            }
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("SAVE CHECKLIST FAILED");
                    System.Diagnostics.Debug.WriteLine("Message Exception Detail: " + ex.ToString());
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine("DATABASE SAVE FAILED: " + ex.ToString());
                    return false;
                }
            }
        }

        // Fungsi untuk mencari data departemen berdasarkan nama
        public List<DepartmentModel> SearchDepartment(string term)
        {
            var departments = new List<DepartmentModel>();
            string sqlQuery = "SELECT id_dept, dept_name FROM mst_department WHERE dept_name LIKE @term";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                {
                    cmd.Parameters.AddWithValue("@term", "%" + term + "%");
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            departments.Add(new DepartmentModel
                            {
                                id_dept = reader["id_dept"]?.ToString(),
                                dept_name = reader["dept_name"]?.ToString()
                            });
                        }
                    }
                }
            }
            return departments;
        }

        // Fungsi untuk mengambil seluruh daftar departemen yang tersedia
        public List<DepartmentModel> GetAllDepartments()
        {
            var departments = new List<DepartmentModel>();
            string sqlQuery = "SELECT id_dept, dept_name FROM mst_department ORDER BY dept_name";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            departments.Add(new DepartmentModel
                            {
                                id_dept = reader["id_dept"]?.ToString(),
                                dept_name = reader["dept_name"]?.ToString()
                            });
                        }
                    }
                }
            }
            return departments;
        }

        // Fungsi untuk mengambil seluruh data user untuk sebagai PIC
        public List<PICModel> GetAllPICs()
        {
            var pics = new List<PICModel>();
            string sqlQuery = "SELECT sesa_id, name FROM mst_users ORDER BY name";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pics.Add(new PICModel
                            {
                                sesa_id = reader["sesa_id"]?.ToString(),
                                name = reader["name"]?.ToString()
                            });
                        }
                    }
                }
            }
            return pics;
        }

        // Fungsi untuk mengambil daftar open points yang masih aktif
        public List<OpenPointsModel> GetOpenPoints()
        {
            var openpoints = new List<OpenPointsModel>();

            string storedProcedureName = "GET_OPEN_POINTS";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(storedProcedureName, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            openpoints.Add(new OpenPointsModel
                            {
                                Report_ID = reader["Report_ID"]?.ToString(),
                                Category = reader["Category"]?.ToString(),
                                Supplier_Name = reader["Supplier_Name"]?.ToString(),
                                Part_Number = reader["Part_Number"]?.ToString(),
                                Critical_Part_Status = reader["Critical_Part_Status"]?.ToString(),
                                Record_Date = Convert.ToDateTime(reader["record_date"]),
                                Question = reader["Question"]?.ToString(),
                                PIC_Name = reader["PIC_Name"]?.ToString(),
                                PIC_Status = Convert.ToInt32(reader["PIC_Status"]),
                                Comment = reader["Comment"]?.ToString(),
                                PIC_Action = reader["PIC_Action"]?.ToString(),
                                Due_Date = reader["Due_Date"] != DBNull.Value ? Convert.ToDateTime(reader["Due_Date"]) : (DateTime?)null,
                                Requirement = reader["Requirement"]?.ToString(),
                                File_Image = reader["File_Image"]?.ToString(),
                                PicSesaId = reader["PicSesaId"]?.ToString(),
                                Dept_Name = reader["Dept_Name"]?.ToString(),
                                File_Action_Image = reader["File_Action_Image"]?.ToString(),
                                Due_Date_Status = reader["Due_Date_Status"]?.ToString(),
                            });
                        }
                    }
                }
            }
            return openpoints;
        }

        // Fungsi untuk menyimpan data result temporary
        public void SaveTemporaryStatus(ChecklistDataModel tempData)
        {
            if (tempData == null || tempData.Checklist_Details == null || !tempData.Checklist_Details.Any())
            {
                return;
            }

            var detail = tempData.Checklist_Details.First();
            string storedProcedureName = "SAVE_TEMPORARY_STATUS";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(storedProcedureName, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@auditBy", tempData.Audit_By);
                    cmd.Parameters.AddWithValue("@uniqueReff", detail.Unique_Reff);
                    cmd.Parameters.AddWithValue("@status", detail.Status);
                    cmd.Parameters.AddWithValue("@idCommodity", tempData.Commodity_ID);
                    cmd.Parameters.AddWithValue("@idSupplier", tempData.Supplier_ID);
                    cmd.Parameters.AddWithValue("@partNumber", tempData.Part_Number);
                    cmd.Parameters.AddWithValue("@criticalStatus", tempData.Critical_Part_Status);
                    cmd.Parameters.AddWithValue("@idIssue", (object)tempData.Id_Issue ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@location", (object)detail.Location ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@defectDesc", (object)detail.Defect_Description ?? DBNull.Value);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Fungsi untuk mencari seluruh data user untuk change PIC.
        public List<UpdatePIC> SearchAllPICs(string term)
        {
            var results = new List<UpdatePIC>();
            string sqlQuery = "SELECT sesa_id, name FROM mst_users WHERE name LIKE @term AND level IN ('inspector', 'admin')";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                {
                    cmd.Parameters.AddWithValue("@term", "%" + term + "%");
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new UpdatePIC
                            {
                                id = reader["sesa_id"].ToString(),
                                text = reader["name"].ToString()
                            });
                        }
                    }
                }
            }
            return results;
        }

        // Fungsi untuk memperbarui PIC 
        public bool UpdatePIC(string reportID, string newPicSesaId)
        {
            string storedProcedureName = "UPDATE_PIC";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(storedProcedureName, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@NewPicSesaId", newPicSesaId);
                    cmd.Parameters.AddWithValue("@ReportID", reportID);

                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        // Fungsi untuk menyimpan action plan (data details NOK).
        public bool SaveAction(string reportID, string picAction, string dueDate, string imagePath)
        {
            string storedProcedureName = "UPDATE_DATA_DETAILS_NOK";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(storedProcedureName, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@picAction", picAction);
                    cmd.Parameters.AddWithValue("@dueDate", dueDate);

                    if (string.IsNullOrEmpty(imagePath))
                    {
                        cmd.Parameters.AddWithValue("@imagePath", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@imagePath", imagePath);
                    }

                    cmd.Parameters.AddWithValue("@ReportID", reportID);

                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    return rowsAffected > 0;
                }
            }
        }

        // Fungsi untuk mengambil detail data user (untuk mendapatkan nama dan email user)
        public UserDetailModel GetPicDetailsBySesaId(string picSesaId)
        {

            var userDetail = new UserDetailModel();
            string query = "SELECT name, email FROM mst_users WHERE sesa_id = @SesaId";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@SesaId", picSesaId);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userDetail.name = reader["name"]?.ToString();
                            userDetail.email = reader["email"]?.ToString();
                        }
                    }
                }
            }
            return userDetail;
        }

        // Fungsi untuk mengirimkan notifikasi email kepada PIC jika ada hasil inspeksi yang NOK (open points)
        private void SendNotificationEmail(string toEmail, string toName, string category, string partNumber, string reportID, string uniqueReff, string comment, string supplierName, string ccEmail)
        {
            try
            {
                string subject = "";
                string bodyHtml = "";

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SEND_NOTIFICATION_EMAIL", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@toEmail", toEmail);
                        cmd.Parameters.AddWithValue("@toName", toName);
                        cmd.Parameters.AddWithValue("@category", category);
                        cmd.Parameters.AddWithValue("@partNumber", partNumber);
                        cmd.Parameters.AddWithValue("@reportID", reportID);
                        cmd.Parameters.AddWithValue("@uniqueReff", uniqueReff);
                        cmd.Parameters.AddWithValue("@comment", (object)comment ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@supplierName", supplierName);

                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                subject = reader["EmailSubject"].ToString();
                                bodyHtml = reader["EmailBody"].ToString();
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(bodyHtml)) return;

                MailMessage mail = new MailMessage();
                string myOutlookEmail = "fadliadtyas10@gmail.com";
                string myAppPassword = "mucrpripdhcnmseu";
                mail.From = new MailAddress(myOutlookEmail, "Smart IQC System");
                mail.To.Add(new MailAddress(toEmail, toName));

                if (!string.IsNullOrEmpty(ccEmail))
                {
                    mail.CC.Add(ccEmail.Trim());
                }

                List<string> bccEmails = GetMailBCC();
                foreach (string bcc in bccEmails)
                {
                    if (!string.IsNullOrWhiteSpace(bcc)) mail.Bcc.Add(bcc.Trim());
                }

                mail.Subject = subject;
                mail.Body = bodyHtml;
                mail.IsBodyHtml = true;

                using (System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient("smtp.gmail.com"))
                {
                    client.Port = 587;
                    client.EnableSsl = true;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;

                    client.Credentials = new System.Net.NetworkCredential("fadliadtyas10@gmail.com", "mucrpripdhcnmseu");

                    client.Send(mail);
                }

                System.Diagnostics.Debug.WriteLine($"EMAIL SENT successfully to {toName} via SP for Ref: {reportID}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EMAIL SEND FAILED via SP: {ex.Message}");
            }
        }

        // Fungsi untuk mengambil daftar email BCC dari database
        private List<string> GetMailBCC()
        {
            List<string> bccEmails = new List<string>();

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                string query = "SELECT email FROM mst_mail_bcc WHERE mail_type = 'bcc'";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    try
                    {
                        con.Open();
                        object emailResult = cmd.ExecuteScalar();

                        if (emailResult != null && emailResult != DBNull.Value)
                        {
                            string emailString = emailResult.ToString();

                            string[] emails = emailString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (string email in emails)
                            {
                                string cleanEmail = email.Trim();
                                if (!string.IsNullOrWhiteSpace(cleanEmail))
                                {
                                    bccEmails.Add(cleanEmail);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"FAILED to get BCC emails: {ex.Message}");
                    }
                }
            }
            return bccEmails;
        }

        // Fungsi untuk memverifikasi dan mengubah status open points menjadi verified
        public bool VerifyOpenPoint(string reportID)
        {
            int recordId;
            string[] parts = reportID.Split('-');
            if (parts.Length < 1 || !int.TryParse(parts[0].Trim(), out recordId)) return false;

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("VERIFY_OPEN_POINTS", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@RecordID", SqlDbType.Int).Value = recordId;

                    try
                    {
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        int rows = (result != null) ? Convert.ToInt32(result) : 0;

                        return rows > 0;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                        return false;
                    }
                }
            }
        }

        // Fungsi untuk mengambil seluruh poin hasil inspeksi (fitur all points)
        public List<OpenPointsModel> GetAllPoints(string dateFrom, string dateTo, string cellName, string family)
        {
            var allpoints = new List<OpenPointsModel>();

            string storedProcedureName = "GET_ALL_POINTS";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(storedProcedureName, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                allpoints.Add(new OpenPointsModel
                                {
                                    Report_ID = reader["Report_ID"]?.ToString(),
                                    Category = reader["Category"]?.ToString(),
                                    Supplier_Name = reader["Supplier_Name"]?.ToString(),
                                    Part_Number = reader["Part_Number"]?.ToString(),
                                    Critical_Part_Status = reader["Critical_Part_Status"]?.ToString(),
                                    Question = reader["Question"]?.ToString(),
                                    PIC_Name = reader["PIC_Name"]?.ToString(),
                                    Status = reader["Status"]?.ToString(),
                                    Record_Date = Convert.ToDateTime(reader["record_date"]),
                                    PIC_Status = reader["PIC_Status"] != DBNull.Value ? Convert.ToInt32(reader["PIC_Status"]) : 0,
                                    Comment = reader["Comment"]?.ToString(),
                                    PIC_Action = reader["PIC_Action"]?.ToString(),
                                    Due_Date = reader["Due_Date"] != DBNull.Value ? Convert.ToDateTime(reader["Due_Date"]) : (DateTime?)null,
                                    Requirement = reader["Requirement"]?.ToString(),
                                    File_Image = reader["File_Image"]?.ToString(),
                                    PicSesaId = reader["PicSesaId"]?.ToString(),
                                    Dept_Name = reader["Dept_Name"]?.ToString(),
                                    File_Action_Image = reader["File_Action_Image"]?.ToString(),
                                    Due_Date_Status = reader["Due_Date_Status"]?.ToString(),
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"GET ALL POINTS FAILED: {ex.ToString()}");
                    }
                }
            }
            return allpoints;
        }

        // Fungsi untuk menyimpan data work instruction
        public bool SaveWorkInstruction(string fileName, string wiType, string uploadedBy)
        {
            string storedProcedureName = "UPLOAD_WORK_INSTRUCTION";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(storedProcedureName, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    {
                        cmd.Parameters.AddWithValue("@FileName", fileName);
                        cmd.Parameters.AddWithValue("@WIType", wiType);
                        cmd.Parameters.AddWithValue("@UploadedBy", (object)uploadedBy ?? DBNull.Value);

                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
        }

        // Fungsi untuk mengambil daftar work instruction yang telah diunggah
        public List<WorkInstructionModel> GetWorkInstructions()
        {
            var fileList = new List<WorkInstructionModel>();
            string query = "SELECT id_wi, file_name, wi_type, upload_by, record_date FROM mst_wi ORDER BY record_date DESC";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fileList.Add(new WorkInstructionModel
                            {
                                id_wi = Convert.ToInt32(reader["id_wi"]),
                                file_name = reader["file_name"].ToString(),
                                upload_by = reader["upload_by"]?.ToString(),
                                wi_type = reader["wi_type"]?.ToString(),
                                record_date = Convert.ToDateTime(reader["record_date"])
                            });
                        }
                    }
                }
            }
            return fileList;
        }

        // Fungsi untuk mengambil data statistik hasil inspeksi IQC untuk ditampilkan pada chart dashboard IQC
        public ChartData GetIqcChartData(string period, string dateFrom, string dateTo)
        {
            DateTime startDate, endDate;

            if (!TryParseDateRange(period, dateFrom, dateTo, out startDate, out endDate))
            {
                return new ChartData
                {
                    Categories = new List<string> { "Error" },
                    SeriesData = new List<ChartSeries> {
                        new ChartSeries { Name = "Data", Data = new List<int> { 0 } }
                    }
                };
            }

            try
            {
                List<AuditResult> allResults = GetRawAuditResultsFromDb();

                var filteredData = allResults
                                    .Where(r => r.record_date.Date >= startDate.Date && r.record_date.Date <= endDate.Date)
                                    .ToList();

                var groupedCounts = filteredData
                    .GroupBy(d => new { d.Commodity_ID, d.Commodity_Name })
                    .Select(g => new
                    {
                        Name = g.Key.Commodity_Name,
                        Count = g.Count()
                    })
                    .OrderByDescending(g => g.Count)
                    .ToList();

                var categories = groupedCounts.Select(g => g.Name).ToList();
                var dataCounts = groupedCounts.Select(g => g.Count).ToList();

                var seriesList = new List<ChartSeries>
                {
                    new ChartSeries
                    {
                        Name = "Total Records",
                        Data = dataCounts
                    }
                };

                return new ChartData
                {
                    Categories = categories,
                    SeriesData = seriesList
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving chart data: {ex.Message}");
                return new ChartData { Categories = new List<string>(), SeriesData = new List<ChartSeries>() };
            }
        }

        // Fungsi helper untuk memproses rentang tanggal berdasarkan periode waktu tertentu.
        private bool TryParseDateRange(string period, string dateFromStr, string dateToStr, out DateTime startDate, out DateTime endDate)
        {
            startDate = DateTime.MinValue;
            endDate = DateTime.MinValue;

            if (string.IsNullOrEmpty(dateFromStr) || string.IsNullOrEmpty(dateToStr))
                return false;

            try
            {
                if (period == "monthly")
                {
                    startDate = DateTime.ParseExact(dateFromStr, "yyyy-MM", CultureInfo.InvariantCulture);
                    endDate = DateTime.ParseExact(dateToStr, "yyyy-MM", CultureInfo.InvariantCulture);
                    endDate = endDate.AddMonths(1).AddDays(-1).Date;
                }
                else if (period == "yearly")
                {
                    int yearFrom = int.Parse(dateFromStr);
                    int yearTo = int.Parse(dateToStr);
                    startDate = new DateTime(yearFrom, 1, 1);
                    endDate = new DateTime(yearTo, 12, 31);
                }
                else if (period == "weekly")
                {
                    startDate = GetDateTimeFromWeekString(dateFromStr);
                    endDate = GetDateTimeFromWeekString(dateToStr).AddDays(6).Date;
                }
                else
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Date Parsing Error: {ex.Message}");
                return false;
            }
        }

        // Helper untuk Parsing Week
        private DateTime GetDateTimeFromWeekString(string weekString)
        {
            int year = int.Parse(weekString.Substring(0, 4));
            int week = int.Parse(weekString.Substring(6));

            DateTime jan1 = new DateTime(year, 1, 1);

            int daysOffset = (int)DayOfWeek.Monday - (int)jan1.DayOfWeek;


            DateTime firstMonday = jan1.AddDays(daysOffset);

            int firstWeek = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(jan1, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            if (firstWeek <= 1)
            {
                week -= 1;
            }

            DateTime targetDate = firstMonday.AddDays(week * 7);

            return targetDate;
        }

        // Fungsi untuk mengambil data mentah hasil inspeksi langsung dari database
        private List<AuditResult> GetRawAuditResultsFromDb()
        {
            var results = new List<AuditResult>();

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GET_RAW_AUDIT_RESULTS", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(new AuditResult
                                {
                                    Commodity_ID = reader["id_commodity"]?.ToString(),
                                    Commodity_Name = reader["Commodity_Name"]?.ToString(),
                                    record_date = reader["record_date"] != DBNull.Value
                                                  ? Convert.ToDateTime(reader["record_date"])
                                                  : DateTime.MinValue
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"DB Read Error (GetRawAuditResultsFromDb): {ex.Message}");
                        return new List<AuditResult>();
                    }
                }
            }
            return results;
        }

        // Logic for Modal Data Details in Open Points - Fungsi untuk mendapatkan detail lengkap open points
        public OpenPointsModel GetOpenPointDetailByReportID(string reportID)
        {
            OpenPointsModel detail = null;

            string storedProcedureName = "GET_OPEN_POINTS_DETAIL";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(storedProcedureName, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ReportID", reportID);

                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                detail = new OpenPointsModel
                                {
                                    Report_ID = reader["Report_ID"]?.ToString(),
                                    Category = reader["Category"]?.ToString(),
                                    Supplier_Name = reader["Supplier_Name"]?.ToString(),
                                    Part_Number = reader["Part_Number"]?.ToString(),
                                    Critical_Part_Status = reader["Critical_Part_Status"]?.ToString(),
                                    Record_Date = Convert.ToDateTime(reader["record_date"]),
                                    Question = reader["Question"]?.ToString(),
                                    PIC_Name = reader["PIC_Name"]?.ToString(),
                                    PIC_Status = Convert.ToInt32(reader["PIC_Status"]),
                                    Comment = reader["Comment"]?.ToString(),
                                    PIC_Action = reader["PIC_Action"]?.ToString(),
                                    Due_Date = reader["Due_Date"] != DBNull.Value ? Convert.ToDateTime(reader["Due_Date"]) : (DateTime?)null,
                                    Requirement = reader["Requirement"]?.ToString(),
                                    File_Image = reader["File_Image"]?.ToString(),
                                    PicSesaId = reader["PicSesaId"]?.ToString(),
                                    Dept_Name = reader["Dept_Name"]?.ToString(),
                                    File_Action_Image = reader["File_Action_Image"]?.ToString(),
                                    Due_Date_Status = reader["Due_Date_Status"]?.ToString(),
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"DB Error in GetOpenPointDetailByReportID for ID {reportID}: {ex.Message}");
                        return null;
                    }
                }
            }
            return detail;
        }

        // Fungsi untuk mendapatkan daftar hari dan tanggal dalam satu minggu tertentu
        public DataSet GetDaysOfWeek(string week_filter)
        {

            SqlConnection conn = new SqlConnection(ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GET_DAYS_OF_WEEK";

            cmd.Parameters.AddWithValue("@week_filter", week_filter);

            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            return ds;
        }

        // Fungsi untuk mendapatkan detail data temp dan hum untuk modal temperature details
        public List<THCheckModel> GetDataTHCheck(string location, string insp_date)
        {
            List<THCheckModel> materials = new List<THCheckModel>();
            string query = "GET_TH_DETAILS";

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    DateTime parsedDate;
                    if (DateTime.TryParse(insp_date, out parsedDate))
                    {
                        cmd.Parameters.AddWithValue("@insp_date", parsedDate);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@insp_date", DBNull.Value);
                    }

                    cmd.Parameters.AddWithValue("@location", location);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            THCheckModel mat = new THCheckModel();
                            mat.insp_trip = reader["insp_trip"]?.ToString();
                            mat.location = reader["location"]?.ToString();
                            mat.temperature_value = reader["temperature_value"]?.ToString();
                            mat.temperature_status = reader["temperature_status"]?.ToString();
                            mat.humidity_value = reader["humidity_value"]?.ToString();
                            mat.humidity_status = reader["humidity_status"]?.ToString();
                            mat.user_id = reader["user_id"]?.ToString();

                            if (reader["record_date"] != DBNull.Value)
                            {
                                mat.insp_date = Convert.ToDateTime(reader["record_date"]).ToString("dd/MM/yyyy hh:mm tt");
                            }
                            else if (reader["insp_date"] != DBNull.Value)
                            {
                                mat.insp_date = Convert.ToDateTime(reader["insp_date"]).ToString("dd/MM/yyyy");
                            }

                            mat.remark = reader["remark"]?.ToString();
                            mat.audit_status = reader["audit_status"]?.ToString();
                            materials.Add(mat);
                        }
                    }
                }
            }
            return materials;
        }

        // Fungsi untuk mendapatkan ringkasan harian status pengecekan suhu dan kelembaban
        public DataSet GetDashTHCheck(string week_filter)
        {
            SqlConnection conn = new SqlConnection(ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GET_TH_CHECK_DAILY";

            cmd.Parameters.AddWithValue("@week_filter", week_filter);


            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            return ds;
        }

        // Fungsi untuk mendapatkan detail data suhu dan kelembaban dalam rentang waktu tertentu untuk dashboard TH
        public List<THCheckModel> GetTHDetail(string date_from, string date_to, string loc, string audit_status)
        {
            List<THCheckModel> assetList = new List<THCheckModel>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("GET_TH_DETAIL_DASH", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@loc", (object)loc ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@audit_status", (object)audit_status ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@date_from", (object)date_from ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@date_to", (object)date_to ?? DBNull.Value);

                    using SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        THCheckModel row = new THCheckModel();
                        row.id = reader["id"].ToString();
                        row.insp_trip = reader["insp_trip"].ToString();
                        row.temperature_value = reader["temperature_value"].ToString();
                        row.temperature_status = reader["temperature_status"].ToString();
                        row.humidity_value = reader["humidity_value"].ToString();
                        row.humidity_status = reader["humidity_status"].ToString();
                        row.tempminmax = reader["tempminmax"].ToString();
                        row.humminmax = reader["humminmax"].ToString();
                        row.audit_status = reader["audit_status"].ToString();
                        row.insp_date = reader["insp_date"].ToString();
                        row.remark = reader["remark"].ToString();
                        assetList.Add(row);
                    }
                }
                conn.Close();
            }
            return assetList;
        }

        // Fungsi untuk mendapatkan summary status wristrap check per minggu (harian)
        public DataSet GetWristrapCheck(string week_filter)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GET_WRISTRAP_DAILY", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (string.IsNullOrEmpty(week_filter))
                    {
                        cmd.Parameters.AddWithValue("@week_filter", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@week_filter", week_filter);
                    }

                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    return ds;
                }
            }
        }

        // Fungsi untuk mendapatkan detail data pengecekan Wristrap yang berstatus OK
        public List<WristrapCheckModel> GetDataWristrapOK(string location, string shift, string check_date)
        {
            List<WristrapCheckModel> materials = new List<WristrapCheckModel>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GET_WRISTRAP_OK", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@location", location);
                    cmd.Parameters.AddWithValue("@shift", shift);
                    cmd.Parameters.AddWithValue("@check_date", check_date);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            materials.Add(new WristrapCheckModel
                            {
                                id_daily = reader["id_daily"].ToString(),
                                inspector = reader["inspector"].ToString(),
                                name = reader["name"].ToString(),
                                result = reader["result"].ToString(),
                                result_final = reader["result_final"]?.ToString(),
                                status_inspector = reader["status_inspector"]?.ToString() ?? reader["result"].ToString(),
                                date = reader["check_date"] != DBNull.Value ? Convert.ToDateTime(reader["check_date"]).ToString("dd/MM/yyyy hh:mm tt") : "",
                                record_date = reader["record_date"] != DBNull.Value ? Convert.ToDateTime(reader["record_date"]).ToString("dd/MM/yyyy hh:mm tt") : "",
                                shift = reader["shift"].ToString(),
                                location = reader["location"].ToString(),
                                remark = reader["remark"].ToString()
                            });
                        }
                    }
                }
            }
            return materials;
        }

        // Fungsi untuk mendapatkan detail data pengecekan Wristrap yang berstatus NG
        public List<WristrapCheckModel> GetDataWristrapNG(string location, string shift, string check_date)
        {
            List<WristrapCheckModel> materials = new List<WristrapCheckModel>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GET_WRISTRAP_NG", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@inspector", location);
                    cmd.Parameters.AddWithValue("@shift", shift);
                    cmd.Parameters.AddWithValue("@check_date", check_date);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            materials.Add(new WristrapCheckModel
                            {
                                id_daily = reader["id_daily"].ToString(),
                                inspector = reader["inspector"].ToString(),
                                name = reader["name"].ToString(),
                                result = reader["result"].ToString(),
                                result_final = reader["result_final"]?.ToString(),
                                remark = reader["remark"].ToString(),
                                location = reader["location"].ToString(),
                                date = Convert.ToDateTime(reader["check_date"]).ToString("dd/MM/yyyy hh:mm tt"),
                                shift = reader["shift"].ToString()
                            });
                        }
                    }
                }
            }
            return materials;
        }

        // Fungsi untuk mendapatkan daftar user 
        public List<UserManagementModel> GetUserData(string dept)
        {
            List<UserManagementModel> materials = new List<UserManagementModel>();
            string query = "GET_USER_MANAGEMENT";
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@dept", dept);
                    cmd.Connection = conn;
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            UserManagementModel mat = new UserManagementModel();
                            mat.id_user = Convert.ToInt32(reader["id_user"]);
                            mat.sesa_id = reader["sesa_id"].ToString();
                            mat.name = reader["name"].ToString();
                            mat.password = reader["password"].ToString();
                            mat.email = reader["email"].ToString();
                            mat.level = reader["level"].ToString();
                            mat.apps_id = reader["apps_id"].ToString();
                            mat.dept_id = reader["dept_id"].ToString();
                            mat.roles = reader["roles"].ToString();
                            mat.record_date = Convert.ToDateTime(reader["record_date"]).ToShortDateString();
                            materials.Add(mat);

                        }
                    }
                    conn.Close();
                }
            }
            return materials;
        }

        // Fungsi untuk mengambil semua data supplier
        public List<SupplierModel> GetSupplierData()
        {
            List<SupplierModel> dataList = new List<SupplierModel>();
            string query = "SELECT supplier_id, supplier_name, vendor_code FROM mst_supplier ORDER BY supplier_name ASC";

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dataList.Add(new SupplierModel
                            {
                                Supplier_ID = Convert.ToInt32(reader["supplier_id"]),
                                Supplier_Name = reader["supplier_name"].ToString(),
                                Vendor_Code = reader["vendor_code"].ToString()
                            });
                        }
                    }
                }
            }
            return dataList;
        }

        // Fungsi Update Supplier
        public bool UpdateSupplier(int id, string name, string code)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "UPDATE mst_supplier SET supplier_name = @name, vendor_code = @code WHERE supplier_id = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@code", code);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // Fungsi untuk mengambil semua data commodity
        public List<CommodityModel> GetCategoryData()
        {
            List<CommodityModel> dataList = new List<CommodityModel>();
            string query = "SELECT commodity, commodity_name FROM mst_commodity ORDER BY commodity_name ASC";

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dataList.Add(new CommodityModel
                            {
                                Commodity = reader["commodity"].ToString(),
                                Commodity_Name = reader["commodity_name"].ToString()
                            });
                        }
                    }
                }
            }
            return dataList;
        }

        // Fungsi Update Commodity
        public bool UpdateCategory(string id_old, string id_new, string name)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                if (id_old != id_new)
                {
                    string checkQuery = "SELECT COUNT(1) FROM mst_commodity WHERE commodity = @id_new";
                    using (SqlCommand cmdCheck = new SqlCommand(checkQuery, conn))
                    {
                        cmdCheck.Parameters.AddWithValue("@id_new", id_new);
                        if ((int)cmdCheck.ExecuteScalar() > 0)
                        {
                            return false;
                        }
                    }
                }

                string query = @"UPDATE mst_commodity 
                         SET commodity = @id_new, commodity_name = @name 
                         WHERE commodity = @id_old";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id_old", id_old);
                    cmd.Parameters.AddWithValue("@id_new", id_new);
                    cmd.Parameters.AddWithValue("@name", name);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // Fungsi untuk mengambil semua data part number / unique reff
        public List<PartListModel> GetPartNumberData()
        {
            List<PartListModel> dataList = new List<PartListModel>();
            string query = "SELECT id_part, part_number FROM mst_part_number ORDER BY part_number ASC";

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dataList.Add(new PartListModel
                            {
                                Part_ID = Convert.ToInt32(reader["id_part"]),
                                Part_Number = reader["part_number"].ToString(),
                            });
                        }
                    }
                }
            }
            return dataList;
        }

        // Fungsi Update Part Number / Unique Reff
        public bool UpdatePartNumber(int id, string number)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "UPDATE mst_part_number SET part_number = @number WHERE id_part = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@number", number);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // Fungsi untuk mengambil semua data issue category
        public List<IssueCategoryModel> GetIssueCategoryData()
        {
            List<IssueCategoryModel> dataList = new List<IssueCategoryModel>();
            string query = "SELECT id_issue, issue_name FROM mst_issue_category ORDER BY issue_name ASC";

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dataList.Add(new IssueCategoryModel
                            {
                                id_issue = reader["id_issue"].ToString(),
                                issue_name = reader["issue_name"].ToString()
                            });
                        }
                    }
                }
            }
            return dataList;
        }

        // Fungsi untuk mengambil data dari library defect untuk ditampilkan di DataTable
        public List<DefectLibraryModel> GetDefectLibrary()
        {
            var defects = new List<DefectLibraryModel>();
            string query = "SELECT defect_code, defect_name, defect_picture, record_date FROM mst_defect_library ORDER BY defect_code ASC";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                defects.Add(new DefectLibraryModel
                                {
                                    defect_code = reader["defect_code"]?.ToString(),
                                    defect_name = reader["defect_name"]?.ToString(),
                                    defect_picture = reader["defect_picture"]?.ToString(),
                                    record_date = reader["record_date"] != DBNull.Value
                                                  ? Convert.ToDateTime(reader["record_date"])
                                                  : (DateTime?)null
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error GetDefectLibrary: {ex.Message}");
                    }
                }
            }
            return defects;
        }

        public List<CapabilityModel> GetCapability()
        {
            List<CapabilityModel> list = new List<CapabilityModel>();
            string query = "SELECT id_capability, capability_name, capability_picture FROM mst_capability ORDER BY id_capability ASC";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new CapabilityModel
                            {
                                id_capability = reader["id_capability"].ToString(),
                                capability_name = reader["capability_name"].ToString(),
                                capability_picture = reader["capability_picture"].ToString()
                            });
                        }
                    }
                }
            }
            return list;
        }

        // Fungsi untuk mengambil daftar lokasi dari tabel mst_dc_th
        public List<string> GetThLocations()
        {
            List<string> locations = new List<string>();
            string query = "SELECT DISTINCT location FROM mst_dc_th ORDER BY location ASC";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            locations.Add(reader["location"].ToString());
                        }
                    }
                }
            }
            return locations;
        }
    }
}