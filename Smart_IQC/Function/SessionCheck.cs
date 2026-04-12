using Microsoft.AspNetCore.Mvc;

namespace P1F_IQC.Function
{
    public static class SessionCheck
    {
        public static IActionResult CheckSession(this Controller controller, Func<IActionResult> action)
        {
            var session = controller.HttpContext.Session;

            //System.Diagnostics.Debug.WriteLine(controller.GetType().Name);
            //System.Diagnostics.Debug.WriteLine(session.GetString("level"));
            string controllerName = controller.GetType().Name.Replace("Controller", "").ToLower();
            string sessionLevel = (session.GetString("level") ?? "").ToLower();
            string sessionRole = (session.GetString("role") ?? "").ToLower();
            //System.Diagnostics.Debug.WriteLine(controller1);
            //System.Diagnostics.Debug.WriteLine(controller2);

            //if (session == null || !session.TryGetValue("sesa_id", out _) || controllerName != sessionLevel)
            //{   
            //    if(controllerName == "finance" && sessionRole == "fbp")
            //    {
            //        return action();
            //    }
            //    else
            //    {
            //        return controller.RedirectToAction("Index", "Login");
            //    }
            //}
            return action();
        }
    }
}
