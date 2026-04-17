using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Smart_IQC.Function;
using Smart_IQC.Models;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.Data.SqlClient;

namespace Smart_IQC.Controllers
{
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Open()
        {
            string user_level = User.FindFirst("Smart_IQC_level")?.Value;
            if (user_level != null && user_level.ToLower() != "no_access")
            {
                switch (user_level.ToLower())
                {
                    case "admin":
                        return RedirectToAction("Dash", "Home");
                    case "inspector":
                        return RedirectToAction("Dash", "Home");
                    default:
                        return RedirectToAction("Dash", "Home");
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel user)
        {
            var db = new Function.DatabaseAccessLayer();
            string connectionString = db.ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                var authTool = new Function.Authentication();
                string passwordHash = authTool.MD5Hash(user.password);

                string query = "SELECT * FROM mst_users WHERE sesa_id = @sesa_id AND password = @password";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@sesa_id", user.sesa_id);
                cmd.Parameters.AddWithValue("@password", passwordHash);

                await conn.OpenAsync();
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                if (!reader.HasRows)
                {
                    return Json(new { success = false, message = "SESA ID or Password is incorrect!" });
                }
                reader.Close();

                var userDetail = db.GetUserDetail(user.sesa_id).FirstOrDefault();
                if (userDetail == null)
                {
                    return Json(new { success = false, message = "User details not found!" });
                }

                var claimsIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userDetail.sesa_id));
                claimsIdentity.AddClaim(new Claim("Smart_IQC_name", userDetail.name));
                claimsIdentity.AddClaim(new Claim("Smart_IQC_level", string.IsNullOrEmpty(userDetail.level) ? "no_access" : userDetail.level.ToLower()));
                claimsIdentity.AddClaim(new Claim("Smart_IQC_role", userDetail.role ?? ""));
                claimsIdentity.AddClaim(new Claim("Smart_IQC_apps", userDetail.apps_id ?? ""));

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                return Json(new { success = true, redirectUrl = Url.Action("Open", "Home") });
            }
        }

        [Authorize]
        public IActionResult Dash()
        {
            string level = User.FindFirst("Smart_IQC_level")?.Value;

            if (string.IsNullOrEmpty(level) || level.ToLower() == "no_access")
            {
                TempData["MessageSSO"] = "You don't have access! <br> Please contact admin for support.";
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        public IActionResult Unauthorize()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RefreshSession()
        {
            // Logika untuk memperbarui session agar tidak hangus
            HttpContext.Session.SetString("LastActivity", DateTime.Now.ToString());

            // Mengembalikan respon sukses ke AJAX
            return Json(new { success = true });
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            HttpContext.Session.Clear();

            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}